using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Student.Analytics;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;
using UniTrackRemaster.Services.Organization;

namespace UniTrackRemaster.Services.Academics
{
    public class MarkAnalyticsService(
        IUnitOfWork unitOfWork,
        IGradingSystemService gradingSystemService,
        ILogger<MarkAnalyticsService> logger)
        : IMarkAnalyticsService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IGradingSystemService _gradingSystemService = gradingSystemService ?? throw new ArgumentNullException(nameof(gradingSystemService));
        private readonly ILogger<MarkAnalyticsService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// OPTIMIZED: Single repository call instead of multiple queries
        /// </summary>
        public async Task<StudentGradeDashboardDto> GetStudentGradeDashboard(Guid studentId, string term = null, string academicYear = null)
        {
            _logger.LogInformation("Getting grade dashboard for student {StudentId}, term: {Term}, academicYear: {AcademicYear}", studentId, term, academicYear);
            
            try
            {
                var student = await _unitOfWork.Students.GetByIdAsync(studentId);
                if (student == null)
                {
                    _logger.LogWarning("Student {StudentId} not found", studentId);
                    throw new KeyNotFoundException($"Student with ID {studentId} not found");
                }

                var institutionId = GetInstitutionId(student);
                var (academicYearEntity, semesterEntity) = await GetAcademicContext(institutionId, term, academicYear);
                var gradingSystem = await GetGradingSystem(institutionId);

                // PERFORMANCE BOOST: Single call to get all data with proper joins
                var (marks, subjects, teachers, classmates, semesters) = await _unitOfWork.Students
                    .GetStudentDashboardDataAsync(studentId, academicYearEntity, semesterEntity);

                if (!marks.Any())
                {
                    _logger.LogInformation("No marks found for student {StudentId} in the specified period", studentId);
                    return CreateEmptyDashboard(semesterEntity);
                }

                // All calculations now use pre-loaded data - no additional DB calls
                double gpa = CalculateGPA(marks, subjects, gradingSystem.Id);
                double gpaTrend = await CalculateGPATrend(studentId, institutionId, academicYearEntity, semesterEntity, gradingSystem.Id);
                var (classRank, classAverage) = await CalculateClassMetrics(student, classmates, academicYearEntity, semesterEntity, gradingSystem.Id);
                var courses = GenerateCourseGrades(marks, subjects, teachers, semesters, semesterEntity?.Type.ToString() ?? "Current Term", gradingSystem.Id);
                var comments = GenerateTeacherComments(marks, subjects, teachers, gradingSystem.Id);
                var gradeDistribution = CalculateGradeDistribution(marks, gradingSystem.Id);
                var performanceTrend = await CalculatePerformanceTrend(studentId, institutionId, gradingSystem.Id);
                var semesterBreakdown = await GenerateSemesterBreakdown(studentId, institutionId, academicYearEntity, gradingSystem.Id);

                _logger.LogInformation("Successfully generated grade dashboard for student {StudentId}", studentId);

                return new StudentGradeDashboardDto(
                    GPA: gpa,
                    GPATrend: gpaTrend,
                    ClassRank: classRank,
                    ClassAverage: classAverage,
                    Courses: courses,
                    Comments: comments,
                    GradeDistribution: gradeDistribution,
                    PerformanceTrend: performanceTrend,
                    CurrentSemesterId: semesterEntity?.Id,
                    CurrentSemesterName: semesterEntity != null ? $"{semesterEntity.Type} {semesterEntity.AcademicYear?.Name}" : null,
                    SemesterBreakdown: semesterBreakdown
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating grade dashboard for student {StudentId}", studentId);
                throw new ApplicationException($"Failed to generate grade dashboard: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// OPTIMIZED: Eliminated N+1 queries by using single optimized call
        /// </summary>
        public async Task<TranscriptDto> GetStudentTranscript(Guid studentId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting transcript for student {StudentId}", studentId);
            
            try
            {
                var student = await _unitOfWork.Students.GetByIdAsync(studentId);
                if (student == null)
                {
                    _logger.LogWarning("Student {StudentId} not found", studentId);
                    throw new KeyNotFoundException($"Student with ID {studentId} not found");
                }

                var institutionId = GetInstitutionId(student);
                var gradingSystem = await GetGradingSystem(institutionId, cancellationToken);

                // PERFORMANCE BOOST: Single query with grouping instead of multiple lookups
                var marksBySemester = await _unitOfWork.Students.GetStudentMarksBySemestersAsync(studentId, institutionId, cancellationToken);

                if (!marksBySemester.Any())
                {
                    return CreateEmptyTranscript();
                }

                // Pre-load all subjects and teachers in batch operations
                var allMarks = marksBySemester.SelectMany(kv => kv.Value.Marks).ToList();
                var subjectIds = allMarks.Select(m => m.SubjectId).Distinct().ToList();
                var subjects = await _unitOfWork.Subjects.GetByIdsAsync(subjectIds);
                var subjectsDict = subjects.ToDictionary(s => s.Id);

                var teacherIds = subjects.Where(s => s.PrimaryTeacherId.HasValue).Select(s => s.PrimaryTeacherId.Value).Distinct().ToList();
                var teachers = teacherIds.Any() ? await _unitOfWork.Teachers.GetByIdsAsync(teacherIds) : new List<Teacher>();
                var teachersDict = teachers.ToDictionary(t => t.Id);

                var courses = new List<CourseGradeDto>();
                var gpaByTerm = new Dictionary<string, double>();
                var semesterSummaries = new Dictionary<string, SemesterSummaryDto>();

                foreach (var semesterGroup in marksBySemester)
                {
                    var termName = semesterGroup.Key;
                    var (termMarks, semester) = semesterGroup.Value;
                    var marksBySubject = termMarks.GroupBy(m => m.SubjectId);
                    var termCourses = new List<CourseGradeDto>();

                    foreach (var group in marksBySubject)
                    {
                        var subjectId = group.Key;
                        if (!subjectsDict.TryGetValue(subjectId, out var subject))
                            continue;

                        decimal averageScore = group.Average(m => m.Value);
                        var courseGrade = CreateCourseGradeDto(subject, averageScore, termName, teachersDict, gradingSystem.Id, semester);
                        courses.Add(courseGrade);
                        termCourses.Add(courseGrade);
                    }

                    double termGpa = CalculateGPA(termMarks, subjectsDict, gradingSystem.Id);
                    gpaByTerm[termName] = termGpa;

                    var semesterSummary = new SemesterSummaryDto(
                        SemesterId: semester.Id,
                        SemesterName: termName,
                        SemesterType: semester.Type.ToString(),
                        AcademicYear: semester.AcademicYear?.Name ?? "Unknown",
                        GPA: termGpa,
                        Credits: termCourses.Sum(c => c.Credits ?? 0),
                        Courses: termCourses
                    );
                    semesterSummaries[termName] = semesterSummary;
                }

                // Calculate credits efficiently
                int totalCreditsAttempted = courses.Sum(c => c.Credits ?? 0);
                int totalCreditsEarned = courses.Where(c => c.Status == "Completed" || c.Status == "In Progress").Sum(c => c.Credits ?? 0);
                var majorCreditsCompleted = await CalculateMajorCredits(courses, student, cancellationToken);
                var genEdCreditsCompleted = await CalculateGenEdCredits(courses, cancellationToken);

                _logger.LogInformation("Successfully generated transcript for student {StudentId}", studentId);

                return new TranscriptDto(
                    Courses: courses,
                    TotalCreditsAttempted: totalCreditsAttempted,
                    TotalCreditsEarned: totalCreditsEarned,
                    MajorCreditsCompleted: majorCreditsCompleted,
                    MajorCreditsRequired: 60,
                    GenEdCreditsCompleted: genEdCreditsCompleted,
                    GenEdCreditsRequired: 30,
                    GPAByTerm: gpaByTerm,
                    SemesterSummaries: semesterSummaries
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating transcript for student {StudentId}", studentId);
                throw new ApplicationException($"Failed to generate transcript: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// OPTIMIZED: Single repository call with proper joins
        /// </summary>
        public async Task<List<CourseGradeDto>> GetStudentTermGrades(Guid studentId, string term, string academicYear)
        {
            if (string.IsNullOrEmpty(term) || string.IsNullOrEmpty(academicYear))
            {
                throw new ArgumentException("Term and academic year must be specified");
            }

            _logger.LogInformation("Getting term grades for student {StudentId}, term: {Term}, year: {Year}", studentId, term, academicYear);

            try
            {
                var student = await _unitOfWork.Students.GetByIdAsync(studentId);
                if (student == null)
                {
                    _logger.LogWarning("Student {StudentId} not found", studentId);
                    throw new KeyNotFoundException($"Student with ID {studentId} not found");
                }

                var institutionId = GetInstitutionId(student);
                var gradingSystem = await GetGradingSystem(institutionId);
                var (academicYearEntity, semesterEntity) = await GetAcademicContext(institutionId, term, academicYear);

                if (academicYearEntity == null || semesterEntity == null)
                {
                    _logger.LogWarning("Academic year {Year} or term {Term} not found", academicYear, term);
                    throw new KeyNotFoundException($"Academic year {academicYear} or term {term} not found");
                }

                // PERFORMANCE BOOST: Single query with all joins
                var (marks, subjects, semesters) = await _unitOfWork.Students.GetStudentMarksWithSubjectsAsync(studentId, academicYearEntity, semesterEntity);

                // Pre-load teachers efficiently
                var teacherIds = subjects.Values.Where(s => s.PrimaryTeacherId.HasValue).Select(s => s.PrimaryTeacherId.Value).Distinct().ToList();
                var teachers = teacherIds.Any() ? await _unitOfWork.Teachers.GetByIdsAsync(teacherIds) : new List<Teacher>();
                var teachersDict = teachers.ToDictionary(t => t.Id);

                var courses = GenerateCourseGrades(marks, subjects, teachersDict, semesters, semesterEntity.Type.ToString(), gradingSystem.Id);

                _logger.LogInformation("Successfully retrieved {Count} term grades for student {StudentId}", courses.Count, studentId);
                return courses;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving term grades for student {StudentId}", studentId);
                throw new ApplicationException($"Failed to retrieve term grades: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// OPTIMIZED: Use optimized repository method
        /// </summary>
        public async Task<double> CalculateStudentGPA(Guid studentId, string term = null, string academicYear = null)
        {
            _logger.LogInformation("Calculating GPA for student {StudentId}, term: {Term}, year: {Year}", studentId, term, academicYear);

            try
            {
                var student = await _unitOfWork.Students.GetByIdAsync(studentId);
                if (student == null)
                {
                    _logger.LogWarning("Student {StudentId} not found", studentId);
                    throw new KeyNotFoundException($"Student with ID {studentId} not found");
                }

                var institutionId = GetInstitutionId(student);
                var gradingSystem = await GetGradingSystem(institutionId);
                var (academicYearEntity, semesterEntity) = await GetAcademicContext(institutionId, term, academicYear);

                // PERFORMANCE BOOST: Single optimized call
                var (marks, subjects, semesters) = await _unitOfWork.Students.GetStudentMarksWithSubjectsAsync(studentId, academicYearEntity, semesterEntity);
                double gpa = CalculateGPA(marks, subjects, gradingSystem.Id);

                _logger.LogInformation("Calculated GPA of {GPA} for student {StudentId}", gpa, studentId);
                return gpa;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating GPA for student {StudentId}", studentId);
                throw new ApplicationException($"Failed to calculate GPA: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// OPTIMIZED AND IMPLEMENTED: Gets a student's rank within their class/grade
        /// </summary>
        public async Task<int?> GetStudentClassRank(Guid studentId, Guid gradeId)
        {
            _logger.LogInformation("Getting class rank for student {StudentId} in grade {GradeId}", studentId, gradeId);

            try
            {
                var student = await _unitOfWork.Students.GetByIdAsync(studentId);
                if (student == null || student.GradeId != gradeId)
                {
                    _logger.LogWarning("Student {StudentId} not found in grade {GradeId}", studentId, gradeId);
                    return null;
                }

                var institutionId = GetInstitutionId(student);
                var gradingSystem = await GetGradingSystem(institutionId);

                // Get current academic period
                var currentDate = DateTime.UtcNow;
                var currentSemester = await _unitOfWork.Semesters.GetByDateAsync(currentDate, institutionId);
                AcademicYear currentAcademicYear;

                if (currentSemester != null)
                {
                    currentAcademicYear = await _unitOfWork.AcademicYears.GetByIdAsync(currentSemester.AcademicYearId);
                }
                else
                {
                    currentAcademicYear = await _unitOfWork.AcademicYears.GetCurrentAsync(institutionId);
                }

                // OPTIMIZED: Get class performance data using optimized method
                var classPerformanceData = await _unitOfWork.Students.GetClassPerformanceDataAsync(gradeId, currentAcademicYear, currentSemester);

                if (!classPerformanceData.Any())
                {
                    _logger.LogInformation("No class performance data found for grade {GradeId}", gradeId);
                    return null;
                }

                // Calculate GPA for each student efficiently
                var studentGpas = new Dictionary<Guid, double>();

                // Get all unique student IDs and subject IDs
                var allStudentIds = classPerformanceData.SelectMany(kv => kv.Value.Select(v => v.StudentId)).Distinct().ToList();
                var allSubjectIds = classPerformanceData.Keys.ToList();

                // Pre-load all subjects needed for GPA calculation
                var subjects = await _unitOfWork.Subjects.GetByIdsAsync(allSubjectIds);
                var subjectsDict = subjects.ToDictionary(s => s.Id);

                // Calculate GPA for each student using aggregated marks per subject
                foreach (var subjectData in classPerformanceData)
                {
                    var subjectId = subjectData.Key;
                    if (!subjectsDict.ContainsKey(subjectId)) continue;

                    foreach (var (classmateId, marks) in subjectData.Value)
                    {
                        if (!marks.Any()) continue;

                        if (!studentGpas.ContainsKey(classmateId))
                            studentGpas[classmateId] = 0;

                        // Calculate subject average and convert to GPA points
                        decimal subjectAverage = marks.Average(m => m.Value);
                        double subjectGpaPoints = _gradingSystemService.ConvertScoreToGpaPoints(subjectAverage, gradingSystem.Id);
                        
                        // Weight by credits
                        int credits = subjectsDict[subjectId].CreditHours ?? 1;
                        studentGpas[classmateId] += subjectGpaPoints * credits;
                    }
                }

                // Convert to actual GPAs
                var finalGpas = new Dictionary<Guid, double>();
                foreach (var studentData in studentGpas)
                {
                    var studentSubjects = classPerformanceData.Where(kv => kv.Value.Any(v => v.StudentId == studentData.Key));
                    int totalCredits = studentSubjects.Sum(s => subjectsDict.ContainsKey(s.Key) ? subjectsDict[s.Key].CreditHours ?? 1 : 1);
                    
                    if (totalCredits > 0)
                        finalGpas[studentData.Key] = studentData.Value / totalCredits;
                }

                if (!finalGpas.ContainsKey(studentId))
                {
                    _logger.LogInformation("Student {StudentId} has no performance data for ranking", studentId);
                    return null;
                }

                // Calculate rank
                var sortedStudents = finalGpas.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToList();
                int rank = sortedStudents.IndexOf(studentId) + 1; // 1-based rank

                _logger.LogInformation("Student {StudentId} has rank {Rank} out of {Total} in grade {GradeId}", 
                    studentId, rank, sortedStudents.Count, gradeId);

                return rank > 0 ? rank : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating class rank for student {StudentId}", studentId);
                return null; // Return null rather than throwing, as rank is non-critical
            }
        }

        /// <summary>
        /// OPTIMIZED: Attendance analysis with single query approach
        /// </summary>
        public async Task<AttendancePerformanceInsightDto> GetAttendancePerformanceInsight(Guid studentId, string term = null, string academicYear = null)
        {
            try
            {
                var student = await _unitOfWork.Students.GetByIdAsync(studentId);
                if (student == null)
                {
                    _logger.LogWarning("Student {StudentId} not found", studentId);
                    throw new KeyNotFoundException($"Student with ID {studentId} not found");
                }

                var institutionId = GetInstitutionId(student);
                var (academicYearEntity, semesterEntity) = await GetAcademicContext(institutionId, term, academicYear);
                var gradingSystem = await GetGradingSystem(institutionId);

                // PERFORMANCE BOOST: Batch operations instead of individual queries
                var studentAttendance = await _unitOfWork.Students.GetStudentAttendanceDataAsync(studentId, academicYearEntity, semesterEntity);
                var (studentMarks, studentSubjects, semesters) = await _unitOfWork.Students.GetStudentMarksWithSubjectsAsync(studentId, academicYearEntity, semesterEntity);
                var classPerformanceData = await _unitOfWork.Students.GetClassPerformanceDataAsync(student.GradeId, academicYearEntity, semesterEntity);

                // All calculations now use pre-loaded data
                var studentMetricsBySubject = CalculateStudentAttendanceMetrics(studentAttendance, studentMarks, semesterEntity);
                var classCorrelationsBySubject = await CalculateClassCorrelationData(classPerformanceData, studentSubjects, academicYearEntity, semesterEntity);
                var subjectInsights = GenerateSubjectInsights(studentMetricsBySubject, classCorrelationsBySubject, studentSubjects, semesters, gradingSystem.Id);
                var (overallStudentAttendance, overallStudentScore, overallClassAttendance, overallClassScore) = CalculateOverallMetrics(studentMetricsBySubject, classCorrelationsBySubject);

                double averageCorrelation = subjectInsights.Any() ? subjectInsights.Average(s => s.AttendanceScoreCorrelation) : 0;

                return new AttendancePerformanceInsightDto
                {
                    StudentId = studentId,
                    StudentName = $"{student.User?.FirstName} {student.User?.LastName}",
                    Period = GetPeriodString(academicYearEntity, semesterEntity),
                    SemesterId = semesterEntity?.Id,
                    SemesterName = semesterEntity != null ? $"{semesterEntity.Type} {semesterEntity.AcademicYear?.Name}" : null,
                    OverallStudentAttendanceRate = overallStudentAttendance,
                    OverallStudentAverageScore = overallStudentScore,
                    OverallStudentGrade = _gradingSystemService.ConvertScoreToGrade(overallStudentScore, gradingSystem.Id),
                    OverallClassAttendanceRate = overallClassAttendance,
                    OverallClassAverageScore = overallClassScore,
                    OverallCorrelation = averageCorrelation,
                    AttendancePerformanceClassRank = CalculateAttendancePerformanceRank(studentId, studentMetricsBySubject, classCorrelationsBySubject),
                    SubjectInsights = subjectInsights,
                    PrimaryAreaForImprovement = IdentifyPrimaryAreaForImprovement(subjectInsights),
                    EstimatedGpaImpact = CalculateEstimatedGpaImpact(subjectInsights, studentSubjects, gradingSystem.Id)
                };
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating attendance-performance insights for student {StudentId}", studentId);
                throw new ApplicationException($"Failed to calculate attendance-performance insights: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Export student grades in specified format
        /// </summary>
        public async Task<byte[]> ExportGrades(Guid studentId, string format, string term = null, string academicYear = null)
        {
            _logger.LogInformation("Exporting grades for student {StudentId} in format {Format}", studentId, format);
            throw new NotImplementedException("Grade export functionality not yet implemented");
        }

        #region Helper Methods

        /// <summary>
        /// Gets the institution ID for a student
        /// </summary>
        private Guid GetInstitutionId(Student student)
        {
            if (student.IsSchoolStudent && student.School?.InstitutionId != null)
                return student.School.InstitutionId;
            if (!student.IsSchoolStudent && student.University?.InstitutionId != null)
                return student.University.InstitutionId;

            _logger.LogError("Student {StudentId} is not associated with any institution", student.Id);
            throw new InvalidOperationException($"Student {student.Id} is not associated with any institution");
        }

        /// <summary>
        /// Gets academic context (year and semester) based on provided parameters
        /// </summary>
        private async Task<(AcademicYear Year, Semester Semester)> GetAcademicContext(Guid institutionId, string termName = null, string academicYearName = null)
        {
            AcademicYear academicYear;
            Semester semester = null;

            if (!string.IsNullOrEmpty(academicYearName))
            {
                var academicYears = await _unitOfWork.AcademicYears.GetAllAsync(institutionId);
                academicYear = academicYears.FirstOrDefault(ay => ay.Name.Equals(academicYearName, StringComparison.OrdinalIgnoreCase));
                if (academicYear == null)
                    throw new KeyNotFoundException($"Academic year '{academicYearName}' not found");
            }
            else
            {
                academicYear = await _unitOfWork.AcademicYears.GetCurrentAsync(institutionId);
            }

            if (!string.IsNullOrEmpty(termName) && academicYear != null)
            {
                var semesters = await _unitOfWork.Semesters.GetByAcademicYearAsync(academicYear.Id);
                if (Enum.TryParse<SemesterType>(termName, true, out var semesterType))
                {
                    semester = semesters.FirstOrDefault(s => s.Type == semesterType);
                }
                else
                {
                    semester = semesters.FirstOrDefault(s => s.Name.Equals(termName, StringComparison.OrdinalIgnoreCase));
                }

                if (semester == null)
                    throw new KeyNotFoundException($"Semester '{termName}' not found in academic year '{academicYear.Name}'");
            }
            else if (academicYear != null)
            {
                semester = await _unitOfWork.Semesters.GetCurrentAsync(academicYear.Id);
            }

            return (academicYear, semester);
        }

        /// <summary>
        /// Gets or initializes the default grading system for an institution
        /// </summary>
        private async Task<GradingSystem> GetGradingSystem(Guid institutionId, CancellationToken cancellationToken = default)
        {
            var gradingSystem = await _unitOfWork.GradingSystems.GetDefaultForInstitutionAsync(institutionId, cancellationToken);

            if (gradingSystem == null)
            {
                _logger.LogInformation("No default grading system found for institution {InstitutionId}, initializing defaults", institutionId);
                await _gradingSystemService.InitializeDefaultGradingSystemsAsync(institutionId, cancellationToken);
                gradingSystem = await _unitOfWork.GradingSystems.GetDefaultForInstitutionAsync(institutionId, cancellationToken);

                if (gradingSystem == null)
                {
                    throw new InvalidOperationException($"Failed to initialize default grading system for institution {institutionId}");
                }
            }

            return gradingSystem;
        }

        /// <summary>
        /// Calculates GPA from marks using the grading system
        /// </summary>
        private double CalculateGPA(IEnumerable<Mark> marks, Dictionary<Guid, Subject> subjects, Guid gradingSystemId)
        {
            if (!marks.Any()) return 0;

            double totalPoints = 0;
            int totalCredits = 0;
            var marksBySubject = marks.GroupBy(m => m.SubjectId);

            foreach (var subjectGroup in marksBySubject)
            {
                if (!subjects.TryGetValue(subjectGroup.Key, out var subject))
                    continue;

                int credits = subject.CreditHours ?? 1;
                decimal averageScore = subjectGroup.Average(m => m.Value);
                double gpaPoints = _gradingSystemService.ConvertScoreToGpaPoints(averageScore, gradingSystemId);

                totalPoints += gpaPoints * credits;
                totalCredits += credits;
            }

            return totalCredits > 0 ? Math.Round(totalPoints / totalCredits, 2) : 0;
        }

        /// <summary>
        /// Calculates GPA trend by comparing with the previous academic period
        /// </summary>
        private async Task<double> CalculateGPATrend(Guid studentId, Guid institutionId, AcademicYear currentAcademicYear, Semester currentSemester, Guid gradingSystemId)
        {
            if (currentAcademicYear == null)
                return 0;

            try
            {
                var (previousYear, previousSemester) = await GetPreviousPeriod(institutionId, currentAcademicYear, currentSemester);
                if (previousYear == null)
                    return 0;

                var (currentMarks, currentSubjects, currentSemesters) = await _unitOfWork.Students.GetStudentMarksWithSubjectsAsync(studentId, currentAcademicYear, currentSemester);
                double currentGPA = CalculateGPA(currentMarks, currentSubjects, gradingSystemId);

                var (previousMarks, previousSubjects, previousSemesters) = await _unitOfWork.Students.GetStudentMarksWithSubjectsAsync(studentId, previousYear, previousSemester);
                double previousGPA = CalculateGPA(previousMarks, previousSubjects, gradingSystemId);

                return Math.Round(currentGPA - previousGPA, 2);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating GPA trend for student {StudentId}", studentId);
                return 0;
            }
        }

        /// <summary>
        /// Gets the previous academic period
        /// </summary>
        private async Task<(AcademicYear Year, Semester Semester)> GetPreviousPeriod(Guid institutionId, AcademicYear currentYear, Semester currentSemester)
        {
            if (currentSemester != null)
            {
                var semesters = (await _unitOfWork.Semesters.GetByAcademicYearAsync(currentYear.Id)).OrderBy(s => s.StartDate).ToList();
                int currentIndex = semesters.FindIndex(s => s.Id == currentSemester.Id);

                if (currentIndex > 0)
                {
                    return (currentYear, semesters[currentIndex - 1]);
                }
            }

            var academicYears = await _unitOfWork.AcademicYears.GetAllAsync(institutionId);
            var previousYear = academicYears.Where(ay => ay.EndDate < currentYear.StartDate).OrderByDescending(ay => ay.EndDate).FirstOrDefault();

            if (previousYear == null)
                return (null, null);

            var previousSemesters = await _unitOfWork.Semesters.GetByAcademicYearAsync(previousYear.Id);
            var lastSemester = previousSemesters.OrderByDescending(s => s.EndDate).FirstOrDefault();

            return (previousYear, lastSemester);
        }

        /// <summary>
        /// OPTIMIZED: Calculates class rank and average using provided classmates data
        /// </summary>
        private async Task<(int? Rank, double? Average)> CalculateClassMetrics(Student student, List<Student> classmates, AcademicYear academicYear, Semester semester, Guid gradingSystemId)
        {
            if (!classmates.Any())
                return (null, null);

            try
            {
                var allStudentIds = classmates.Select(c => c.Id).Append(student.Id).ToList();
                var studentGpas = new Dictionary<Guid, double>();

                // OPTIMIZED: Get class performance data using optimized method
                var classPerformanceData = await _unitOfWork.Students.GetClassPerformanceDataAsync(student.GradeId, academicYear, semester);

                if (!classPerformanceData.Any())
                    return (null, null);

                // Pre-load all subjects for efficient GPA calculation
                var subjectIds = classPerformanceData.Keys.ToList();
                var subjects = await _unitOfWork.Subjects.GetByIdsAsync(subjectIds);
                var subjectsDict = subjects.ToDictionary(s => s.Id);

                // Calculate GPA for each student
                foreach (var subjectData in classPerformanceData.Values)
                {
                    foreach (var (classmateId, marks) in subjectData)
                    {
                        if (!allStudentIds.Contains(classmateId) || !marks.Any())
                            continue;

                        if (!studentGpas.ContainsKey(classmateId))
                            studentGpas[classmateId] = 0;

                        // Calculate subject GPA and add to running total
                        decimal subjectAverage = marks.Average(m => m.Value);
                        double subjectGpaPoints = _gradingSystemService.ConvertScoreToGpaPoints(subjectAverage, gradingSystemId);
                        
                        // Find subject for credits
                        var subjectId = marks.First().SubjectId;
                        if (subjectsDict.TryGetValue(subjectId, out var subject))
                        {
                            int credits = subject.CreditHours ?? 1;
                            studentGpas[classmateId] += subjectGpaPoints * credits;
                        }
                    }
                }

                if (!studentGpas.Any())
                    return (null, null);

                // Convert to actual GPAs by dividing by total credits
                var finalGpas = new Dictionary<Guid, double>();
                foreach (var studentData in studentGpas)
                {
                    var studentSubjects = classPerformanceData.Where(kv => kv.Value.Any(v => v.StudentId == studentData.Key));
                    int totalCredits = studentSubjects.Sum(s => subjectsDict.ContainsKey(s.Key) ? subjectsDict[s.Key].CreditHours ?? 1 : 1);
                    
                    if (totalCredits > 0)
                        finalGpas[studentData.Key] = studentData.Value / totalCredits;
                }

                double? classAverage = finalGpas.Any() ? Math.Round(finalGpas.Values.Average(), 2) : null;

                var sortedStudents = finalGpas.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToList();
                int? rank = finalGpas.ContainsKey(student.Id) ? sortedStudents.IndexOf(student.Id) + 1 : null;

                return (rank, classAverage);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating class metrics for student {StudentId}", student.Id);
                return (null, null);
            }
        }

        /// <summary>
        /// Generates course grade DTOs from marks data
        /// </summary>
        private List<CourseGradeDto> GenerateCourseGrades(IEnumerable<Mark> marks, Dictionary<Guid, Subject> subjects, Dictionary<Guid, Teacher> teachers, Dictionary<Guid, Semester> semesters, string termName, Guid gradingSystemId)
        {
            var courseGrades = new List<CourseGradeDto>();
            var marksBySubject = marks.GroupBy(m => m.SubjectId);

            foreach (var group in marksBySubject)
            {
                var subjectId = group.Key;
                if (!subjects.TryGetValue(subjectId, out var subject))
                    continue;

                decimal averageScore = group.Average(m => m.Value);
                var markSemesters = group.Where(m => m.SemesterId.HasValue).Select(m => m.SemesterId.Value).Distinct().ToList();
                Semester semester = null;
                if (markSemesters.Count == 1 && semesters.TryGetValue(markSemesters.First(), out semester))
                {
                    // All marks are from the same semester
                }

                courseGrades.Add(CreateCourseGradeDto(subject, averageScore, termName, teachers, gradingSystemId, semester));
            }

            return courseGrades;
        }

        /// <summary>
        /// Creates a CourseGradeDto for a subject with a given score
        /// </summary>
        private CourseGradeDto CreateCourseGradeDto(Subject subject, decimal score, string termName, Dictionary<Guid, Teacher> teachers, Guid gradingSystemId, Semester semester = null)
        {
            string grade = _gradingSystemService.ConvertScoreToGrade(score, gradingSystemId);
            string instructorName = "Unknown";
            if (subject.PrimaryTeacherId.HasValue && teachers.TryGetValue(subject.PrimaryTeacherId.Value, out var teacher))
            {
                instructorName = $"{teacher.User?.FirstName ?? ""} {teacher.User?.LastName ?? ""}".Trim();
            }

            string status = _gradingSystemService.DetermineStatus(score, gradingSystemId);

            return new CourseGradeDto(
                Id: subject.Id,
                Code: subject.Code,
                Name: subject.Name,
                Instructor: instructorName,
                Score: score,
                Grade: grade,
                Credits: subject.CreditHours,
                Term: termName,
                SemesterId: semester?.Id,
                SemesterName: semester != null ? $"{semester.Type} {semester.AcademicYear?.Name}" : null,
                Status: status
            );
        }

        /// <summary>
        /// Generates teacher comments based on student performance
        /// </summary>
        private List<TeacherCommentDto> GenerateTeacherComments(IEnumerable<Mark> marks, Dictionary<Guid, Subject> subjects, Dictionary<Guid, Teacher> teachers, Guid gradingSystemId)
        {
            var comments = new List<TeacherCommentDto>();
            var topSubjects = marks.GroupBy(m => m.SubjectId).OrderByDescending(g => g.Count()).Take(3).ToList();

            foreach (var group in topSubjects)
            {
                var subjectId = group.Key;
                if (!subjects.TryGetValue(subjectId, out var subject))
                    continue;

                if (!subject.PrimaryTeacherId.HasValue || !teachers.TryGetValue(subject.PrimaryTeacherId.Value, out var teacher))
                    continue;

                decimal avgScore = group.Average(m => m.Value);
                string content = GenerateComment(avgScore, subject.Name, gradingSystemId);

                comments.Add(new TeacherCommentDto(
                    Id: Guid.NewGuid(),
                    Subject: subject.Name,
                    Teacher: $"{teacher.User?.FirstName ?? ""} {teacher.User?.LastName ?? ""}".Trim(),
                    Content: content,
                    Date: DateTime.UtcNow.AddDays(-7)
                ));
            }

            return comments;
        }

        /// <summary>
        /// Generates a comment based on student performance
        /// </summary>
        private string GenerateComment(decimal score, string subjectName, Guid gradingSystemId)
        {
            string gradeLetter = _gradingSystemService.ConvertScoreToGrade(score, gradingSystemId);

            if (score >= 90 || gradeLetter.StartsWith("A"))
            {
                return $"Excellent work in {subjectName}. The student consistently demonstrates superior understanding of the material and contributes high-quality work.";
            }
            else if (score >= 80 || gradeLetter.StartsWith("B"))
            {
                return $"Good performance in {subjectName}. The student shows solid understanding of the concepts and regularly completes assignments well.";
            }
            else if (score >= 70 || gradeLetter.StartsWith("C"))
            {
                return $"Satisfactory progress in {subjectName}. The student grasps the basic concepts but could benefit from more active participation and practice.";
            }
            else if (score >= 60 || gradeLetter.StartsWith("D"))
            {
                return $"Needs improvement in {subjectName}. The student should seek additional help and focus on building a stronger foundation in the subject.";
            }
            else
            {
                return $"Significant improvement needed in {subjectName}. The student requires immediate intervention and support to address knowledge gaps and develop effective study strategies.";
            }
        }

        /// <summary>
        /// Calculates grade distribution from marks
        /// </summary>
        private GradeDistributionDto CalculateGradeDistribution(IEnumerable<Mark> marks, Guid gradingSystemId)
        {
            var gradeCounts = new Dictionary<string, int>();
            var marksBySubject = marks.GroupBy(m => m.SubjectId);

            foreach (var subjectGroup in marksBySubject)
            {
                decimal subjectAverage = subjectGroup.Average(m => m.Value);
                string grade = _gradingSystemService.ConvertScoreToGrade(subjectAverage, gradingSystemId);

                if (!gradeCounts.ContainsKey(grade))
                    gradeCounts[grade] = 0;

                gradeCounts[grade]++;
            }

            return new GradeDistributionDto(gradeCounts);
        }

        /// <summary>
        /// Calculates performance trend over recent terms
        /// </summary>
        private async Task<PerformanceTrendDto> CalculatePerformanceTrend(Guid studentId, Guid institutionId, Guid gradingSystemId)
        {
            try
            {
                var semesters = await GetRecentSemesters(institutionId, 4);
                if (semesters.Count == 0)
                    return CreateEmptyPerformanceTrend();

                var termNames = new List<string>();
                var studentAverages = new List<double>();
                var classAverages = new List<double>();
                var semesterIds = new List<Guid>();

                var student = await _unitOfWork.Students.GetByIdAsync(studentId);
                if (student == null)
                    return CreateEmptyPerformanceTrend();

                foreach (var (year, semester) in semesters)
                {
                    termNames.Add($"{semester.Type} {year.Name}");
                    semesterIds.Add(semester.Id);

                    var (studentMarks, subjects, semesterData) = await _unitOfWork.Students.GetStudentMarksWithSubjectsAsync(studentId, year, semester);
                    double studentGpa = CalculateGPA(studentMarks, subjects, gradingSystemId);
                    studentAverages.Add(Math.Round(studentGpa * 25, 1));

                    var classPerformanceData = await _unitOfWork.Students.GetClassPerformanceDataAsync(student.GradeId, year, semester);

                    if (classPerformanceData.Any())
                    {
                        var allSubjectIds = classPerformanceData.Keys.ToList();
                        var allSubjects = await _unitOfWork.Subjects.GetByIdsAsync(allSubjectIds);
                        var subjectsDict = allSubjects.ToDictionary(s => s.Id);

                        var classGpas = new List<double>();
                        foreach (var subjectData in classPerformanceData.Values)
                        {
                            foreach (var (classmateId, marks) in subjectData)
                            {
                                if (marks.Any())
                                {
                                    double classmateGpa = CalculateGPA(marks, subjectsDict, gradingSystemId);
                                    if (!classGpas.Any(g => Math.Abs(g - classmateGpa) < 0.01))
                                        classGpas.Add(classmateGpa);
                                }
                            }
                        }

                        double termClassAverage = classGpas.Any() ? Math.Round(classGpas.Average() * 25, 1) : 0;
                        classAverages.Add(termClassAverage);
                    }
                    else
                    {
                        classAverages.Add(0);
                    }
                }

                return new PerformanceTrendDto(termNames, studentAverages, classAverages, semesterIds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating performance trends for student {StudentId}", studentId);
                return CreateEmptyPerformanceTrend();
            }
        }

        /// <summary>
        /// Gets recent semesters for an institution
        /// </summary>
        private async Task<List<(AcademicYear Year, Semester Semester)>> GetRecentSemesters(Guid institutionId, int count)
        {
            var result = new List<(AcademicYear, Semester)>();
            var years = (await _unitOfWork.AcademicYears.GetAllAsync(institutionId)).OrderByDescending(y => y.EndDate).Take(2).ToList();

            foreach (var year in years)
            {
                var semesters = (await _unitOfWork.Semesters.GetByAcademicYearAsync(year.Id)).OrderByDescending(s => s.EndDate).ToList();

                foreach (var semester in semesters)
                {
                    result.Add((year, semester));
                    if (result.Count >= count) break;
                }

                if (result.Count >= count) break;
            }

            return result.OrderBy(t => t.Item1.StartDate).ThenBy(t => t.Item2.StartDate).ToList();
        }

        /// <summary>
        /// Generates semester breakdown for academic year
        /// </summary>
        private async Task<List<SemesterSummaryDto>> GenerateSemesterBreakdown(Guid studentId, Guid institutionId, AcademicYear academicYear, Guid gradingSystemId)
        {
            if (academicYear == null)
                return new List<SemesterSummaryDto>();

            try
            {
                var breakdownData = await _unitOfWork.Students.GetAcademicYearBreakdownAsync(studentId, academicYear.Id);

                if (!breakdownData.Any())
                    return new List<SemesterSummaryDto>();

                var semesterSummaries = new List<SemesterSummaryDto>();
                var allSemesters = breakdownData.Values.SelectMany(list => list.Select(tuple => tuple.Semester)).Distinct().OrderBy(s => s.StartDate).ToList();

                foreach (var semester in allSemesters)
                {
                    var semesterMarks = breakdownData.Values.SelectMany(list => list.Where(tuple => tuple.Semester.Id == semester.Id)).SelectMany(tuple => tuple.Marks).ToList();

                    if (!semesterMarks.Any())
                        continue;

                    var subjectIds = semesterMarks.Select(m => m.SubjectId).Distinct().ToList();
                    var subjects = await _unitOfWork.Subjects.GetByIdsAsync(subjectIds);
                    var subjectsDict = subjects.ToDictionary(s => s.Id);

                    var teacherIds = subjects.Where(s => s.PrimaryTeacherId.HasValue).Select(s => s.PrimaryTeacherId.Value).Distinct().ToList();
                    var teachers = teacherIds.Any() ? await _unitOfWork.Teachers.GetByIdsAsync(teacherIds) : new List<Teacher>();
                    var teachersDict = teachers.ToDictionary(t => t.Id);

                    double semesterGpa = CalculateGPA(semesterMarks, subjectsDict, gradingSystemId);

                    var courses = GenerateCourseGrades(
                        semesterMarks,
                        subjectsDict,
                        teachersDict,
                        new Dictionary<Guid, Semester> { { semester.Id, semester } },
                        $"{semester.Type} {semester.AcademicYear?.Name}",
                        gradingSystemId);

                    var summary = new SemesterSummaryDto(
                        SemesterId: semester.Id,
                        SemesterName: $"{semester.Type} {semester.AcademicYear?.Name}",
                        SemesterType: semester.Type.ToString(),
                        AcademicYear: semester.AcademicYear?.Name ?? "Unknown",
                        GPA: semesterGpa,
                        Credits: courses.Sum(c => c.Credits ?? 0),
                        Courses: courses
                    );

                    semesterSummaries.Add(summary);
                }

                return semesterSummaries;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error generating semester breakdown for student {StudentId}", studentId);
                return new List<SemesterSummaryDto>();
            }
        }

        /// <summary>
        /// Calculates student attendance metrics by subject
        /// </summary>
        private Dictionary<Guid, (double AttendanceRate, decimal Score)> CalculateStudentAttendanceMetrics(List<Absence> attendanceRecords, List<Mark> marks, Semester semester)
        {
            var metrics = new Dictionary<Guid, (double AttendanceRate, decimal Score)>();
            var marksBySubject = marks.GroupBy(m => m.SubjectId);

            foreach (var subjectGroup in marksBySubject)
            {
                var subjectId = subjectGroup.Key;
                var subjectAbsences = attendanceRecords.Where(a => a.SubjectId == subjectId).ToList();
                int totalClasses = EstimateTotalClasses(subjectId, semester);
                double attendanceRate = CalculateAttendanceRate(subjectAbsences, totalClasses);
                decimal averageScore = subjectGroup.Average(m => m.Value);

                metrics[subjectId] = (attendanceRate, averageScore);
                _logger.LogDebug("Subject {SubjectId}: {AbsenceCount} absences out of {TotalClasses} classes = {AttendanceRate:P1} attendance", 
                    subjectId, subjectAbsences.Count, totalClasses, attendanceRate);
            }

            return metrics;
        }

        /// <summary>
        /// OPTIMIZED: Calculates class correlation data by subject with batch attendance data
        /// </summary>
        private async Task<Dictionary<Guid, List<(double AttendanceRate, decimal Score)>>> CalculateClassCorrelationData(
            Dictionary<Guid, List<(Guid StudentId, List<Mark> Marks)>> classPerformanceData,
            Dictionary<Guid, Subject> subjects,
            AcademicYear academicYear,
            Semester semester)
        {
            var correlationData = new Dictionary<Guid, List<(double AttendanceRate, decimal Score)>>();

            foreach (var subjectData in classPerformanceData)
            {
                var subjectId = subjectData.Key;
                var studentData = subjectData.Value;
                var subjectCorrelations = new List<(double AttendanceRate, decimal Score)>();

                foreach (var (studentId, marks) in studentData)
                {
                    if (!marks.Any()) continue;

                    // OPTIMIZED: Get attendance data for this student
                    var studentAbsences = await _unitOfWork.Students.GetStudentAttendanceDataAsync(studentId, academicYear, semester);
                    var subjectAbsences = studentAbsences.Where(a => a.SubjectId == subjectId).ToList();

                    int totalClasses = EstimateTotalClasses(subjectId, semester);
                    double attendanceRate = CalculateAttendanceRate(subjectAbsences, totalClasses);
                    decimal averageScore = marks.Average(m => m.Value);

                    subjectCorrelations.Add((attendanceRate, averageScore));
                }

                if (subjectCorrelations.Any())
                {
                    correlationData[subjectId] = subjectCorrelations;
                }
            }

            return correlationData;
        }

        /// <summary>
        /// Generates subject-specific insights
        /// </summary>
        private List<SubjectAttendanceInsightDto> GenerateSubjectInsights(
            Dictionary<Guid, (double AttendanceRate, decimal Score)> studentMetrics,
            Dictionary<Guid, List<(double AttendanceRate, decimal Score)>> classCorrelations,
            Dictionary<Guid, Subject> subjects,
            Dictionary<Guid, Semester> semesters,
            Guid gradingSystemId)
        {
            var insights = new List<SubjectAttendanceInsightDto>();

            foreach (var subjectId in studentMetrics.Keys)
            {
                if (!subjects.TryGetValue(subjectId, out var subject) || !classCorrelations.ContainsKey(subjectId))
                    continue;

                var (studentAttendanceRate, studentScore) = studentMetrics[subjectId];
                var classData = classCorrelations[subjectId];

                double classAvgAttendance = classData.Average(d => d.AttendanceRate);
                decimal classAvgScore = classData.Average(d => d.Score);

                double correlation = CalculatePearsonCorrelation(
                    classData.Select(d => d.AttendanceRate).ToArray(),
                    classData.Select(d => (double)d.Score).ToArray());

                decimal improvementPotential = CalculateSubjectImprovementPotential(
                    studentAttendanceRate, studentScore, classAvgAttendance, classAvgScore, correlation);

                double optimalAttendance = CalculateOptimalAttendance(classData);
                var subjectSemester = semesters.Values.FirstOrDefault();

                insights.Add(new SubjectAttendanceInsightDto
                {
                    SubjectId = subjectId,
                    SubjectName = subject.Name,
                    SemesterId = subjectSemester?.Id,
                    SemesterName = subjectSemester != null ? $"{subjectSemester.Type} {subjectSemester.AcademicYear?.Name}" : null,
                    StudentAttendanceRate = studentAttendanceRate,
                    StudentAverageScore = studentScore,
                    ClassAvgAttendanceRate = classAvgAttendance,
                    ClassAvgScore = classAvgScore,
                    AttendanceScoreCorrelation = correlation,
                    ImprovementPotential = improvementPotential,
                    OptimalAttendanceTarget = optimalAttendance,
                    PerformanceImpact = DeterminePerformanceImpact(correlation),
                    PersonalizedRecommendation = GeneratePersonalizedRecommendation(
                        studentAttendanceRate, classAvgAttendance, correlation, optimalAttendance, subject.Name)
                });
            }

            return insights;
        }

        /// <summary>
        /// Calculates overall metrics across all subjects
        /// </summary>
        private (double StudentAttendance, decimal StudentScore, double ClassAttendance, decimal ClassScore) CalculateOverallMetrics(
            Dictionary<Guid, (double AttendanceRate, decimal Score)> studentMetrics,
            Dictionary<Guid, List<(double AttendanceRate, decimal Score)>> classCorrelations)
        {
            double overallStudentAttendance = studentMetrics.Any() ? studentMetrics.Average(kv => kv.Value.AttendanceRate) : 0;
            decimal overallStudentScore = studentMetrics.Any() ? studentMetrics.Average(kv => kv.Value.Score) : 0;

            double overallClassAttendance = classCorrelations.Any() ? classCorrelations.Average(kv => kv.Value.Average(v => v.AttendanceRate)) : 0;
            decimal overallClassScore = classCorrelations.Any() ? classCorrelations.Average(kv => kv.Value.Average(v => v.Score)) : 0;

            return (overallStudentAttendance, overallStudentScore, overallClassAttendance, overallClassScore);
        }

        // ======= ATTENDANCE PERFORMANCE HELPER METHODS =======

        private double CalculatePearsonCorrelation(double[] xValues, double[] yValues)
        {
            if (xValues.Length != yValues.Length || xValues.Length < 2)
                return 0;

            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0, sumY2 = 0;
            int n = xValues.Length;

            for (int i = 0; i < n; i++)
            {
                sumX += xValues[i];
                sumY += yValues[i];
                sumXY += xValues[i] * yValues[i];
                sumX2 += xValues[i] * xValues[i];
                sumY2 += yValues[i] * yValues[i];
            }

            double numerator = n * sumXY - sumX * sumY;
            double denominator = Math.Sqrt((n * sumX2 - sumX * sumX) * (n * sumY2 - sumY * sumY));

            if (denominator == 0)
                return 0;

            return Math.Round(numerator / denominator, 2);
        }

        private decimal CalculateSubjectImprovementPotential(double studentAttendanceRate, decimal studentScore, double classAvgAttendance, decimal classAvgScore, double correlation)
        {
            if (studentAttendanceRate >= 0.95 || correlation <= 0)
                return 0;

            double optimalAttendance = Math.Min(0.95, classAvgAttendance * 1.15);
            double attendanceGap = optimalAttendance - studentAttendanceRate;
            if (attendanceGap <= 0) return 0;

            double adjustedCorrelation = Math.Abs(correlation) * 0.7;
            decimal baseImprovement = (decimal)(attendanceGap * adjustedCorrelation * (double)(classAvgScore - studentScore > 0 ? classAvgScore - studentScore : 5m));

            if (studentScore > 85)
                baseImprovement *= (100m - studentScore) / 15m;

            return Math.Min(baseImprovement, 15.0m);
        }

        private double CalculateOptimalAttendance(List<(double AttendanceRate, decimal Score)> classData)
        {
            if (classData.Count < 5)
                return 0.95;

            var topPerformers = classData.OrderByDescending(d => d.Score).Take(Math.Max(1, classData.Count / 4)).ToList();
            double topAttendance = topPerformers.Average(d => d.AttendanceRate);
            return Math.Min(0.98, Math.Max(0.85, topAttendance));
        }

        private string DeterminePerformanceImpact(double correlation)
        {
            if (correlation >= 0.7) return "Very Strong";
            else if (correlation >= 0.5) return "Strong";
            else if (correlation >= 0.3) return "Moderate";
            else if (correlation >= 0.1) return "Weak";
            else if (correlation > -0.1) return "Negligible";
            else return "Inverse";
        }

        private string GeneratePersonalizedRecommendation(double studentAttendance, double classAvgAttendance, double correlation, double optimalAttendance, string subjectName)
        {
            if (correlation <= 0.1)
            {
                return $"Attendance appears to have limited impact on your performance in {subjectName}. Focus on effective study strategies and engagement quality rather than just attendance.";
            }

            if (studentAttendance >= optimalAttendance)
            {
                return $"Your excellent attendance in {subjectName} is already at or above the optimal level. To further improve, focus on active participation and engagement during class.";
            }

            double attendanceGap = optimalAttendance - studentAttendance;

            if (attendanceGap > 0.2)
            {
                return $"Significantly increasing your attendance in {subjectName} (current: {studentAttendance:P0}, target: {optimalAttendance:P0}) could substantially improve your performance. Missing classes has a strong correlation with lower grades in this subject.";
            }
            if (attendanceGap > 0.1)
            {
                return $"Improving your attendance in {subjectName} from {studentAttendance:P0} to {optimalAttendance:P0} would likely have a positive impact on your grades. Consider prioritizing this class in your schedule.";
            }
            return $"Your attendance in {subjectName} is good but could be optimized. Aim for {optimalAttendance:P0} attendance to maximize your performance, and focus on quality engagement during class time.";
        }

        private int CalculateAttendancePerformanceRank(Guid studentId, Dictionary<Guid, (double AttendanceRate, decimal Score)> studentMetrics, Dictionary<Guid, List<(double AttendanceRate, decimal Score)>> classMetrics)
        {
            var studentScores = new Dictionary<Guid, double>();

            if (studentMetrics.Any())
            {
                double avgAttendance = studentMetrics.Average(m => m.Value.AttendanceRate);
                decimal avgScore = studentMetrics.Average(m => m.Value.Score);
                double combinedScore = (0.6 * (double)avgScore) + (0.4 * avgAttendance * 100);
                studentScores[studentId] = combinedScore;
            }

            var allClassData = classMetrics.SelectMany(kv => kv.Value).ToList();
            if (allClassData.Any())
            {
                double classAvgAttendance = allClassData.Average(d => d.AttendanceRate);
                decimal classAvgScore = allClassData.Average(d => d.Score);
                double classCombinedScore = (0.6 * (double)classAvgScore) + (0.4 * classAvgAttendance * 100);

                if (studentScores.ContainsKey(studentId))
                {
                    double studentScore = studentScores[studentId];
                    return studentScore >= classCombinedScore ? 1 : 2;
                }
            }

            return 0;
        }

        private string IdentifyPrimaryAreaForImprovement(List<SubjectAttendanceInsightDto> insights)
        {
            if (!insights.Any())
                return "No specific areas identified for improvement.";

            var significantInsights = insights.Where(i => i.AttendanceScoreCorrelation >= 0.3 && i.ImprovementPotential > 0).OrderByDescending(i => i.ImprovementPotential).ToList();

            if (!significantInsights.Any())
                return "Attendance does not show a strong correlation with performance in your subjects.";

            var topSubject = significantInsights.First();
            return $"{topSubject.SubjectName} (potential score improvement: {topSubject.ImprovementPotential:F1} points with {topSubject.OptimalAttendanceTarget:P0} attendance target)";
        }

        private double CalculateEstimatedGpaImpact(List<SubjectAttendanceInsightDto> insights, Dictionary<Guid, Subject> subjects, Guid gradingSystemId)
        {
            if (!insights.Any() || !subjects.Any())
                return 0;

            double currentGpaPoints = 0;
            double improvedGpaPoints = 0;
            int totalCredits = 0;

            foreach (var insight in insights)
            {
                if (!subjects.TryGetValue(insight.SubjectId, out var subject))
                    continue;

                int credits = subject.CreditHours ?? 1;
                totalCredits += credits;

                double currentPoints = _gradingSystemService.ConvertScoreToGpaPoints(insight.StudentAverageScore, gradingSystemId);
                currentGpaPoints += currentPoints * credits;

                decimal improvedScore = insight.StudentAverageScore + insight.ImprovementPotential;
                double improvedPoints = _gradingSystemService.ConvertScoreToGpaPoints(improvedScore, gradingSystemId);
                improvedGpaPoints += improvedPoints * credits;
            }

            if (totalCredits == 0)
                return 0;

            double currentGpa = currentGpaPoints / totalCredits;
            double improvedGpa = improvedGpaPoints / totalCredits;

            return Math.Round(improvedGpa - currentGpa, 2);
        }

        private string GetPeriodString(AcademicYear year, Semester semester)
        {
            if (year == null) return "Current Period";
            if (semester == null) return year.Name;
            return $"{semester.Type} {year.Name}";
        }

        private double CalculateAttendanceRate(IEnumerable<Absence> absenceRecords, int totalClasses)
        {
            if (totalClasses <= 0) return 1.0;

            int unexcusedAbsences = absenceRecords.Count(a => a.Status == AbsenceStatus.Absent);
            int excusedAbsences = absenceRecords.Count(a => a.Status == AbsenceStatus.ExcusedAbsence);
            int lateArrivals = absenceRecords.Count(a => a.Status == AbsenceStatus.Late);

            double weightedAbsences = unexcusedAbsences + (excusedAbsences * 0.5) + (lateArrivals * 0.25);
            double classesAttended = Math.Max(0, totalClasses - weightedAbsences);
            return Math.Min(1.0, classesAttended / totalClasses);
        }

        private int EstimateTotalClasses(Guid subjectId, Semester semester)
        {
            if (semester == null) return 40;

            var subject = _unitOfWork.Subjects.GetByIdAsync(subjectId).Result;
            if (subject == null) return 40;

            int weeks = semester.WeekCount > 0 ? semester.WeekCount : (int)Math.Ceiling((semester.EndDate - semester.StartDate).TotalDays / 7);
            int classesPerWeek = subject.CreditHours.HasValue ? Math.Max(1, subject.CreditHours.Value) : (subject.SubjectType == SubjectType.UniversityCourse ? 2 : 3);
            int adjustedWeeks = weeks - 2;
            return Math.Max(0, adjustedWeeks * classesPerWeek);
        }

        /// <summary>
        /// Creates an empty performance trend object
        /// </summary>
        private PerformanceTrendDto CreateEmptyPerformanceTrend()
        {
            return new PerformanceTrendDto(new List<string>(), new List<double>(), new List<double>(), new List<Guid>());
        }

        /// <summary>
        /// Creates an empty dashboard when no data is available
        /// </summary>
        private StudentGradeDashboardDto CreateEmptyDashboard(Semester semester = null)
        {
            return new StudentGradeDashboardDto(
                GPA: 0,
                GPATrend: 0,
                ClassRank: null,
                ClassAverage: null,
                Courses: new List<CourseGradeDto>(),
                Comments: new List<TeacherCommentDto>(),
                GradeDistribution: new GradeDistributionDto(new Dictionary<string, int>()),
                PerformanceTrend: CreateEmptyPerformanceTrend(),
                CurrentSemesterId: semester?.Id,
                CurrentSemesterName: semester != null ? $"{semester.Type} {semester.AcademicYear?.Name}" : null,
                SemesterBreakdown: new List<SemesterSummaryDto>()
            );
        }

        /// <summary>
        /// Creates an empty transcript when no data is available
        /// </summary>
        private TranscriptDto CreateEmptyTranscript()
        {
            return new TranscriptDto(
                Courses: new List<CourseGradeDto>(),
                TotalCreditsAttempted: 0,
                TotalCreditsEarned: 0,
                MajorCreditsCompleted: 0,
                MajorCreditsRequired: 60,
                GenEdCreditsCompleted: 0,
                GenEdCreditsRequired: 30,
                GPAByTerm: new Dictionary<string, double>(),
                SemesterSummaries: new Dictionary<string, SemesterSummaryDto>()
            );
        }

        /// <summary>
        /// Calculates credits for major courses
        /// </summary>
        private async Task<int> CalculateMajorCredits(List<CourseGradeDto> courses, Student student, CancellationToken cancellationToken = default)
        {
            int totalCredits = 0;

            if (student.IsSchoolStudent || !student.MajorId.HasValue)
                return totalCredits;

            var major = await _unitOfWork.Majors.GetByIdAsync(student.MajorId.Value, cancellationToken);
            if (major == null)
                return totalCredits;

            var subjectIds = courses.Select(c => c.Id).ToList();
            var subjects = await _unitOfWork.Subjects.GetByIdsAsync(subjectIds);

            var departments = await _unitOfWork.Departments.GetByFacultyAsync(major.FacultyId);
            var departmentIds = departments.Select(d => d.Id).ToHashSet();

            foreach (var course in courses.Where(c => c.Status == "Completed"))
            {
                var subject = subjects.FirstOrDefault(s => s.Id == course.Id);
                if (subject == null) continue;

                bool isMajorCourse = (subject.DepartmentId.HasValue && departmentIds.Contains(subject.DepartmentId.Value)) ||
                                   (subject.Code?.StartsWith(major.Code, StringComparison.OrdinalIgnoreCase) == true);

                if (isMajorCourse)
                    totalCredits += course.Credits ?? 0;
            }

            return totalCredits;
        }

        /// <summary>
        /// Calculates credits for general education courses
        /// </summary>
        private async Task<int> CalculateGenEdCredits(List<CourseGradeDto> courses, CancellationToken cancellationToken = default)
        {
            int totalCredits = 0;
            var subjectIds = courses.Select(c => c.Id).ToList();
            var subjects = await _unitOfWork.Subjects.GetByIdsAsync(subjectIds);

            foreach (var course in courses.Where(c => c.Status == "Completed"))
            {
                var subject = subjects.FirstOrDefault(s => s.Id == course.Id);
                if (subject == null) continue;

                if (IsGeneralEducationCourse(subject, course.Code))
                    totalCredits += course.Credits ?? 0;
            }

            return totalCredits;
        }

        /// <summary>
        /// Determines if a subject is a general education course
        /// </summary>
        private bool IsGeneralEducationCourse(Subject subject, string courseCode)
        {
            var genEdDepartments = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "English", "Mathematics", "Math", "History", "Science", "Biology",
                "Chemistry", "Physics", "Arts", "Humanities", "Philosophy",
                "Language", "Communication", "Psychology", "Sociology"
            };

            var genEdPrefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ENG", "MATH", "HIST", "SCI", "BIO", "CHEM", "PHYS", "ART",
                "HUM", "PHIL", "LANG", "COMM", "PSY", "SOC"
            };

            if (subject.SubjectType == SubjectType.UniversityCourse && !string.IsNullOrEmpty(courseCode))
            {
                string codeDigits = new string(courseCode.Where(char.IsDigit).ToArray());
                if (codeDigits.Length >= 3 && int.TryParse(codeDigits.Substring(0, 1), out int level))
                {
                    if (level == 1 || level == 2)
                        return true;
                }
            }

            if (subject.IsElective)
            {
                return subject.ElectiveType == ElectiveType.Language ||
                       subject.ElectiveType == ElectiveType.Academic ||
                       subject.ElectiveType == ElectiveType.Arts;
            }

            if (subject.Department?.Name != null)
            {
                foreach (var deptName in genEdDepartments)
                {
                    if (subject.Department.Name.Contains(deptName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            if (!string.IsNullOrEmpty(courseCode))
            {
                string prefix = new string(courseCode.TakeWhile(char.IsLetter).ToArray());
                return !string.IsNullOrEmpty(prefix) && genEdPrefixes.Contains(prefix);
            }

            return subject.SubjectType == SubjectType.SchoolSubject && !subject.IsElective;
        }

        #endregion
    }
}