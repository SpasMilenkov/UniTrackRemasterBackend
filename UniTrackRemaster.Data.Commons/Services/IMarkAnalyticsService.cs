using UniTrackRemaster.Api.Dto.Student.Analytics;

namespace UniTrackRemaster.Commons.Services;

   /// <summary>
    /// Mark analytics service interface with semester-aware methods
    /// </summary>
    public interface IMarkAnalyticsService
    {
        #region Grade Dashboard and Analytics

        /// <summary>
        /// Gets a comprehensive grade dashboard for a student with semester-aware analytics
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="term">Semester term (optional)</param>
        /// <param name="academicYear">Academic year (optional)</param>
        /// <returns>Student grade dashboard with semester breakdown</returns>
        /// <exception cref="KeyNotFoundException">Thrown when student is not found</exception>
        /// <exception cref="ApplicationException">Thrown when dashboard generation fails</exception>
        Task<StudentGradeDashboardDto> GetStudentGradeDashboard(Guid studentId, string? term = null, string? academicYear = null);

        /// <summary>
        /// Gets a complete academic transcript with semester summaries
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Complete transcript with semester breakdown</returns>
        /// <exception cref="KeyNotFoundException">Thrown when student is not found</exception>
        /// <exception cref="ApplicationException">Thrown when transcript generation fails</exception>
        Task<TranscriptDto> GetStudentTranscript(Guid studentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets grades for a specific academic term with semester awareness
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="term">Semester term (required)</param>
        /// <param name="academicYear">Academic year (required)</param>
        /// <returns>List of courses and grades for the specified term</returns>
        /// <exception cref="ArgumentException">Thrown when term or academic year is null/empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown when student or academic term is not found</exception>
        /// <exception cref="ApplicationException">Thrown when grade retrieval fails</exception>
        Task<List<CourseGradeDto>> GetStudentTermGrades(Guid studentId, string term, string academicYear);

        #endregion

        #region GPA Calculations

        /// <summary>
        /// Calculates a student's GPA for a specific period with semester awareness
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="term">Semester term (optional)</param>
        /// <param name="academicYear">Academic year (optional)</param>
        /// <returns>Calculated GPA for the specified period</returns>
        /// <exception cref="KeyNotFoundException">Thrown when student is not found</exception>
        /// <exception cref="ApplicationException">Thrown when GPA calculation fails</exception>
        Task<double> CalculateStudentGPA(Guid studentId, string? term = null, string? academicYear = null);

        /// <summary>
        /// Gets a student's rank within their class/grade with semester awareness
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="gradeId">Grade/class ID</param>
        /// <returns>Student's rank (1-based) or null if ranking cannot be determined</returns>
        Task<int?> GetStudentClassRank(Guid studentId, Guid gradeId);

        #endregion

        #region Attendance Analytics

        /// <summary>
        /// Analyzes attendance-performance correlation with semester awareness
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="term">Semester term (optional)</param>
        /// <param name="academicYear">Academic year (optional)</param>
        /// <returns>Detailed attendance-performance insights with recommendations</returns>
        /// <exception cref="KeyNotFoundException">Thrown when student is not found</exception>
        /// <exception cref="ApplicationException">Thrown when analysis fails</exception>
        Task<AttendancePerformanceInsightDto> GetAttendancePerformanceInsight(
            Guid studentId, string? term = null, string? academicYear = null);

        #endregion

        #region Export Functions

        /// <summary>
        /// Exports student grades in the specified format
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="format">Export format (pdf, excel, etc.)</param>
        /// <param name="term">Semester term (optional)</param>
        /// <param name="academicYear">Academic year (optional)</param>
        /// <returns>Exported file as byte array</returns>
        /// <exception cref="NotImplementedException">Thrown when export functionality is not implemented</exception>
        /// <exception cref="KeyNotFoundException">Thrown when student is not found</exception>
        /// <exception cref="ApplicationException">Thrown when export fails</exception>
        Task<byte[]> ExportGrades(Guid studentId, string format, string? term = null, string? academicYear = null);

        #endregion
    }