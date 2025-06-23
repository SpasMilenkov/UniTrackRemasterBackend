using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Api.Dto.User;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;
using UniTrackRemaster.Services.Storage;

namespace UniTrackRemaster.Services.User;

public class UserService(
    UserManager<ApplicationUser> userManager,
    IUnitOfWork unitOfWork,
    IFirebaseStorageService storageService,
    ILogger<UserService> logger)
    : IUserService
{
    public async Task<UserDetailsResponse> UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto profileDto)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        // Update user properties
        user.FirstName = profileDto.FirstName;
        user.LastName = profileDto.LastName;
        user.Email = profileDto.Email;
        user.PhoneNumber = profileDto.Phone;

        // Save the changes
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new ApplicationException("Failed to update user profile");
        }

        // Return updated user details
        return await GetUserDetailsAsync(userId);
    }

    public async Task<string> UploadProfileImageAsync(Guid userId, IFormFile image)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        try
        {
            // Upload image using the generic method
            string customPath = $"profile-images/{userId}";
            string imageUrl = await storageService.UploadFileAsync(image.OpenReadStream(), customPath);

            // Update user's profile image URL
            user.AvatarUrl = imageUrl;

            // Save the changes directly using UserManager
            var result = await userManager.UpdateAsync(user);
            return !result.Succeeded ? throw new ApplicationException("Failed to update user profile image") : imageUrl;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Error uploading profile image: {ex.Message}", ex);
        }
    }

    public async Task<ApplicationUser?> GetUserById(Guid id)
    {
        try
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user is null)
                logger.LogWarning("User with that id does not exist ${id}", id);
            return user;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while trying to fetch a user");
            throw;
        }
    }

    public async Task<UserDetailsResponse> GetUserDetailsAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        // Determine if the user is linked to an institution
        var admin = await unitOfWork.Admins.GetByUserIdAsync(userId);
        var student = await unitOfWork.Students.GetByUserIdAsync(userId);
        var teacher = await unitOfWork.Teachers.GetByUserIdAsync(userId);
        var parent = await unitOfWork.Parents.GetByUserIdAsync(userId);

        var isLinked = admin != null || student != null || teacher != null || parent != null;
        var role = DetermineUserRole(admin, student, teacher, parent);
        var department = admin?.Department;

        // Get profile image URL if it exists
        string profileImageUrl = null;
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            // Create a signed URL that's valid for a time period
            profileImageUrl = await storageService.CreateSignedUrl(user.AvatarUrl);
        }

        return new UserDetailsResponse(
            userId,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            profileImageUrl, // Use the signed URL
            department,
            isLinked,
            role
        );
    }

    public async Task<PrivacySettingsDto> GetPrivacySettingsAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user == null
            ? throw new ApplicationException("User not found")
            : new PrivacySettingsDto
        (
            user.DataAnalytics,
            user.EmailUpdates,
            user.MarketingEmails,
            user.ProfileVisibility
        );
    }

    public async Task<PrivacySettingsDto> UpdatePrivacySettingsAsync(Guid userId, UpdatePrivacySettingsDto settingsDto)
    {
        var user = await userManager.FindByIdAsync(userId.ToString()) ?? throw new ApplicationException("User not found");

        // Only update properties that are provided (not null)
        if (settingsDto.DataAnalytics.HasValue)
            user.DataAnalytics = settingsDto.DataAnalytics.Value;

        if (settingsDto.EmailUpdates.HasValue)
            user.EmailUpdates = settingsDto.EmailUpdates.Value;

        if (settingsDto.MarketingEmails.HasValue)
            user.MarketingEmails = settingsDto.MarketingEmails.Value;

        if (settingsDto.ProfileVisibility.HasValue)
            user.ProfileVisibility = settingsDto.ProfileVisibility.Value;

        var result = await userManager.UpdateAsync(user);
        return !result.Succeeded
            ? throw new ApplicationException("Failed to update privacy settings")
            : await GetPrivacySettingsAsync(userId);
    }

    public async Task<PaginatedUserListDto> GetAllUsersAsync(UserPaginationParams parameters, CancellationToken cancellationToken = default)
    {
        if (parameters.PageNumber < 1) parameters.PageNumber = 1;
        if (parameters.PageSize < 1 || parameters.PageSize > 100) parameters.PageSize = 20;

        // Create query for users
        var query = userManager.Users.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm) ||
                u.Email.ToLower().Contains(searchTerm) ||
                u.UserName.ToLower().Contains(searchTerm));
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            query = parameters.SortBy.ToLower() switch
            {
                "firstname" => parameters.Ascending ? query.OrderBy(u => u.FirstName) : query.OrderByDescending(u => u.FirstName),
                "lastname" => parameters.Ascending ? query.OrderBy(u => u.LastName) : query.OrderByDescending(u => u.LastName),
                "email" => parameters.Ascending ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
                _ => query.OrderBy(u => u.LastName) // Default sort
            };
        }
        else
        {
            // Default sort by lastname
            query = query.OrderBy(u => u.LastName);
        }

        // Temporarily get all users, we'll apply role filtering and pagination manually
        var allUsers = await query.ToListAsync(cancellationToken);

        // Get all role relationships in one go to avoid N+1 problem
        var allUserIds = allUsers.Select(u => u.Id).ToList();

        // Get all role mappings in a single batch
        var admins = (await unitOfWork.Admins.GetAllAsync())
            .Where(a => allUserIds.Contains(a.UserId))
            .ToList();

        var teachers = (await unitOfWork.Teachers.GetAllAsync())
            .Where(t => allUserIds.Contains(t.UserId))
            .ToList();

        var students = (await unitOfWork.Students.GetAllAsync())
            .Where(s => allUserIds.Contains(s.UserId))
            .ToList();

        var parents = (await unitOfWork.Parents.GetAllAsync())
            .Where(p => allUserIds.Contains(p.UserId))
            .ToList();

        // Create a dictionary for quick lookups
        var adminsByUserId = admins.GroupBy(a => a.UserId).ToDictionary(g => g.Key, g => g.ToList());
        var teachersByUserId = teachers.GroupBy(t => t.UserId).ToDictionary(g => g.Key, g => g.ToList());
        var studentsByUserId = students.GroupBy(s => s.UserId).ToDictionary(g => g.Key, g => g.ToList());
        var parentsByUserId = parents.GroupBy(p => p.UserId).ToDictionary(g => g.Key, g => g.ToList());

        // Apply role filtering if specified
        if (!string.IsNullOrWhiteSpace(parameters.Role))
        {
            switch (parameters.Role.ToLower())
            {
                case "admin":
                    allUsers = allUsers.Where(u => adminsByUserId.ContainsKey(u.Id)).ToList();
                    break;

                case "teacher":
                    allUsers = allUsers.Where(u => teachersByUserId.ContainsKey(u.Id)).ToList();
                    break;

                case "parent":
                    allUsers = allUsers.Where(u => parentsByUserId.ContainsKey(u.Id)).ToList();
                    break;

                case "student":
                    allUsers = allUsers.Where(u => studentsByUserId.ContainsKey(u.Id)).ToList();
                    break;

                case "guest":
                case "user":
                    // Users that are not linked to any institution
                    allUsers = allUsers.Where(u =>
                        !adminsByUserId.ContainsKey(u.Id) &&
                        !teachersByUserId.ContainsKey(u.Id) &&
                        !studentsByUserId.ContainsKey(u.Id) &&
                        !parentsByUserId.ContainsKey(u.Id)).ToList();
                    break;
            }
        }

        // Get total count after role filtering
        var totalCount = allUsers.Count;

        // Apply pagination manually
        var paginatedUsers = allUsers
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        // Calculate pagination metadata
        var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);
        var hasNextPage = parameters.PageNumber < totalPages;
        var hasPreviousPage = parameters.PageNumber > 1;

        // Convert to UserDetailsResponse objects
        var userResponses = await ConvertUsersToResponseDtos(
            paginatedUsers,
            adminsByUserId,
            teachersByUserId,
            studentsByUserId,
            parentsByUserId,
            cancellationToken);

        return new PaginatedUserListDto
        {
            Users = userResponses,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalPages = totalPages,
            HasPreviousPage = hasPreviousPage,
            HasNextPage = hasNextPage
        };
    }

    private async Task<List<UserDetailsResponse>> ConvertUsersToResponseDtos(
        List<ApplicationUser> users,
        Dictionary<Guid, List<Admin>> adminsByUserId,
        Dictionary<Guid, List<Teacher>> teachersByUserId,
        Dictionary<Guid, List<Student>> studentsByUserId,
        Dictionary<Guid, List<Parent>> parentsByUserId,
        CancellationToken cancellationToken = default)
    {
        var userResponses = new List<UserDetailsResponse>();
        var institutionCache = new Dictionary<Guid, string>(); // Cache to avoid duplicate db queries

        foreach (var user in users)
        {
            // Get all roles for this user using the dictionaries
            var userAdmins = adminsByUserId.TryGetValue(user.Id, out var admins) ? admins : new List<Admin>();
            var userTeachers = teachersByUserId.TryGetValue(user.Id, out var teachers) ? teachers : new List<Teacher>();
            var userStudents = studentsByUserId.TryGetValue(user.Id, out var students) ? students : new List<Student>();
            var userParents = parentsByUserId.TryGetValue(user.Id, out var parents) ? parents : new List<Parent>();

            var isLinked = userAdmins.Count != 0 || userTeachers.Count != 0 || userStudents.Count != 0 || userParents.Count != 0;

            // Determine primary role - prioritize Admin > Teacher > Parent > Student > Guest
            var role = DetermineUserRole(userAdmins, userTeachers, userParents, userStudents);

            // Get department from first admin account if applicable
            var department = userAdmins.FirstOrDefault()?.Department;

            string? profileImageUrl = null;
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                profileImageUrl = await storageService.CreateSignedUrl(user.AvatarUrl);
            }

            // Get primary institution information based on the primary role
            string? institutionId = null;
            string? institutionName = null;

            // Try to get institution based on primary role
            if (role == "admin" && userAdmins.Count != 0)
            {
                var primaryAdmin = userAdmins.First();
                institutionId = primaryAdmin.InstitutionId.ToString();
                institutionName = await GetInstitutionName(primaryAdmin.InstitutionId, institutionCache, cancellationToken);
            }
            else if (role == "teacher" && userTeachers.Count != 0)
            {
                var primaryTeacher = userTeachers.First();
                institutionId = primaryTeacher.InstitutionId.ToString();
                institutionName = await GetInstitutionName(primaryTeacher.InstitutionId, institutionCache, cancellationToken);
            }
            else if (role == "parent" && userParents.Count != 0)
            {
                var primaryParent = userParents.First();
                // For parents, get institution through their children
                var children = await unitOfWork.Parents.GetChildrenAsync(primaryParent.Id);
                if (children.Count != 0)
                {
                    var firstChild = children.First();
                    if (firstChild.SchoolId.HasValue && firstChild.SchoolId.Value != Guid.Empty)
                    {
                        var school = await unitOfWork.Schools.GetByIdAsync(firstChild.SchoolId.Value, cancellationToken);
                        if (school != null && school.InstitutionId != Guid.Empty)
                        {
                            institutionId = school.InstitutionId.ToString();
                            institutionName = await GetInstitutionName(school.InstitutionId, institutionCache, cancellationToken);
                        }
                    }
                    else if (firstChild.UniversityId.HasValue && firstChild.UniversityId.Value != Guid.Empty)
                    {
                        var university = await unitOfWork.Universities.GetByIdAsync(firstChild.UniversityId.Value, cancellationToken);
                        if (university != null && university.InstitutionId != Guid.Empty)
                        {
                            institutionId = university.InstitutionId.ToString();
                            institutionName = await GetInstitutionName(university.InstitutionId, institutionCache, cancellationToken);
                        }
                    }
                }
            }
            else if (role == "student" && userStudents.Count != 0)
            {
                var primaryStudent = userStudents.First();
                if (primaryStudent.SchoolId.HasValue && primaryStudent.SchoolId.Value != Guid.Empty)
                {
                    var school = await unitOfWork.Schools.GetByIdAsync(primaryStudent.SchoolId.Value, cancellationToken);
                    if (school != null && school.InstitutionId != Guid.Empty)
                    {
                        institutionId = school.InstitutionId.ToString();
                        institutionName = await GetInstitutionName(school.InstitutionId, institutionCache, cancellationToken);
                    }
                }
                else if (primaryStudent.UniversityId.HasValue && primaryStudent.UniversityId.Value != Guid.Empty)
                {
                    var university = await unitOfWork.Universities.GetByIdAsync(primaryStudent.UniversityId.Value, cancellationToken);
                    if (university != null && university.InstitutionId != Guid.Empty)
                    {
                        institutionId = university.InstitutionId.ToString();
                        institutionName = await GetInstitutionName(university.InstitutionId, institutionCache, cancellationToken);
                    }
                }
            }

            // Create a list of all roles this user has
            var allRoles = new List<string>();
            if (userAdmins.Count != 0) allRoles.Add("admin");
            if (userTeachers.Count != 0) allRoles.Add("teacher");
            if (userParents.Count != 0) allRoles.Add("parent");
            if (userStudents.Count != 0) allRoles.Add("student");
            if (allRoles.Count == 0) allRoles.Add("guest");

            userResponses.Add(new UserDetailsResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                profileImageUrl,
                department,
                isLinked,
                role,
                institutionId,
                institutionName,
                allRoles // Pass all roles the user has
            ));
        }

        return userResponses;
    }

    // Helper method to get institution name with caching
    private async Task<string?> GetInstitutionName(Guid institutionId, Dictionary<Guid, string> cache, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(institutionId, out var name))
        {
            return name;
        }

        var institution = await unitOfWork.Institutions.GetByIdAsync(institutionId, cancellationToken);
        if (institution != null)
        {
            cache[institutionId] = institution.Name;
            return institution.Name;
        }

        return null;
    }

    // Updated method to determine primary role from multiple possibilities
    private string DetermineUserRole(Admin? admin, Student? student, Teacher? teacher, Parent? parent)
    {
        if (admin != null) return "admin";
        if (teacher != null) return "teacher";
        if (parent != null) return "parent";
        if (student != null) return "student";
        return "guest";
    }

    private string DetermineUserRole(List<Admin> admins, List<Teacher> teachers, List<Parent> parents, List<Student> students)
    {
        if (admins.Count != 0) return "admin";
        if (teachers.Count != 0) return "teacher";
        if (parents.Count != 0) return "parent";
        if (students.Count != 0) return "student";
        return "guest";
    }

    public async Task<List<InstitutionDto>> GetUserInstitutionsAsync(Guid userId)
    {
        var institutions = new List<InstitutionDto>();
        var processedInstitutionIds = new HashSet<Guid>();

        try
        {
            // Check if user is an admin
            var admin = await unitOfWork.Admins.GetByUserIdAsync(userId);
            if (admin != null && !processedInstitutionIds.Contains(admin.InstitutionId))
            {
                var institution = await unitOfWork.Institutions.GetByIdAsync(admin.InstitutionId);
                if (institution != null)
                {
                    var institutionDto = await CreateInstitutionDto(institution);
                    institutions.Add(institutionDto);
                    processedInstitutionIds.Add(institution.Id);
                }
            }

            // Check if user is a teacher
            var teacher = await unitOfWork.Teachers.GetByUserIdAsync(userId);
            if (teacher != null && !processedInstitutionIds.Contains(teacher.InstitutionId))
            {
                var institution = await unitOfWork.Institutions.GetByIdAsync(teacher.InstitutionId);
                if (institution != null)
                {
                    var institutionDto = await CreateInstitutionDto(institution);
                    institutions.Add(institutionDto);
                    processedInstitutionIds.Add(institution.Id);
                }
            }

            // Check if user is a parent
            var parent = await unitOfWork.Parents.GetByUserIdAsync(userId);
            if (parent != null)
            {
                var children = await unitOfWork.Parents.GetChildrenAsync(parent.Id);
                
                foreach (var child in children)
                {
                    // Check school
                    if (child.SchoolId.HasValue && child.SchoolId.Value != Guid.Empty)
                    {
                        var school = await unitOfWork.Schools.GetByIdAsync(child.SchoolId.Value);
                        if (school != null && school.InstitutionId != Guid.Empty && !processedInstitutionIds.Contains(school.InstitutionId))
                        {
                            var institution = await unitOfWork.Institutions.GetByIdAsync(school.InstitutionId);
                            if (institution != null)
                            {
                                var institutionDto = await CreateInstitutionDto(institution);
                                institutions.Add(institutionDto);
                                processedInstitutionIds.Add(institution.Id);
                            }
                        }
                    }

                    // Check university
                    if (child.UniversityId.HasValue && child.UniversityId.Value != Guid.Empty)
                    {
                        var university = await unitOfWork.Universities.GetByIdAsync(child.UniversityId.Value);
                        if (university != null && university.InstitutionId != Guid.Empty && !processedInstitutionIds.Contains(university.InstitutionId))
                        {
                            var institution = await unitOfWork.Institutions.GetByIdAsync(university.InstitutionId);
                            if (institution != null)
                            {
                                var institutionDto = await CreateInstitutionDto(institution);
                                institutions.Add(institutionDto);
                                processedInstitutionIds.Add(institution.Id);
                            }
                        }
                    }
                }
            }

            // Check if user is a student
            var student = await unitOfWork.Students.GetByUserIdAsync(userId);
            if (student != null)
            {
                // Check school
                if (student.SchoolId.HasValue && student.SchoolId.Value != Guid.Empty)
                {
                    var school = await unitOfWork.Schools.GetByIdAsync(student.SchoolId.Value);
                    if (school != null && school.InstitutionId != Guid.Empty && !processedInstitutionIds.Contains(school.InstitutionId))
                    {
                        var institution = await unitOfWork.Institutions.GetByIdAsync(school.InstitutionId);
                        if (institution != null)
                        {
                            var institutionDto = await CreateInstitutionDto(institution);
                            institutions.Add(institutionDto);
                            processedInstitutionIds.Add(institution.Id);
                        }
                    }
                }

                // Check university
                if (student.UniversityId.HasValue && student.UniversityId.Value != Guid.Empty)
                {
                    var university = await unitOfWork.Universities.GetByIdAsync(student.UniversityId.Value);
                    if (university != null && university.InstitutionId != Guid.Empty && !processedInstitutionIds.Contains(university.InstitutionId))
                    {
                        var institution = await unitOfWork.Institutions.GetByIdAsync(university.InstitutionId);
                        if (institution != null)
                        {
                            var institutionDto = await CreateInstitutionDto(institution);
                            institutions.Add(institutionDto);
                            processedInstitutionIds.Add(institution.Id);
                        }
                    }
                }
            }

            return institutions;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user institutions for user {UserId}", userId);
            return new List<InstitutionDto>();
        }
    }

    // Helper method to create InstitutionDto with signed URLs
    private async Task<InstitutionDto> CreateInstitutionDto(Data.Models.Organizations.Institution institution)
    {
        // Get all image URLs with signed URLs
        var imageUrls = new List<string>();
        if (institution.Images != null && institution.Images.Any())
        {
            foreach (var image in institution.Images)
            {
                if (!string.IsNullOrEmpty(image.Url))
                {
                    var signedUrl = await storageService.CreateSignedUrl(image.Url);
                    imageUrls.Add(signedUrl);
                }
            }
        }

        // Get logo URL with signed URL
        string logoUrl = string.Empty;
        if (!string.IsNullOrEmpty(institution.LogoUrl))
        {
            logoUrl = await storageService.CreateSignedUrl(institution.LogoUrl);
        }

        return InstitutionDto.FromEntity(institution, imageUrls, logoUrl);
    }

    public async Task<bool> UserHasAccessToGroupAsync(Guid userId, Guid groupId, string groupType)
    {
        try
        {
            switch (groupType.ToLower())
            {
                case "institution":
                    // For institution-level chats, check if the user is associated with this institution
                    // The groupId in this case is the institutionId

                    // Check if user is an admin of this institution
                    var admin = await unitOfWork.Admins.GetByUserIdAsync(userId);
                    if (admin != null && admin.InstitutionId == groupId)
                    {
                        return true;
                    }

                    // Check if user is a teacher in this institution
                    var teacher = await unitOfWork.Teachers.GetByUserIdAsync(userId);
                    if (teacher != null && teacher.InstitutionId == groupId)
                    {
                        return true;
                    }

                    // Check if user is a parent with children in this institution
                    var parent = await unitOfWork.Parents.GetByUserIdAsync(userId);
                    if (parent != null)
                    {
                        var children = await unitOfWork.Parents.GetChildrenAsync(parent.Id);
                        foreach (var child in children)
                        {
                            // Check via school
                            if (child.SchoolId.HasValue)
                            {
                                var school = await unitOfWork.Schools.GetByIdAsync(child.SchoolId.Value);
                                if (school != null && school.InstitutionId == groupId)
                                {
                                    return true;
                                }
                            }

                            // Check via university
                            if (child.UniversityId.HasValue)
                            {
                                var university = await unitOfWork.Universities.GetByIdAsync(child.UniversityId.Value);
                                if (university != null && university.InstitutionId == groupId)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    // Check if user is a student in this institution
                    var student = await unitOfWork.Students.GetByUserIdAsync(userId);
                    if (student != null)
                    {
                        // Check via school
                        if (student.SchoolId.HasValue)
                        {
                            var school = await unitOfWork.Schools.GetByIdAsync(student.SchoolId.Value);
                            if (school != null && school.InstitutionId == groupId)
                            {
                                return true;
                            }
                        }

                        // Check via university
                        if (student.UniversityId.HasValue)
                        {
                            var university = await unitOfWork.Universities.GetByIdAsync(student.UniversityId.Value);
                            if (university != null && university.InstitutionId == groupId)
                            {
                                return true;
                            }
                        }
                    }

                    break;

                case "class":
                case "grade":
                    // For class/grade-level chats, the groupId would be the gradeId
                    // Check if user is a student in this grade
                    var studentForGrade = await unitOfWork.Students.GetByUserIdAsync(userId);
                    if (studentForGrade != null && studentForGrade.GradeId == groupId)
                    {
                        return true;
                    }

                    // Check if user is a teacher for this grade
                    var teacherForGrade = await unitOfWork.Teachers.GetByUserIdAsync(userId);
                    if (teacherForGrade != null)
                    {
                        // Check if teacher is homeroom teacher for this grade
                        if (teacherForGrade.ClassGradeId == groupId)
                        {
                            return true;
                        }

                        // With the new structure, check if teacher teaches any subjects to this grade
                        // Get all subjects for this teacher
                        var teacherSubjects = teacherForGrade.Subjects;
                        if (teacherSubjects != null && teacherSubjects.Any())
                        {
                            // Check if any of the teacher's subjects are associated with this grade
                            foreach (var teacherSubject in teacherSubjects)
                            {
                                if (teacherSubject.Grades != null && teacherSubject.Grades.Any(g => g.Id == groupId))
                                {
                                    return true;
                                }
                            }
                        }

                        // Alternative: If teacher has Grades collection directly
                        if (teacherForGrade.Grades != null && teacherForGrade.Grades.Any(g => g.Id == groupId))
                        {
                            return true;
                        }
                    }

                    // Check if user is a parent with children in this grade
                    var parentForGrade = await unitOfWork.Parents.GetByUserIdAsync(userId);
                    if (parentForGrade != null)
                    {
                        var children = await unitOfWork.Parents.GetChildrenAsync(parentForGrade.Id);
                        if (children.Any(child => child.GradeId == groupId))
                        {
                            return true;
                        }
                    }

                    // Check if user is admin of the institution containing this grade
                    var grade = await unitOfWork.Grades.GetByIdAsync(groupId);
                    if (grade != null)
                    {
                        var adminForGrade = await unitOfWork.Admins.GetByUserIdAsync(userId);
                        if (adminForGrade != null && adminForGrade.InstitutionId == grade.InstitutionId)
                        {
                            return true;
                        }
                    }

                    break;

                case "department":
                    // For department-level chats (university), groupId would be departmentId
                    var department = await unitOfWork.Departments.GetByIdAsync(groupId);
                    if (department != null)
                    {
                        // Check if user is a teacher in this department
                        var departmentTeacher = await unitOfWork.Teachers.GetByUserIdAsync(userId);
                        if (departmentTeacher != null)
                        {
                            // Check if teacher has any subjects in this department
                            if (departmentTeacher.Subjects != null &&
                                departmentTeacher.Subjects.Any(s => s.DepartmentId == groupId))
                            {
                                return true;
                            }
                        }

                        // Check if user is admin of the faculty/university containing this department
                        var facultyAdmin = await unitOfWork.Admins.GetByUserIdAsync(userId);
                        if (facultyAdmin != null)
                        {
                            // Get the faculty this department belongs to
                            var faculty = await unitOfWork.Faculties.GetByIdAsync(department.FacultyId);
                            if (faculty != null)
                            {
                                var university = await unitOfWork.Universities.GetByIdAsync(faculty.UniversityId);
                                if (university != null && university.InstitutionId == facultyAdmin.InstitutionId)
                                {
                                    return true;
                                }
                            }
                        }

                        // Check if user is a parent with children in this department (through majors)
                        var parentForDept = await unitOfWork.Parents.GetByUserIdAsync(userId);
                        if (parentForDept != null)
                        {
                            var children = await unitOfWork.Parents.GetChildrenAsync(parentForDept.Id);
                            foreach (var child in children)
                            {
                                if (child.Major != null && child.Major.Faculty != null &&
                                    child.Major.Faculty.Departments != null &&
                                    child.Major.Faculty.Departments.Any(d => d.Id == groupId))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    break;

                case "subject":
                    // For subject-specific chats, groupId would be subjectId
                    var subject = await unitOfWork.Subjects.GetByIdAsync(groupId);
                    if (subject != null)
                    {
                        // Check if user is a teacher for this subject
                        var subjectTeacher = await unitOfWork.Teachers.GetByUserIdAsync(userId);
                        if (subjectTeacher != null)
                        {
                            // Check if this teacher is the primary teacher
                            if (subject.PrimaryTeacherId == subjectTeacher.Id)
                            {
                                return true;
                            }

                            // Check if teacher is in the subject's teachers collection
                            if (subject.Teachers != null && subject.Teachers.Any(t => t.Id == subjectTeacher.Id))
                            {
                                return true;
                            }

                            // Alternative: Check from teacher's side
                            if (subjectTeacher.Subjects != null && subjectTeacher.Subjects.Any(s => s.Id == groupId))
                            {
                                return true;
                            }
                        }

                        // Check if user is a student in any grade that has this subject
                        var subjectStudent = await unitOfWork.Students.GetByUserIdAsync(userId);
                        if (subjectStudent != null && subjectStudent.GradeId != Guid.Empty)
                        {
                            // Check if the subject is taught to the student's grade
                            if (subject.Grades != null && subject.Grades.Any(g => g.Id == subjectStudent.GradeId))
                            {
                                return true;
                            }

                            // Check elective enrollment if this is an elective subject
                            if (subject.IsElective && subjectStudent.Electives != null)
                            {
                                // Check if student is enrolled in this elective
                                var isEnrolled = subjectStudent.Electives.Any(e => e.SubjectId == groupId);
                                if (isEnrolled)
                                {
                                    return true;
                                }
                            }
                        }

                        // Check if user is a parent with children taking this subject
                        var parentForSubject = await unitOfWork.Parents.GetByUserIdAsync(userId);
                        if (parentForSubject != null)
                        {
                            var children = await unitOfWork.Parents.GetChildrenAsync(parentForSubject.Id);
                            foreach (var child in children)
                            {
                                // Check if child's grade has this subject
                                if (subject.Grades != null && subject.Grades.Any(g => g.Id == child.GradeId))
                                {
                                    return true;
                                }

                                // Check if child is enrolled in this elective
                                if (subject.IsElective && child.Electives != null &&
                                    child.Electives.Any(e => e.SubjectId == groupId))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    break;
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking group access for user {UserId} and group {GroupId}", userId, groupId);
            return false;
        }
    }

    public async Task<bool> IsUserAdminAsync(Guid userId)
    {
        try
        {
            var user = await userManager.FindByIdAsync(userId.ToString()) ?? throw new ApplicationException("User not found");

            // Check if user is SuperAdmin first
            var isSuperAdmin = await userManager.IsInRoleAsync(user, "SuperAdmin");

            if (isSuperAdmin)
                return true;

            // Check if user is Admin with Active status
            var admin = await unitOfWork.Admins.GetByUserIdAsync(userId);
            return admin != null && admin.Status == ProfileStatus.Active;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking admin status for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsUserSuperAdminAsync(Guid userId)
    {
        try
        {
            var user = await userManager.FindByIdAsync(userId.ToString()) ?? throw new ApplicationException("User not found");

            // Check if user is SuperAdmin first
            return await userManager.IsInRoleAsync(user, "SuperAdmin");

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking super admin status for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> CanUserManageChatMessageAsync(Guid userId, Guid? recipientId, Guid? groupId)
    {
        try
        {
            // SuperAdmins can manage any message
            if (await IsUserSuperAdminAsync(userId))
                return true;

            // For direct messages (recipientId is set)
            if (recipientId.HasValue && !groupId.HasValue)
            {
                // For direct messages, check if admin has privileges in any shared institution
                return await CanAdminManageDirectMessageAsync(userId, recipientId.Value);
            }

            // For group messages (groupId is set) - groupId is the institutionId
            if (groupId.HasValue)
            {
                // Check if user is admin in the specific institution
                return await IsUserAdminInInstitutionAsync(userId, groupId.Value);
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking chat message management permissions for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsUserAdminInInstitutionAsync(Guid userId, Guid institutionId)
    {
        try
        {
            // SuperAdmins can manage anything
            if (await IsUserSuperAdminAsync(userId))
                return true;

            // Check if user is active admin in the specific institution
            var admin = await unitOfWork.Admins.GetByUserIdAsync(userId);
            return admin != null &&
                   admin.InstitutionId == institutionId &&
                   admin.Status == ProfileStatus.Active;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking admin status for user {UserId} in institution {InstitutionId}", userId, institutionId);
            return false;
        }
    }

    private async Task<bool> CanAdminManageDirectMessageAsync(Guid adminUserId, Guid otherUserId)
    {
        try
        {
            // SuperAdmins can manage any direct message
            if (await IsUserSuperAdminAsync(adminUserId))
                return true;

            // Get the admin's institution
            var admin = await unitOfWork.Admins.GetByUserIdAsync(adminUserId);
            if (admin == null || admin.Status != ProfileStatus.Active)
                return false;

            var adminInstitutionId = admin.InstitutionId;

            // Check if the other user belongs to the same institution
            // Check if other user is a student
            var student = await unitOfWork.Students.GetByUserIdAsync(otherUserId);
            if (student != null)
            {
                // Check via school
                if (student.SchoolId.HasValue)
                {
                    var school = await unitOfWork.Schools.GetByIdAsync(student.SchoolId.Value);
                    if (school != null && school.InstitutionId == adminInstitutionId)
                        return true;
                }

                // Check via university
                if (student.UniversityId.HasValue)
                {
                    var university = await unitOfWork.Universities.GetByIdAsync(student.UniversityId.Value);
                    if (university != null && university.InstitutionId == adminInstitutionId)
                        return true;
                }
            }

            // Check if other user is a teacher in the same institution
            var teacher = await unitOfWork.Teachers.GetByUserIdAsync(otherUserId);
            if (teacher != null && teacher.InstitutionId == adminInstitutionId)
                return true;

            // Check if other user is an admin in the same institution
            var otherAdmin = await unitOfWork.Admins.GetByUserIdAsync(otherUserId);
            if (otherAdmin != null && otherAdmin.InstitutionId == adminInstitutionId)
                return true;

            // Check if other user is a parent with children in the same institution
            var parent = await unitOfWork.Parents.GetByUserIdAsync(otherUserId);
            if (parent != null)
            {
                var children = await unitOfWork.Parents.GetChildrenAsync(parent.Id);
                foreach (var child in children)
                {
                    // Check via school
                    if (child.SchoolId.HasValue)
                    {
                        var school = await unitOfWork.Schools.GetByIdAsync(child.SchoolId.Value);
                        if (school != null && school.InstitutionId == adminInstitutionId)
                            return true;
                    }

                    // Check via university
                    if (child.UniversityId.HasValue)
                    {
                        var university = await unitOfWork.Universities.GetByIdAsync(child.UniversityId.Value);
                        if (university != null && university.InstitutionId == adminInstitutionId)
                            return true;
                    }
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking direct message management permissions for admin {AdminUserId} and user {OtherUserId}", adminUserId, otherUserId);
            return false;
        }
    }
}