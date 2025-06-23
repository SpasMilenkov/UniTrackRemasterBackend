using System;
using UniTrackRemaster.Api.Dto.Event;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Services.Organization;

public class OrganizerService : IOrganizerService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrganizerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrganizerResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var organizer = await _unitOfWork.Organizers.GetByIdAsync(id, cancellationToken);
        if (organizer == null)
            throw new KeyNotFoundException($"Organizer with ID {id} not found.");

        return OrganizerResponseDto.FromEntity(organizer);
    }

    public async Task<OrganizerResponseDto> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var organizer = await _unitOfWork.Organizers.GetByUserIdAsync(userId, cancellationToken);
        if (organizer == null)
            throw new KeyNotFoundException($"Organizer for user {userId} not found.");

        return OrganizerResponseDto.FromEntity(organizer);
    }

    public async Task<IEnumerable<OrganizerResponseDto>> GetByInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default)
    {
        var organizers = await _unitOfWork.Organizers.GetByInstitutionAsync(institutionId, cancellationToken);
        return organizers.Select(OrganizerResponseDto.FromEntity);
    }

    public async Task<OrganizerResponseDto> CreateAsync(CreateOrganizerDto createDto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Check if user is already an organizer
            var existingOrganizer = await _unitOfWork.Organizers.GetByUserIdAsync(createDto.UserId, cancellationToken);
            if (existingOrganizer != null)
                throw new InvalidOperationException("User is already an organizer.");

            var organizer = createDto.ToEntity();
            var createdOrganizer = await _unitOfWork.Organizers.CreateAsync(organizer, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return OrganizerResponseDto.FromEntity(createdOrganizer);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<OrganizerResponseDto> UpdateAsync(Guid id, CreateOrganizerDto updateDto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var existingOrganizer = await _unitOfWork.Organizers.GetByIdAsync(id, cancellationToken);
            if (existingOrganizer == null)
                throw new KeyNotFoundException($"Organizer with ID {id} not found.");

            // Update properties
            existingOrganizer.Department = updateDto.Department;
            existingOrganizer.Role = updateDto.Role;
            existingOrganizer.CanCreatePublicEvents = updateDto.CanCreatePublicEvents;
            existingOrganizer.InstitutionId = updateDto.InstitutionId;
            existingOrganizer.UpdatedAt = DateTime.UtcNow;

            var updatedOrganizer = await _unitOfWork.Organizers.UpdateAsync(existingOrganizer, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return OrganizerResponseDto.FromEntity(updatedOrganizer);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var existingOrganizer = await _unitOfWork.Organizers.GetByIdAsync(id, cancellationToken);
            if (existingOrganizer == null)
                throw new KeyNotFoundException($"Organizer with ID {id} not found.");

            await _unitOfWork.Organizers.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> IsUserOrganizerAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Organizers.IsUserOrganizerAsync(userId, cancellationToken);
    }

    public async Task<bool> CanCreatePublicEventsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Organizers.CanCreatePublicEventsAsync(userId, cancellationToken);
    }
}