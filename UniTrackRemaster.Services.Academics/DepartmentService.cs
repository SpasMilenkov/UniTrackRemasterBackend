using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Department;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Services.Academics;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(
        IDepartmentRepository departmentRepository,
        IFacultyRepository facultyRepository,
        ILogger<DepartmentService> logger)
    {
        _departmentRepository = departmentRepository;
        _facultyRepository = facultyRepository;
        _logger = logger;
    }

    public async Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto)
    {
        var faculty = await _facultyRepository.GetByIdAsync(dto.FacultyId);
        if (faculty == null)
            throw new NotFoundException("Faculty not found");

        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            Location = dto.Location,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            Type = dto.Type,
            Status = dto.Status,
            FacultyId = dto.FacultyId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _departmentRepository.CreateAsync(department);
        return DepartmentResponseDto.FromEntity(department);
    }

    public async Task<DepartmentResponseDto> UpdateAsync(Guid id, UpdateDepartmentDto dto)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null)
            throw new NotFoundException("Department not found");

        if (dto.Name != null) department.Name = dto.Name;
        if (dto.Code != null) department.Code = dto.Code;
        if (dto.Description != null) department.Description = dto.Description;
        if (dto.Location != null) department.Location = dto.Location;
        if (dto.ContactEmail != null) department.ContactEmail = dto.ContactEmail;
        if (dto.ContactPhone != null) department.ContactPhone = dto.ContactPhone;
        if (dto.Type.HasValue) department.Type = dto.Type.Value;
        if (dto.Status.HasValue) department.Status = dto.Status.Value;

        department.UpdatedAt = DateTime.UtcNow;
        await _departmentRepository.UpdateAsync(department);
        return DepartmentResponseDto.FromEntity(department);
    }

    public async Task<DepartmentResponseDto> GetByIdAsync(Guid id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null)
            throw new NotFoundException("Department not found");
        return DepartmentResponseDto.FromEntity(department);
    }

    public async Task<IEnumerable<DepartmentResponseDto>> GetByFacultyAsync(Guid facultyId)
    {
        var departments = await _departmentRepository.GetByFacultyAsync(facultyId);
        return departments.Select(DepartmentResponseDto.FromEntity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null)
            throw new NotFoundException("Department not found");

        if (department.Teachers?.Any() == true)
            throw new ValidationException("Cannot delete department with assigned teachers");

        await _departmentRepository.DeleteAsync(id);
    }
}