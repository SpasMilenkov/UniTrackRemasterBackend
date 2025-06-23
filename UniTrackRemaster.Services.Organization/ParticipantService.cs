using System;
using UniTrackRemaster.Api.Dto.Event;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;

namespace UniTrackRemaster.Services.Organization;

public class ParticipantService : IParticipantService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventNotificationService _notificationService;

    public ParticipantService(IUnitOfWork unitOfWork, IEventNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<ParticipantResponseDto>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var participants = await _unitOfWork.Participants.GetByEventAsync(eventId, cancellationToken);
        return participants.Select(ParticipantResponseDto.FromEntity);
    }

    public async Task<IEnumerable<ParticipantResponseDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var participants = await _unitOfWork.Participants.GetByUserAsync(userId, cancellationToken);
        return participants.Select(ParticipantResponseDto.FromEntity);
    }

    public async Task<ParticipantResponseDto> AddParticipantAsync(CreateParticipantDto createDto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Check if user is already a participant
            var existingParticipant = await _unitOfWork.Participants.GetByEventAndUserAsync(createDto.EventId, createDto.UserId, cancellationToken);
            if (existingParticipant != null)
                throw new InvalidOperationException("User is already a participant in this event.");

            var participant = createDto.ToEntity();
            var createdParticipant = await _unitOfWork.Participants.CreateAsync(participant, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send invitation notification
            await _notificationService.SendEventInvitationAsync(createDto.EventId, createDto.UserId, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);
            return ParticipantResponseDto.FromEntity(createdParticipant);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ParticipantResponseDto> UpdateParticipantStatusAsync(Guid eventId, Guid userId, UpdateParticipantStatusDto updateDto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var participant = await _unitOfWork.Participants.GetByEventAndUserAsync(eventId, userId, cancellationToken);
            if (participant == null)
                throw new KeyNotFoundException("Participant not found.");

            updateDto.ApplyToEntity(participant);
            var updatedParticipant = await _unitOfWork.Participants.UpdateAsync(participant, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return ParticipantResponseDto.FromEntity(updatedParticipant);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task RemoveParticipantAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _unitOfWork.Participants.DeleteByEventAndUserAsync(eventId, userId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> IsUserParticipantAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Participants.IsUserParticipantAsync(eventId, userId, cancellationToken);
    }

    public async Task<Dictionary<ParticipantStatus, int>> GetParticipantStatsAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Participants.GetParticipantCountsByStatusAsync(eventId, cancellationToken);
    }
}