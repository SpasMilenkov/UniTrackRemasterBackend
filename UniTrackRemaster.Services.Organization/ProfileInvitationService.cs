using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Invitations;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Organizations;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.Organization;


public class ProfileInvitationService(
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager,
    ILogger<ProfileInvitationService> logger) : IProfileInvitationService
{
    public async Task<IEnumerable<PendingInvitationDto>> GetPendingInvitationsAsync(Guid userId)
    {
        try
        {
            var invitations = new List<PendingInvitationDto>();

            // Get pending student invitations
            var studentInvitations = await unitOfWork.Students.GetPendingByUserIdAsync(userId);
            foreach (var student in studentInvitations)
            {
                invitations.Add(new PendingInvitationDto
                {
                    Id = student.Id,
                    Type = "Student",
                    InstitutionId = student.SchoolId ?? student.UniversityId ?? Guid.Empty,
                    InstitutionName = student.School?.Institution?.Name ?? student.University?.Institution?.Name ?? "Unknown",
                    Role = student.IsSchoolStudent ? "School Student" : "University Student",
                    GradeName = student.Grade?.Name,
                    AdditionalInfo = $"Grade: {student.Grade?.Name}",
                    InvitedAt = student.CreatedAt,
                    Status = student.Status
                });
            }

            // Get pending teacher invitations
            var teacherInvitations = await unitOfWork.Teachers.GetPendingByUserIdAsync(userId);
            foreach (var teacher in teacherInvitations)
            {
                var institution = await unitOfWork.Institutions.GetByIdAsync(teacher.InstitutionId);
                invitations.Add(new PendingInvitationDto
                {
                    Id = teacher.Id,
                    Type = "Teacher",
                    InstitutionId = teacher.InstitutionId,
                    InstitutionName = institution?.Name ?? "Unknown",
                    Role = teacher.Title,
                    GradeName = teacher.ClassGrade?.Name,
                    AdditionalInfo = $"Title: {teacher.Title}" + (teacher.ClassGrade != null ? $", Class: {teacher.ClassGrade.Name}" : ""),
                    InvitedAt = teacher.CreatedAt,
                    Status = teacher.Status
                });
            }

            // Get pending admin invitations
            var adminInvitations = await unitOfWork.Admins.GetPendingByUserIdAsync(userId);
            foreach (var admin in adminInvitations)
            {
                var institution = await unitOfWork.Institutions.GetByIdAsync(admin.InstitutionId);
                invitations.Add(new PendingInvitationDto
                {
                    Id = admin.Id,
                    Type = "Admin",
                    InstitutionId = admin.InstitutionId,
                    InstitutionName = institution?.Name ?? "Unknown",
                    Role = admin.Role.ToString(),
                    AdditionalInfo = $"Position: {admin.Position}" + (admin.Department != null ? $", Department: {admin.Department}" : ""),
                    InvitedAt = admin.CreatedAt,
                    Status = admin.Status
                });
            }

            return invitations.OrderByDescending(i => i.InvitedAt);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting pending invitations for user {UserId}", userId);
            throw;
        }
    }

    public async Task<AcceptInvitationResponseDto> AcceptInvitationAsync(Guid userId, AcceptInvitationDto dto)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var user = await userManager.FindByIdAsync(userId.ToString()) ??
                throw new NotFoundException("User not found");

            switch (dto.ProfileType.ToLower())
            {
                case "student":
                    return await AcceptStudentInvitationAsync(user, dto.ProfileId);
                case "teacher":
                    return await AcceptTeacherInvitationAsync(user, dto.ProfileId);
                case "admin":
                    return await AcceptAdminInvitationAsync(user, dto.ProfileId);
                default:
                    throw new ValidationException("Invalid profile type");
            }
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error accepting invitation for user {UserId}", userId);
            throw;
        }
    }

    public async Task DeclineInvitationAsync(Guid userId, DeclineInvitationDto dto)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            switch (dto.ProfileType.ToLower())
            {
                case "student":
                    var student = await unitOfWork.Students.GetByIdAsync(dto.ProfileId);
                    if (student == null || student.UserId != userId)
                        throw new NotFoundException("Student invitation not found");

                    student.Status = ProfileStatus.Rejected;
                    await unitOfWork.Students.UpdateAsync(student);
                    break;

                case "teacher":
                    var teacher = await unitOfWork.Teachers.GetByIdAsync(dto.ProfileId);
                    if (teacher == null || teacher.UserId != userId)
                        throw new NotFoundException("Teacher invitation not found");

                    teacher.Status = ProfileStatus.Rejected;
                    await unitOfWork.Teachers.UpdateAsync(teacher);
                    break;

                case "admin":
                    var admin = await unitOfWork.Admins.GetByIdAsync(dto.ProfileId);
                    if (admin == null || admin.UserId != userId)
                        throw new NotFoundException("Admin invitation not found");

                    admin.Status = ProfileStatus.Rejected;
                    await unitOfWork.Admins.UpdateAsync(admin);
                    break;

                default:
                    throw new ValidationException("Invalid profile type");
            }

            await unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error declining invitation for user {UserId}", userId);
            throw;
        }
    }

    public async Task ResendInvitationAsync(ResendInvitationDto dto)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            switch (dto.ProfileType.ToLower())
            {
                case "student":
                    var student = await unitOfWork.Students.GetByIdAsync(dto.ProfileId);
                    if (student == null)
                        throw new NotFoundException("Student not found");

                    student.Status = ProfileStatus.Pending;
                    await unitOfWork.Students.UpdateAsync(student);
                    break;

                case "teacher":
                    var teacher = await unitOfWork.Teachers.GetByIdAsync(dto.ProfileId);
                    if (teacher == null)
                        throw new NotFoundException("Teacher not found");

                    teacher.Status = ProfileStatus.Pending;
                    await unitOfWork.Teachers.UpdateAsync(teacher);
                    break;

                case "admin":
                    var admin = await unitOfWork.Admins.GetByIdAsync(dto.ProfileId);
                    if (admin == null)
                        throw new NotFoundException("Admin not found");

                    admin.Status = ProfileStatus.Pending;
                    await unitOfWork.Admins.UpdateAsync(admin);
                    break;

                default:
                    throw new ValidationException("Invalid profile type");
            }

            await unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error resending invitation for profile {ProfileId}", dto.ProfileId);
            throw;
        }
    }

    public async Task<IEnumerable<InstitutionInvitationDto>> GetInstitutionInvitationsAsync(Guid institutionId, ProfileStatus? status = null)
    {
        try
        {
            var invitations = new List<InstitutionInvitationDto>();

            // Get student invitations
            var students = await unitOfWork.Students.GetByInstitutionAsync(institutionId, status);
            foreach (var student in students)
            {
                invitations.Add(new InstitutionInvitationDto
                {
                    Id = student.Id,
                    Type = "Student",
                    UserName = $"{student.User?.FirstName} {student.User?.LastName}",
                    Email = student.User?.Email ?? "",
                    Role = student.IsSchoolStudent ? "School Student" : "University Student",
                    Status = student.Status,
                    InvitedAt = student.CreatedAt,
                    AcceptedAt = student.Status == ProfileStatus.Active ? student.UpdatedAt : null
                });
            }

            // Get teacher invitations
            var teachers = await unitOfWork.Teachers.GetByInstitutionAsync(institutionId, status);
            foreach (var teacher in teachers)
            {
                invitations.Add(new InstitutionInvitationDto
                {
                    Id = teacher.Id,
                    Type = "Teacher",
                    UserName = $"{teacher.User?.FirstName} {teacher.User?.LastName}",
                    Email = teacher.User?.Email ?? "",
                    Role = teacher.Title,
                    Status = teacher.Status,
                    InvitedAt = teacher.CreatedAt,
                    AcceptedAt = teacher.Status == ProfileStatus.Active ? teacher.UpdatedAt : null
                });
            }

            // Get admin invitations
            var admins = await unitOfWork.Admins.GetByInstitutionAsync(institutionId, status);
            foreach (var admin in admins)
            {
                invitations.Add(new InstitutionInvitationDto
                {
                    Id = admin.Id,
                    Type = "Admin",
                    UserName = $"{admin.User?.FirstName} {admin.User?.LastName}",
                    Email = admin.User?.Email ?? "",
                    Role = admin.Role.ToString(),
                    Status = admin.Status,
                    InvitedAt = admin.CreatedAt,
                    AcceptedAt = admin.Status == ProfileStatus.Active ? admin.UpdatedAt : null
                });
            }

            return invitations.OrderByDescending(i => i.InvitedAt);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting institution invitations for {InstitutionId}", institutionId);
            throw;
        }
    }

    public async Task CancelInvitationAsync(Guid profileId, string profileType)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            switch (profileType.ToLower())
            {
                case "student":
                    var student = await unitOfWork.Students.GetByIdAsync(profileId);
                    if (student == null)
                        throw new NotFoundException("Student not found");

                    if (student.Status != ProfileStatus.Pending)
                        throw new ValidationException("Can only cancel pending invitations");

                    await unitOfWork.Students.DeleteAsync(profileId);
                    break;

                case "teacher":
                    var teacher = await unitOfWork.Teachers.GetByIdAsync(profileId);
                    if (teacher == null)
                        throw new NotFoundException("Teacher not found");

                    if (teacher.Status != ProfileStatus.Pending)
                        throw new ValidationException("Can only cancel pending invitations");

                    await unitOfWork.Teachers.DeleteAsync(profileId);
                    break;

                case "admin":
                    var admin = await unitOfWork.Admins.GetByIdAsync(profileId);
                    if (admin == null)
                        throw new NotFoundException("Admin not found");

                    if (admin.Status != ProfileStatus.Pending)
                        throw new ValidationException("Can only cancel pending invitations");

                    await unitOfWork.Admins.DeleteAsync(profileId);
                    break;

                default:
                    throw new ValidationException("Invalid profile type");
            }

            await unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error canceling invitation for profile {ProfileId}", profileId);
            throw;
        }
    }

    // Private helper methods for accepting invitations
    private async Task<AcceptInvitationResponseDto> AcceptStudentInvitationAsync(ApplicationUser user, Guid studentId)
    {
        var student = await unitOfWork.Students.GetByIdAsync(studentId);
        if (student == null || student.UserId != user.Id)
            throw new NotFoundException("Student invitation not found");

        if (student.Status != ProfileStatus.Pending)
            throw new ValidationException("Invitation is not in pending status");

        // Now link institutions and add roles
        bool institutionsUpdated = false;

        if (student.SchoolId.HasValue)
        {
            var school = await unitOfWork.Schools.GetByIdAsync(student.SchoolId.Value);
            if (school?.Institution != null)
            {
                if (user.Institutions == null)
                    user.Institutions = new List<Institution> { school.Institution };
                else if (!user.Institutions.Any(i => i.Id == school.Institution.Id))
                    user.Institutions.Add(school.Institution);
                institutionsUpdated = true;
            }
        }

        if (student.UniversityId.HasValue)
        {
            var university = await unitOfWork.Universities.GetByIdAsync(student.UniversityId.Value);
            if (university?.Institution != null)
            {
                if (user.Institutions == null)
                    user.Institutions = new List<Institution> { university.Institution };
                else if (!user.Institutions.Any(i => i.Id == university.Institution.Id))
                    user.Institutions.Add(university.Institution);
                institutionsUpdated = true;
            }
        }

        if (institutionsUpdated)
        {
            user.IsLinked = true;
            await userManager.UpdateAsync(user);
        }

        // Add Student role
        if (!await userManager.IsInRoleAsync(user, "Student"))
            await userManager.AddToRoleAsync(user, "Student");

        // Update student status
        student.Status = ProfileStatus.Active;
        student.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.Students.UpdateAsync(student);
        await unitOfWork.CommitAsync();

        return new AcceptInvitationResponseDto
        {
            ProfileType = "Student",
            InstitutionName = student.School?.Institution?.Name ?? student.University?.Institution?.Name ?? "Unknown",
            Role = student.IsSchoolStudent ? "School Student" : "University Student",
            AcceptedAt = DateTime.UtcNow
        };
    }

    private async Task<AcceptInvitationResponseDto> AcceptTeacherInvitationAsync(ApplicationUser user, Guid teacherId)
    {
        var teacher = await unitOfWork.Teachers.GetByIdAsync(teacherId);
        if (teacher == null || teacher.UserId != user.Id)
            throw new NotFoundException("Teacher invitation not found");

        if (teacher.Status != ProfileStatus.Pending)
            throw new ValidationException("Invitation is not in pending status");

        // Link institution
        var institution = await unitOfWork.Institutions.GetByIdAsync(teacher.InstitutionId);
        if (institution != null)
        {
            if (user.Institutions == null)
                user.Institutions = new List<Institution> { institution };
            else if (!user.Institutions.Any(i => i.Id == institution.Id))
                user.Institutions.Add(institution);

            user.IsLinked = true;
            await userManager.UpdateAsync(user);
        }

        // Add Teacher role
        if (!await userManager.IsInRoleAsync(user, "Teacher"))
            await userManager.AddToRoleAsync(user, "Teacher");

        // Update teacher status
        teacher.Status = ProfileStatus.Active;
        teacher.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.Teachers.UpdateAsync(teacher);
        await unitOfWork.CommitAsync();

        return new AcceptInvitationResponseDto
        {
            ProfileType = "Teacher",
            InstitutionName = institution?.Name ?? "Unknown",
            Role = teacher.Title,
            AcceptedAt = DateTime.UtcNow
        };
    }

    private async Task<AcceptInvitationResponseDto> AcceptAdminInvitationAsync(ApplicationUser user, Guid adminId)
    {
        var admin = await unitOfWork.Admins.GetByIdAsync(adminId);
        if (admin == null || admin.UserId != user.Id)
            throw new NotFoundException("Admin invitation not found");

        if (admin.Status != ProfileStatus.Pending)
            throw new ValidationException("Invitation is not in pending status");

        // Link institution
        var institution = await unitOfWork.Institutions.GetByIdAsync(admin.InstitutionId);
        if (institution != null)
        {
            if (user.Institutions == null)
                user.Institutions = new List<Institution> { institution };
            else if (!user.Institutions.Any(i => i.Id == institution.Id))
                user.Institutions.Add(institution);

            user.IsLinked = true;
            await userManager.UpdateAsync(user);
        }

        // Add Admin role
        if (!await userManager.IsInRoleAsync(user, "Admin"))
            await userManager.AddToRoleAsync(user, "Admin");

        // Update admin status
        admin.Status = ProfileStatus.Active;
        admin.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.Admins.UpdateAsync(admin);
        await unitOfWork.CommitAsync();

        return new AcceptInvitationResponseDto
        {
            ProfileType = "Admin",
            InstitutionName = institution?.Name ?? "Unknown",
            Role = admin.Role.ToString(),
            AcceptedAt = DateTime.UtcNow
        };
    }
}