using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Enums;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.Authentication;

public class RoleService(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IUnitOfWork unitOfWork,
    ILogger<RoleService> logger)
    : IRoleService
{
    public async Task<List<ApplicationRole>> GetAllRolesAsync()
    {
        try
        {
            return await roleManager.Roles.ToListAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving all roles");
            throw;
        }
    }

    public async Task<List<ApplicationRole>> GetPublicRolesAsync()
    {
        try
        {
            return await roleManager.Roles
                .Where(r => !new[] { nameof(Roles.SuperAdmin), nameof(Roles.Admin), nameof(Roles.Guest) }
                    .Contains(r.Name))
                .ToListAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving public roles");
            throw;
        }
    }

    public async Task<List<string>> GetUserRolesInInstitutionAsync(Guid userId, Guid institutionId)
    {
        try
        {
            var roles = new List<string>();

            // Check if user is a SuperAdmin (they have access to everything)
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new Exception("User not found");

            var userRoles = await userManager.GetRolesAsync(user);
            if (userRoles.Contains(nameof(Roles.SuperAdmin)))
            {
                roles.Add(nameof(Roles.SuperAdmin));
                return roles;
            }

            // Check if user is an Admin in this institution
            var admin = await unitOfWork.Admins.GetByUserIdAsync(userId);
            if (admin != null && admin.InstitutionId == institutionId)
            {
                roles.Add(nameof(Roles.Admin));
            }

            // Check if user is a Teacher in this institution
            var teacher = await unitOfWork.Teachers.GetByUserIdAsync(userId);
            if (teacher != null && teacher.InstitutionId == institutionId)
            {
                roles.Add(nameof(Roles.Teacher));
            }

            // Check if user is a Student in this institution
            var student = await unitOfWork.Students.GetByUserIdAsync(userId);
            if (student != null)
            {
                // Check if the student is associated with this institution
                // We need to look up the actual institution IDs through school or university
                bool isStudentOfInstitution = false;

                if (student.SchoolId.HasValue)
                {
                    var school = await unitOfWork.Schools.GetByIdAsync(student.SchoolId.Value);
                    if (school != null && school.InstitutionId == institutionId)
                    {
                        isStudentOfInstitution = true;
                    }
                }

                if (!isStudentOfInstitution && student.UniversityId.HasValue)
                {
                    var university = await unitOfWork.Universities.GetByIdAsync(student.UniversityId.Value);
                    if (university != null && university.InstitutionId == institutionId)
                    {
                        isStudentOfInstitution = true;
                    }
                }

                if (isStudentOfInstitution)
                {
                    roles.Add(nameof(Roles.Student));
                }
            }

            // Check if user is a Parent with children in this institution
            var parent = await unitOfWork.Parents.GetByUserIdAsync(userId);
            if (parent != null)
            {
                // Get children for this parent
                var children = await unitOfWork.Parents.GetChildrenAsync(parent.Id);
                var hasChildrenInInstitution = false;

                foreach (var child in children)
                {
                    // Check if child is in a school associated with this institution
                    if (child.SchoolId.HasValue)
                    {
                        var school = await unitOfWork.Schools.GetByIdAsync(child.SchoolId.Value);
                        if (school != null && school.InstitutionId == institutionId)
                        {
                            hasChildrenInInstitution = true;
                            break;
                        }
                    }

                    // Check if child is in a university associated with this institution
                    if (!hasChildrenInInstitution && child.UniversityId.HasValue)
                    {
                        var university = await unitOfWork.Universities.GetByIdAsync(child.UniversityId.Value);
                        if (university != null && university.InstitutionId == institutionId)
                        {
                            hasChildrenInInstitution = true;
                            break;
                        }
                    }
                }

                if (hasChildrenInInstitution)
                {
                    roles.Add(nameof(Roles.Parent));
                }
            }

            return roles;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving user roles in institution for user {UserId} in institution {InstitutionId}", userId, institutionId);
            throw;
        }
    }

    public async Task<bool> CreateRoleAsync(string roleName)
    {
        try
        {
            if (await roleManager.RoleExistsAsync(roleName))
                return false;

            var result = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
            return result.Succeeded;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating role {RoleName}", roleName);
            throw;
        }
    }

    public async Task<bool> IsUserInRole(Guid userId, string roleName)
    {
        try
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            return await userManager.IsInRoleAsync(user, roleName);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error checking if user {UserId} is in role {RoleName}", userId, roleName);
            throw;
        }
    }
}