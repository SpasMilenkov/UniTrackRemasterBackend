using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.Academics;

public class TeacherService : ITeacherService
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TeacherService> _logger;

    public TeacherService(
        ITeacherRepository teacherRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<TeacherService> logger)
    {
        _teacherRepository = teacherRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<TeacherResponseDto> GetByIdAsync(Guid id)
    {
        var teacher = await _teacherRepository.GetByIdAsync(id);
        if (teacher == null) throw new NotFoundException("Teacher not found");
        return TeacherResponseDto.FromEntity(teacher, teacher.User);
    }

    public async Task<IEnumerable<TeacherResponseDto>> GetAllAsync()
    {
        var teachers = await _teacherRepository.GetAllAsync();
        return teachers.Select(t => TeacherResponseDto.FromEntity(t, t.User));
    }

    public async Task<TeacherResponseDto> CreateAsync(CreateTeacherDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            AvatarUrl = null
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.First().Description);
        }

        await _userManager.AddToRoleAsync(user, "Teacher");

        var teacher = CreateTeacherDto.ToEntity(dto, user.Id);
        await _teacherRepository.CreateAsync(teacher);

        return TeacherResponseDto.FromEntity(teacher, user);
    }

    public async Task<TeacherResponseDto> UpdateAsync(Guid id, UpdateTeacherDto dto)
    {
        var teacher = await _teacherRepository.GetByIdAsync(id);
        if (teacher == null) throw new NotFoundException("Teacher not found");

        if (dto.Title != null) teacher.Title = dto.Title;
        if (dto.ClassGradeId.HasValue) teacher.ClassGradeId = dto.ClassGradeId;

        await _teacherRepository.UpdateAsync(teacher);
        return TeacherResponseDto.FromEntity(teacher, teacher.User);
    }

    public async Task DeleteAsync(Guid id)
    {
        var teacher = await _teacherRepository.GetByIdAsync(id);
        if (teacher == null) throw new NotFoundException("Teacher not found");

        await _userManager.DeleteAsync(teacher.User);
        await _teacherRepository.DeleteAsync(id);
    }
}