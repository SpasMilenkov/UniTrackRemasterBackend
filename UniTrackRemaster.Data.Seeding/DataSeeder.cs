using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using UniTrackRemaster.Data.Models.Users;
using UniTrackRemaster.Commons.Enums;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Organizations;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Location;
using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace UniTrackRemaster.Data.Seeding;

    public class DataSeeder
    {
        private static UserManager<ApplicationUser> _userManager = null!;
        private static UniTrackDbContext _dbContext = null!;

        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<UniTrackDbContext>();

            _userManager = userManager;
            _dbContext = dbContext;

            await SeedRolesAsync(roleManager);
            await SeedSuperAdminAsync(userManager, dbContext);

            // Seed comprehensive data in the correct order
            // await SeedInstitutionsAsync();
            // await SeedAcademicDataAsync();  // Create subjects, grades, faculties first
            // await SeedUsersAsync();  // Then create users (teachers and students)
            // await SeedRelationshipsAsync(); //Create relationships between teachers, subjects, and grades
            // await SeedAnalyticsAsync();  // Create reports after users exist
                                        // await SeedClubsAndEventsAsync();  // Create clubs after teachers and students exist
        }

        // Summary of what this seeder includes
        private static void PrintSeederSummary()
        {
            Console.WriteLine("=============== Data Seeder Summary ===============");
            Console.WriteLine("This seeder includes:");
            Console.WriteLine("1. Roles: SuperAdmin, Admin, Teacher, Student, Parent, Guest");
            Console.WriteLine("2. One SuperAdmin user");
            Console.WriteLine("3. 5 Universities with reports, faculties, departments, and majors");
            Console.WriteLine("4. 5 Schools with reports and grade levels");
            Console.WriteLine("5. 1 Admin per institution");
            Console.WriteLine("6. 15 Teachers per institution");
            Console.WriteLine("7. 100 Students per institution");
            Console.WriteLine("8. Academic years and semesters for each institution");
            Console.WriteLine("9. Subject curricula for both universities and schools");
            Console.WriteLine("10. Grading systems with scale definitions");
            Console.WriteLine("11. Personal reports for each student");
            Console.WriteLine("12. Marks, attendance records, and recommendations");
            Console.WriteLine("==================================================");
        }

        private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
        {
            await EnsureRoleExistsAsync(roleManager, nameof(Roles.SuperAdmin));
            await EnsureRoleExistsAsync(roleManager, nameof(Roles.Admin));
            await EnsureRoleExistsAsync(roleManager, nameof(Roles.Guest));
            await EnsureRoleExistsAsync(roleManager, nameof(Roles.Teacher));
            await EnsureRoleExistsAsync(roleManager, nameof(Roles.Student));
            await EnsureRoleExistsAsync(roleManager, nameof(Roles.Parent));
        }

        private static async Task EnsureRoleExistsAsync(RoleManager<ApplicationRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }

        private static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager, UniTrackDbContext dbContext)
        {
            const string email = "superadmin@unitracker.com";
            const string username = "superadmin";
            const string password = "Superadmin@123";

            try
            {
                var superAdmin = await userManager.FindByEmailAsync(email);
                if (superAdmin == null)
                {
                    superAdmin = new ApplicationUser
                    {
                        UserName = username,
                        Email = email,
                        EmailConfirmed = true,
                        FirstName = "Super",
                        LastName = "Admin",
                        PhoneNumber = "1234567890",
                        PhoneNumberConfirmed = true,
                        IsLinked = true
                    };

                    var result = await userManager.CreateAsync(superAdmin, password);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(superAdmin, nameof(Roles.SuperAdmin));

                        // Check if super admin entity already exists
                        if (!await dbContext.SuperAdmins.AnyAsync(sa => sa.UserId == superAdmin.Id))
                        {
                            var superAdminEntity = new SuperAdmin
                            {
                                Id = Guid.NewGuid(),
                                UserId = superAdmin.Id,
                            };

                            dbContext.SuperAdmins.Add(superAdminEntity);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create super admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating super admin: {ex.Message}");
                throw;
            }
        }

        private static async Task SeedInstitutionsAsync()
        {
            try
            {
                // Check if institutions already exist
                if (await _dbContext.Institutions.AnyAsync())
                {
                    Console.WriteLine("Institutions already exist. Skipping institution seeding.");
                    return;
                }

                // Use UTC for all DateTime values
                var now = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync(); // Save reports first

                // Seed Universities
                var universityAddresses = new List<Address>
                {
                    new Address { Id = Guid.NewGuid(), Country = "United States", Settlement = "Cambridge", PostalCode = "02138", Street = "Harvard Yard" },
                    new Address { Id = Guid.NewGuid(), Country = "United Kingdom", Settlement = "Oxford", PostalCode = "OX1 2JD", Street = "University Offices" },
                    new Address { Id = Guid.NewGuid(), Country = "Bulgaria", Settlement = "Sofia", PostalCode = "1000", Street = "15 Tsar Osvoboditel Blvd" },
                    new Address { Id = Guid.NewGuid(), Country = "Germany", Settlement = "Berlin", PostalCode = "10117", Street = "Unter den Linden" },
                    new Address { Id = Guid.NewGuid(), Country = "Japan", Settlement = "Tokyo", PostalCode = "113-8654", Street = "7-3-1 Hongo" }
                };

                // Seed Schools
                var schoolAddresses = new List<Address>
                {
                    new Address { Id = Guid.NewGuid(), Country = "United States", Settlement = "New York", PostalCode = "10021", Street = "32 E 76th St" },
                    new Address { Id = Guid.NewGuid(), Country = "United Kingdom", Settlement = "London", PostalCode = "SW1P 3BT", Street = "Dean's Yard" },
                    new Address { Id = Guid.NewGuid(), Country = "Bulgaria", Settlement = "Sofia", PostalCode = "1504", Street = "Viktor Hugo 15" },
                    new Address { Id = Guid.NewGuid(), Country = "Canada", Settlement = "Toronto", PostalCode = "M5S 1A7", Street = "371 Bloor Street West" },
                    new Address { Id = Guid.NewGuid(), Country = "Australia", Settlement = "Melbourne", PostalCode = "3004", Street = "Domain Road" }
                };

                var schools = new List<(Institution institution, School school)>
                {
                    CreateSchool("The Dalton School", "Progressive private school in Manhattan",
                        InstitutionType.PrivateSchool, schoolAddresses[0], 1300, 15.0, true),
                    CreateSchool("Westminster School", "Historic British public school",
                        InstitutionType.PrivateSchool, schoolAddresses[1], 750, 12.0, true),
                    CreateSchool("First English Language School", "Premier language school in Sofia",
                        InstitutionType.InternationalSchool, schoolAddresses[2], 1200, 18.0, false),
                    CreateSchool("University of Toronto Schools", "University-affiliated school",
                        InstitutionType.PrivateSchool, schoolAddresses[3], 675, 10.0, false),
                    CreateSchool("Melbourne Grammar School", "Independent Anglican school",
                        InstitutionType.PrivateSchool, schoolAddresses[4], 1800, 16.0, true)
                };

                foreach (var (institution, school) in schools)
                {
                    // Check if institution already exists
                    if (!await _dbContext.Institutions.AnyAsync(i => i.Name == institution.Name))
                    {
                        // Add address first
                        _dbContext.SchoolAddress.Add(institution.Address);
                        _dbContext.Institutions.Add(institution);
                        _dbContext.Schools.Add(school);
                    }
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding institutions: {ex.Message}");
                throw;
            }
        }


        private static (Institution institution, University university) CreateUniversity(
            string name, string description, InstitutionType type, Address address,
            FocusArea[] focusAreas, int undergrads, int grads, double acceptanceRate, Guid reportId)
        {
            var institution = new Institution
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Type = type,
                Location = LocationType.Urban,
                Accreditations = new List<AccreditationType> { AccreditationType.Regional, AccreditationType.National },
                Address = address,
                LogoUrl = $"https://example.com/logos/{name.Replace(" ", "").ToLower()}.png",
                EstablishedDate = DateTime.UtcNow.AddYears(-100 - new Random().Next(100)),
                Website = $"https://{name.Replace(" ", "").ToLower()}.edu",
                Email = $"info@{name.Replace(" ", "").ToLower()}.edu",
                Phone = "+1-555-" + new Random().Next(1000, 9999),
                IntegrationStatus = IntegrationStatus.Active
            };

            var university = new University
            {
                Id = Guid.NewGuid(),
                InstitutionId = institution.Id,
                FocusAreas = new List<FocusArea>(focusAreas),
                UndergraduateCount = undergrads,
                GraduateCount = grads,
                AcceptanceRate = acceptanceRate,
                ResearchFunding = new Random().Next(50000000, 500000000),
                HasStudentHousing = true,
                Departments = new List<string> { "Computer Science", "Engineering", "Mathematics", "Physics", "Chemistry", "Business", "Law" },
                UniversityReportId = reportId  // Set the actual report ID
            };

            return (institution, university);
        }

        private static (Institution institution, School school) CreateSchool(
            string name, string description, InstitutionType type, Address address,
            int studentCount, double ratio, bool hasUniform)
        {
            var institution = new Institution
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Type = type,
                Location = LocationType.Urban,
                Accreditations = new List<AccreditationType> { AccreditationType.Regional },
                Address = address,
                LogoUrl = $"https://example.com/logos/{name.Replace(" ", "").ToLower()}.png",
                EstablishedDate = DateTime.UtcNow.AddYears(-50 - new Random().Next(100)),
                Website = $"https://{name.Replace(" ", "").ToLower()}.edu",
                Email = $"info@{name.Replace(" ", "").ToLower()}.edu",
                Phone = "+1-555-" + new Random().Next(1000, 9999),
                IntegrationStatus = IntegrationStatus.Active
            };

            var school = new School
            {
                Id = Guid.NewGuid(),
                InstitutionId = institution.Id,
                StudentCount = studentCount,
                StudentTeacherRatio = ratio,
                HasSpecialEducation = true,
                HasUniform = hasUniform,
                Programs = new[] { "International Baccalaureate", "Advanced Placement", "STEM Program" }
            };

            return (institution, school);
        }

        private static async Task SeedUsersAsync()
        {
            var institutions = await _dbContext.Institutions
                .Include(i => i.Address)
                .ToListAsync();
            var rand = new Random();

            // Seed Admins
            foreach (var institution in institutions)
            {
                var adminUser = await CreateUserAsync($"admin_{institution.Name.Replace(" ", "").ToLower()}",
                    "admin", institution.Name, $"admin@{institution.Email.Split('@')[1]}", true);

                if (adminUser != null)
                {
                    // Associate user with this institution
                    if (adminUser.Institutions == null)
                        adminUser.Institutions = new List<Institution>();

                    adminUser.Institutions.Add(institution);
                    adminUser.IsLinked = true;

                    // Save user-institution relationship
                    await _userManager.UpdateAsync(adminUser);

                    var admin = new Admin
                    {
                        Id = Guid.NewGuid(),
                        UserId = adminUser.Id,
                        InstitutionId = institution.Id,
                        Position = "Principal Administrator",
                        Department = "Administration",
                        StartDate = DateTime.UtcNow.AddYears(-5),
                        Role = AdminRole.InstitutionAdmin,
                        Notes = $"Primary administrator for {institution.Name}"
                    };

                    _dbContext.Admins.Add(admin);
                }
            }

            // Seed Teachers
            var teacherFirstNames = new[] { "John", "Emma", "Michael", "Sarah", "David", "Laura", "Robert", "Jessica", "William", "Maria" };
            var teacherLastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };
            var teacherTitles = new[] { "Dr.", "Prof.", "Mr.", "Ms.", "Mrs." };

            foreach (var institution in institutions)
            {
                for (int i = 0; i < 15; i++)
                {
                    var firstName = teacherFirstNames[rand.Next(teacherFirstNames.Length)];
                    var lastName = teacherLastNames[rand.Next(teacherLastNames.Length)];
                    var title = teacherTitles[rand.Next(teacherTitles.Length)];

                    // Create unique username by adding institution short code and teacher number
                    var institutionCode = institution.Name.Split(' ')[0].ToLower();
                    var teacherUser = await CreateUserAsync($"{firstName.ToLower()}.{lastName.ToLower()}.{institutionCode}.t{i}",
                        firstName, lastName, $"{firstName.ToLower()}.{lastName.ToLower()}{i}@{institution.Email.Split('@')[1]}", true);

                    if (teacherUser != null)
                    {
                        // Associate user with this institution
                        if (teacherUser.Institutions == null)
                            teacherUser.Institutions = new List<Institution>();

                        teacherUser.Institutions.Add(institution);
                        teacherUser.IsLinked = true;

                        // Save user-institution relationship
                        await _userManager.UpdateAsync(teacherUser);

                        var teacher = new Teacher
                        {
                            Id = Guid.NewGuid(),
                            UserId = teacherUser.Id,
                            InstitutionId = institution.Id,
                            Title = title
                        };

                        _dbContext.Teachers.Add(teacher);
                    }
                }
            }

            // Seed Students
            var studentFirstNames = new[] { "Oliver", "Amelia", "Noah", "Sophia", "Liam", "Emma", "Ethan", "Ava", "Lucas", "Mia" };
            var studentLastNames = new[] { "Anderson", "Taylor", "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson", "Garcia", "Martinez" };

            foreach (var institution in institutions)
            {
                // Get appropriate grades for each institution
                var grades = await _dbContext.Grades
                    .Where(g => g.InstitutionId == institution.Id)
                    .ToListAsync();

                if (grades.Count == 0)
                {
                    Console.WriteLine($"No grades found for institution {institution.Name}. Skipping student creation for this institution.");
                    continue;
                }

                for (int i = 0; i < 100; i++)
                {
                    var firstName = studentFirstNames[rand.Next(studentFirstNames.Length)];
                    var lastName = studentLastNames[rand.Next(studentLastNames.Length)];

                    // Create unique username by adding institution short code and student number
                    var institutionCode = institution.Name.Split(' ')[0].ToLower();
                    var studentUser = await CreateUserAsync($"{firstName.ToLower()}.{lastName.ToLower()}.{institutionCode}.s{i}",
                        firstName, lastName, $"{firstName.ToLower()}.{lastName.ToLower()}{i}@student.{institution.Email.Split('@')[1]}", false);

                    if (studentUser != null)
                    {
                        var isSchool = institution.Type.ToString().Contains("School");

                        // Assign a random grade from the available grades for this institution
                        var grade = grades[rand.Next(grades.Count)];

                        // Get appropriate fields based on institution type
                        Guid? schoolId = null;
                        Guid? universityId = null;
                        Guid? majorId = null;

                        if (isSchool)
                        {
                            var school = await _dbContext.Schools
                                .FirstOrDefaultAsync(s => s.InstitutionId == institution.Id);
                            schoolId = school?.Id;
                        }
                        else
                        {
                            var university = await _dbContext.Universities
                                .FirstOrDefaultAsync(u => u.InstitutionId == institution.Id);

                            // Skip if no university is found
                            if (university == null)
                            {
                                Console.WriteLine($"No university found for institution {institution.Name}. Skipping student creation.");
                                continue;
                            }

                            universityId = university.Id;

                            // Get a random major if this is a university
                            var majors = await _dbContext.Majors
                                .Where(m => m.Faculty.UniversityId == university.Id)
                                .ToListAsync();

                            if (majors.Count != 0)
                            {
                                majorId = majors[rand.Next(majors.Count)].Id;
                            }
                        }

                        // Associate user with this institution
                        if (studentUser.Institutions == null)
                            studentUser.Institutions = new List<Institution>();

                        studentUser.Institutions.Add(institution);
                        studentUser.IsLinked = true;

                        // Save user-institution relationship
                        await _userManager.UpdateAsync(studentUser);

                        var student = new Student
                        {
                            Id = Guid.NewGuid(),
                            UserId = studentUser.Id,
                            IsSchoolStudent = isSchool,
                            GradeId = grade.Id,
                            SchoolId = schoolId,
                            UniversityId = universityId,
                            MajorId = majorId,
                        };

                        _dbContext.Students.Add(student);
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        private static async Task<ApplicationUser?> CreateUserAsync(string username, string firstName, string lastName, string email, bool isLinked)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    Console.WriteLine($"User with email {email} already exists. Skipping.");
                    return existingUser;
                }
                existingUser = await _userManager.FindByNameAsync(username);
                if (existingUser != null)
                {
                    Console.WriteLine($"User with username {username} already exists. Skipping.");
                    return existingUser;
                }

                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = $"+1-555-{new Random().Next(1000, 9999)}",
                    PhoneNumberConfirmed = true,
                    IsLinked = isLinked,
                    ProfileVisibility = ProfileVisibility.Institution,
                    DataAnalytics = true,
                    EmailUpdates = true,
                    MarketingEmails = false
                };

                var result = await _userManager.CreateAsync(user, "Password123!");
                if (result.Succeeded)
                {
                    // Better role assignment logic
                    string role;
                    if (username.StartsWith("admin"))
                    {
                        role = nameof(Roles.Admin);
                    }
                    else if (username.EndsWith(".t", StringComparison.OrdinalIgnoreCase) || username.Contains(".t"))
                    {
                        role = nameof(Roles.Teacher);
                    }
                    else if (username.EndsWith(".s", StringComparison.OrdinalIgnoreCase) || username.Contains(".s"))
                    {
                        role = nameof(Roles.Student);
                    }
                    else
                    {
                        // Default role if pattern doesn't match
                        role = nameof(Roles.Student);
                    }

                    await _userManager.AddToRoleAsync(user, role);
                    return user;
                }
                else
                {
                    Console.WriteLine($"Failed to create user {username}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user {username}: {ex.Message}");
            }
            return null;
        }
        private static string[] GetDepartmentsByFaculty(string facultyName)
        {
            return facultyName switch
            {
                "Engineering" => new[] { "Computer Engineering", "Electrical Engineering", "Mechanical Engineering", "Civil Engineering", "Chemical Engineering" },
                "Business" => new[] { "Accounting", "Finance", "Marketing", "Management", "Economics" },
                "Arts and Sciences" => new[] { "Mathematics", "Physics", "Chemistry", "Biology", "Psychology", "Sociology" },
                "Medicine" => new[] { "General Medicine", "Surgery", "Pediatrics", "Psychiatry", "Radiology" },
                "Law" => new[] { "Civil Law", "Criminal Law", "Corporate Law", "International Law", "Constitutional Law" },
                _ => new[] { "General Department" }
            };
        }

        private static async Task SeedRelationshipsAsync()
        {
            try
            {
                // Get all entities we need to establish relationships between
                var teachers = await _dbContext.Teachers
                    .Include(t => t.Subjects)
                    .Include(t => t.Grades)
                    .ToListAsync();

                var subjects = await _dbContext.Subjects.ToListAsync();
                var grades = await _dbContext.Grades.ToListAsync();
                var institutions = await _dbContext.Institutions.ToListAsync();
                var rand = new Random();

                foreach (var institution in institutions)
                {
                    var institutionTeachers = teachers.Where(t => t.InstitutionId == institution.Id).ToList();
                    if (!institutionTeachers.Any()) continue;

                    var institutionGrades = grades.Where(g => g.InstitutionId == institution.Id).ToList();
                    if (!institutionGrades.Any()) continue;

                    // Assign subjects to teachers based on institution type
                    if (institution.Type.ToString().Contains("University"))
                    {
                        var universitySubjects = subjects.Where(s =>
                            s.SubjectType == SubjectType.UniversityCourse ||
                            s.AcademicLevel == AcademicLevel.Undergraduate ||
                            s.AcademicLevel == AcademicLevel.Graduate).ToList();

                        foreach (var teacher in institutionTeachers)
                        {
                            // Initialize collections if they're null
                            teacher.Subjects ??= new List<Subject>();
                            teacher.Grades ??= new List<Grade>();

                            // Each teacher teaches 2-4 subjects
                            var subjectCount = rand.Next(2, 5);
                            var assignedSubjects = universitySubjects
                                .OrderBy(x => rand.Next())
                                .Take(subjectCount)
                                .ToList();

                            foreach (var subject in assignedSubjects)
                            {
                                if (!teacher.Subjects.Contains(subject))
                                {
                                    teacher.Subjects.Add(subject);

                                    // Also ensure the subject knows about this teacher
                                    subject.Teachers ??= new List<Teacher>();
                                    if (!subject.Teachers.Contains(teacher))
                                    {
                                        subject.Teachers.Add(teacher);
                                    }
                                }
                            }

                            // Assign 1-2 grades to each teacher
                            var gradeCount = rand.Next(1, 3);
                            var assignedGrades = institutionGrades
                                .OrderBy(x => rand.Next())
                                .Take(gradeCount)
                                .ToList();

                            foreach (var grade in assignedGrades)
                            {
                                if (!teacher.Grades.Contains(grade))
                                {
                                    teacher.Grades.Add(grade);

                                    // Ensure grade knows about this teacher
                                    grade.Teachers ??= new List<Teacher>();
                                    if (!grade.Teachers.Contains(teacher))
                                    {
                                        grade.Teachers.Add(teacher);
                                    }
                                }
                            }
                        }
                    }
                    else // School
                    {
                        var schoolSubjects = subjects.Where(s =>
                            s.SubjectType == SubjectType.SchoolSubject ||
                            s.AcademicLevel == AcademicLevel.Elementary ||
                            s.AcademicLevel == AcademicLevel.MiddleSchool ||
                            s.AcademicLevel == AcademicLevel.HighSchool).ToList();

                        foreach (var teacher in institutionTeachers)
                        {
                            // Initialize collections if they're null
                            teacher.Subjects ??= new List<Subject>();
                            teacher.Grades ??= new List<Grade>();

                            // Each teacher teaches 1-3 subjects
                            var subjectCount = rand.Next(1, 4);
                            var assignedSubjects = schoolSubjects
                                .OrderBy(x => rand.Next())
                                .Take(subjectCount)
                                .ToList();

                            foreach (var subject in assignedSubjects)
                            {
                                if (!teacher.Subjects.Contains(subject))
                                {
                                    teacher.Subjects.Add(subject);

                                    // Also ensure the subject knows about this teacher
                                    subject.Teachers ??= new List<Teacher>();
                                    if (!subject.Teachers.Contains(teacher))
                                    {
                                        subject.Teachers.Add(teacher);
                                    }
                                }
                            }

                            // Assign 1-2 grades to each teacher
                            var gradeCount = rand.Next(1, 3);
                            var assignedGrades = institutionGrades
                                .OrderBy(x => rand.Next())
                                .Take(gradeCount)
                                .ToList();

                            foreach (var grade in assignedGrades)
                            {
                                if (!teacher.Grades.Contains(grade))
                                {
                                    teacher.Grades.Add(grade);

                                    // Ensure grade knows about this teacher
                                    grade.Teachers ??= new List<Teacher>();
                                    if (!grade.Teachers.Contains(teacher))
                                    {
                                        grade.Teachers.Add(teacher);
                                    }
                                }
                            }
                        }
                    }
                }

                // Assign subjects to grades
                foreach (var grade in grades)
                {
                    grade.Subjects ??= new List<Subject>();

                    var institution = institutions.FirstOrDefault(i => i.Id == grade.InstitutionId);
                    if (institution == null) continue;

                    if (institution.Type.ToString().Contains("University"))
                    {
                        // For universities, assign 6-8 subjects per grade/year
                        var universitySubjects = subjects.Where(s =>
                            s.SubjectType == SubjectType.UniversityCourse ||
                            s.AcademicLevel == AcademicLevel.Undergraduate ||
                            s.AcademicLevel == AcademicLevel.Graduate).ToList();

                        var subjectCount = rand.Next(6, 9);
                        var assignedSubjects = universitySubjects
                            .OrderBy(x => rand.Next())
                            .Take(subjectCount)
                            .ToList();

                        foreach (var subject in assignedSubjects)
                        {
                            if (!grade.Subjects.Contains(subject))
                            {
                                grade.Subjects.Add(subject);

                                // Ensure subject knows about this grade
                                subject.Grades ??= new List<Grade>();
                                if (!subject.Grades.Contains(grade))
                                {
                                    subject.Grades.Add(grade);
                                }
                            }
                        }
                    }
                    else // School
                    {
                        // For schools, assign appropriate subjects based on grade level
                        var gradeLevel = int.Parse(grade.Name.Split(' ')[0].Replace("st", "").Replace("nd", "").Replace("rd", "").Replace("th", ""));

                        var appropriateSubjects = subjects.Where(s =>
                            s.SubjectType == SubjectType.SchoolSubject &&
                            s.MinGradeLevel <= gradeLevel &&
                            (s.MaxGradeLevel >= gradeLevel || s.MaxGradeLevel == null)).ToList();

                        foreach (var subject in appropriateSubjects)
                        {
                            if (!grade.Subjects.Contains(subject))
                            {
                                grade.Subjects.Add(subject);

                                // Ensure subject knows about this grade
                                subject.Grades ??= new List<Grade>();
                                if (!subject.Grades.Contains(grade))
                                {
                                    subject.Grades.Add(grade);  // Fixed: was incorrectly adding 'subject' instead of 'grade'
                                }
                            }
                        }
                    }
                }

                // Set some subjects as primary teacher assignments
                foreach (var subject in subjects)
                {
                    if (subject.Teachers != null && subject.Teachers.Any() && subject.PrimaryTeacherId == null)
                    {
                        subject.PrimaryTeacherId = subject.Teachers.First().Id;
                    }
                }

                await _dbContext.SaveChangesAsync();
                Console.WriteLine("Successfully established relationships between teachers, subjects, and grades");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding relationships: {ex.Message}");
                throw;
            }
        }

        private static async Task SeedAnalyticsAsync()
        {
            try
            {
                // Ensure we have all the necessary data loaded
                var students = await _dbContext.Students
                    .Include(s => s.Grade)
                    .ToListAsync();
                if (!students.Any())
                {
                    Console.WriteLine("No students found. Skipping analytics seeding.");
                    return;
                }

                // Include teachers in subjects for relationships
                var subjects = await _dbContext.Subjects
                    .Include(s => s.Teachers)
                    .Include(s => s.Grades)
                    .ToListAsync();
                if (!subjects.Any())
                {
                    Console.WriteLine("No subjects found. Skipping analytics seeding.");
                    return;
                }

                var teachers = await _dbContext.Teachers
                    .Include(t => t.Subjects)
                    .Include(t => t.Grades)
                    .ToListAsync();
                if (!teachers.Any())
                {
                    Console.WriteLine("No teachers found. Skipping analytics seeding.");
                    return;
                }

                var universities = await _dbContext.Universities
                    .Include(u => u.Institution)
                    .ToListAsync();
                var schools = await _dbContext.Schools
                    .Include(s => s.Institution)
                    .ToListAsync();
                var rand = new Random();

                // University Reports are already created in SeedInstitutionsAsync
                // Save the reports and updated institutions
                await _dbContext.SaveChangesAsync();

                // Create Recommendations for users who don't have them
                var users = await _userManager.Users.ToListAsync();
                foreach (var user in users.Where(u => u.Email != "superadmin@unitracker.com").Take(20))
                {
                    var existingRecommendationsCount = await _dbContext.Recommendations
                        .CountAsync(r => r.UserId == user.Id);

                    if (existingRecommendationsCount < 3)
                    {
                        var recommendationsToAdd = 3 - existingRecommendationsCount;

                        for (int i = 0; i < recommendationsToAdd; i++)
                        {
                            var recommendation = new Recommendation
                            {
                                Id = Guid.NewGuid(),
                                Topic = new[] {
                                    "Study Skills Improvement",
                                    "Time Management Techniques",
                                    "Career Development",
                                    "Research Opportunities",
                                    "Leadership Training"
                                }[rand.Next(5)],
                                SourceLink = "https://example.com/resources/" + Guid.NewGuid().ToString().Substring(0, 8),
                                Date = DateTime.UtcNow.AddDays(-rand.Next(30)),
                                UserId = user.Id,
                                User = user
                            };
                            _dbContext.Recommendations.Add(recommendation);
                        }
                    }
                }

                await _dbContext.SaveChangesAsync();
                Console.WriteLine("Successfully seeded analytics data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding analytics: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        private static List<Subject> CreateSubjects(Institution institution)
        {
            var subjects = new List<Subject>();
            var rand = new Random();

            if (institution.Type.ToString().Contains("University"))
            {
                // University subjects
                var universitySubjects = new[]
                {
                    ("Computer Science 101", "CS101", "Introduction to Computer Science", AcademicLevel.Undergraduate, SubjectType.UniversityCourse, 4),
                    ("Advanced Mathematics", "MATH301", "Advanced Calculus and Linear Algebra", AcademicLevel.Undergraduate, SubjectType.UniversityCourse, 3),
                    ("Organic Chemistry", "CHEM201", "Advanced Organic Chemistry", AcademicLevel.Undergraduate, SubjectType.UniversityCourse, 4),
                    ("Business Management", "BUS101", "Principles of Business Management", AcademicLevel.Undergraduate, SubjectType.UniversityCourse, 3),
                    ("English Literature", "ENG202", "Modern English Literature", AcademicLevel.Undergraduate, SubjectType.UniversityCourse, 3),
                    ("Database Systems", "CS302", "Advanced Database Systems", AcademicLevel.Graduate, SubjectType.UniversityCourse, 3),
                    ("Artificial Intelligence", "CS401", "Machine Learning and AI", AcademicLevel.Graduate, SubjectType.UniversityCourse, 4),
                    ("International Law", "LAW301", "Public International Law", AcademicLevel.Graduate, SubjectType.UniversityCourse, 3),
                    ("Web Development", "CS205", "Modern Web Development", AcademicLevel.Undergraduate, SubjectType.ElectiveCourse, 3),
                    ("Photography", "ART101", "Digital Photography", AcademicLevel.Undergraduate, SubjectType.ElectiveCourse, 2)
                };

                foreach (var (name, code, description, level, type, credits) in universitySubjects)
                {
                    subjects.Add(new Subject
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Code = code,
                        ShortDescription = description,
                        DetailedDescription = $"Comprehensive course covering {description.ToLower()}",
                        SubjectType = type,
                        AcademicLevel = level,
                        CreditHours = credits,
                        CreditValue = credits,
                        IsElective = type == SubjectType.ElectiveCourse,
                        ElectiveType = type == SubjectType.ElectiveCourse ? ElectiveType.Academic : null,
                        HasLab = code.StartsWith("CHEM") || code.StartsWith("CS"),
                        MaxStudents = type == SubjectType.ElectiveCourse ? 30 : null
                    });
                }
            }
            else // School subjects
            {
                var schoolSubjects = new[]
                {
                    ("Mathematics", "MATH", "Basic Mathematics", AcademicLevel.MiddleSchool, 1, 8),
                    ("English Language Arts", "ELA", "Reading and Writing", AcademicLevel.Elementary, 1, 12),
                    ("Science", "SCI", "General Science", AcademicLevel.Elementary, 1, 12),
                    ("History", "HIST", "World History", AcademicLevel.HighSchool, 9, 12),
                    ("Physical Education", "PE", "Physical Education", AcademicLevel.Elementary, 1, 12),
                    ("Art", "ART", "Visual Arts", AcademicLevel.Elementary, 1, 12),
                    ("Music", "MUS", "Music Education", AcademicLevel.Elementary, 1, 12),
                    ("Computer Science", "CS", "Introduction to Computing", AcademicLevel.HighSchool, 9, 12),
                    ("Spanish", "SPAN", "Spanish Language", AcademicLevel.HighSchool, 9, 12),
                    ("Drama", "DRAMA", "Theater Arts", AcademicLevel.MiddleSchool, 6, 8)
                };

                foreach (var (name, code, description, level, minGrade, maxGrade) in schoolSubjects)
                {
                    var isElective = new[] { "ART", "MUS", "DRAMA" }.Contains(code);

                    subjects.Add(new Subject
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Code = $"{code}{rand.Next(100, 999)}",
                        ShortDescription = description,
                        DetailedDescription = $"Comprehensive {description.ToLower()} curriculum",
                        SubjectType = isElective ? SubjectType.ElectiveCourse : SubjectType.SchoolSubject,
                        AcademicLevel = level,
                        MinGradeLevel = minGrade,
                        MaxGradeLevel = maxGrade,
                        IsElective = isElective,
                        ElectiveType = isElective ? ElectiveType.Arts : null,
                        MaxStudents = isElective ? 25 : null,
                        HasLab = code == "SCI" || code == "CS"
                    });
                }
            }

            return subjects;
        }

    }