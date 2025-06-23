using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Event;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Services.Organization;

namespace UniTrackRemaster.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IParticipantService _participantService;
        private readonly IOrganizerService _organizerService;
        private readonly IEventNotificationService _notificationService;

        public EventsController(
            IEventService eventService,
            IParticipantService participantService,
            IOrganizerService organizerService,
            IEventNotificationService notificationService)
        {
            _eventService = eventService;
            _participantService = participantService;
            _organizerService = organizerService;
            _notificationService = notificationService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        #region Event Endpoints

        /// <summary>
        /// Get all events with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetEvents([FromQuery] EventFilterDto? filter = null)
        {
            try
            {
                IEnumerable<EventResponseDto> events;

                if (filter != null && (filter.StartDate.HasValue || filter.EndDate.HasValue ||
                    filter.Type.HasValue || filter.Status.HasValue || !string.IsNullOrEmpty(filter.SearchTerm)))
                {
                    events = await _eventService.GetFilteredAsync(filter);
                }
                else
                {
                    events = await _eventService.GetAllAsync();
                }

                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving events.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get event by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EventResponseDto>> GetEvent(Guid id)
        {
            try
            {
                var eventDto = await _eventService.GetByIdAsync(id);
                return Ok(eventDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the event.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get detailed event information by ID
        /// </summary>
        [HttpGet("{id:guid}/details")]
        public async Task<ActionResult<EventDetailResponseDto>> GetEventDetails(Guid id)
        {
            try
            {
                var eventDetails = await _eventService.GetDetailByIdAsync(id);
                return Ok(eventDetails);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving event details.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get upcoming events for current user
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetUpcomingEvents()
        {
            try
            {
                var userId = GetCurrentUserId();
                var events = await _eventService.GetUpcomingEventsAsync(userId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving upcoming events.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get events by organizer
        /// </summary>
        [HttpGet("organizer/{organizerId:guid}")]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetEventsByOrganizer(Guid organizerId)
        {
            try
            {
                var events = await _eventService.GetByOrganizerAsync(organizerId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving organizer events.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get events by institution
        /// </summary>
        [HttpGet("institution/{institutionId:guid}")]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetEventsByInstitution(Guid institutionId)
        {
            try
            {
                var events = await _eventService.GetByInstitutionAsync(institutionId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving institution events.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get events where current user is a participant
        /// </summary>
        [HttpGet("my-participations")]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetMyParticipations()
        {
            try
            {
                var userId = GetCurrentUserId();
                var events = await _eventService.GetByParticipantAsync(userId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving participations.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new event
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<EventResponseDto>> CreateEvent([FromBody] CreateEventDto createEventDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var eventDto = await _eventService.CreateAsync(createEventDto, userId);
                return CreatedAtAction(nameof(GetEvent), new { id = eventDto.Id }, eventDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the event.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing event
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<EventResponseDto>> UpdateEvent(Guid id, [FromBody] UpdateEventDto updateEventDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var canModify = await _eventService.CanUserModifyEventAsync(id, userId);

                if (!canModify)
                {
                    return Forbid("You are not authorized to modify this event.");
                }

                var eventDto = await _eventService.UpdateAsync(id, updateEventDto);
                return Ok(eventDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the event.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete an event
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var canModify = await _eventService.CanUserModifyEventAsync(id, userId);

                if (!canModify)
                {
                    return Forbid("You are not authorized to delete this event.");
                }

                await _eventService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the event.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get event statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<EventStatsDto>> GetEventStats()
        {
            try
            {
                var stats = await _eventService.GetStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving event statistics.", error = ex.Message });
            }
        }

        #endregion

        #region Participant Endpoints

        /// <summary>
        /// Get participants for an event
        /// </summary>
        [HttpGet("{eventId:guid}/participants")]
        public async Task<ActionResult<IEnumerable<ParticipantResponseDto>>> GetEventParticipants(Guid eventId)
        {
            try
            {
                var participants = await _participantService.GetByEventAsync(eventId);
                return Ok(participants);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving participants.", error = ex.Message });
            }
        }

        /// <summary>
        /// Add a participant to an event
        /// </summary>
        [HttpPost("{eventId:guid}/participants")]
        public async Task<ActionResult<ParticipantResponseDto>> AddParticipant(Guid eventId, [FromBody] Guid userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var canModify = await _eventService.CanUserModifyEventAsync(eventId, currentUserId);

                if (!canModify)
                {
                    return Forbid("You are not authorized to add participants to this event.");
                }

                var createDto = new CreateParticipantDto(eventId, userId, false);

                var participant = await _participantService.AddParticipantAsync(createDto);
                return CreatedAtAction(nameof(GetEventParticipants), new { eventId }, participant);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the participant.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update participant status (RSVP)
        /// </summary>
        [HttpPut("{eventId:guid}/participants/{userId:guid}/status")]
        public async Task<ActionResult<ParticipantResponseDto>> UpdateParticipantStatus(
            Guid eventId,
            Guid userId,
            [FromBody] UpdateParticipantStatusDto updateDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Users can only update their own status, or organizers can update anyone's
                if (currentUserId != userId)
                {
                    var canModify = await _eventService.CanUserModifyEventAsync(eventId, currentUserId);
                    if (!canModify)
                    {
                        return Forbid("You are not authorized to update this participant's status.");
                    }
                }

                var participant = await _participantService.UpdateParticipantStatusAsync(eventId, userId, updateDto);
                return Ok(participant);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating participant status.", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove a participant from an event
        /// </summary>
        [HttpDelete("{eventId:guid}/participants/{userId:guid}")]
        public async Task<IActionResult> RemoveParticipant(Guid eventId, Guid userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Users can remove themselves, or organizers can remove anyone
                if (currentUserId != userId)
                {
                    var canModify = await _eventService.CanUserModifyEventAsync(eventId, currentUserId);
                    if (!canModify)
                    {
                        return Forbid("You are not authorized to remove this participant.");
                    }
                }

                await _participantService.RemoveParticipantAsync(eventId, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing the participant.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get participant statistics for an event
        /// </summary>
        [HttpGet("{eventId:guid}/participants/stats")]
        public async Task<ActionResult<Dictionary<string, int>>> GetParticipantStats(Guid eventId)
        {
            try
            {
                var stats = await _participantService.GetParticipantStatsAsync(eventId);
                var formattedStats = stats.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
                return Ok(formattedStats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving participant statistics.", error = ex.Message });
            }
        }

        #endregion

        #region Organizer Endpoints

        /// <summary>
        /// Get organizer by user ID
        /// </summary>
        [HttpGet("organizers/me")]
        public async Task<ActionResult<OrganizerResponseDto>> GetMyOrganizerProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var organizer = await _organizerService.GetByUserIdAsync(userId);
                return Ok(organizer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving organizer profile.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create organizer profile
        /// </summary>
        [HttpPost("organizers")]
        public async Task<ActionResult<OrganizerResponseDto>> CreateOrganizer([FromBody] CreateOrganizerDto createDto)
        {
            try
            {
                var organizer = await _organizerService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetMyOrganizerProfile), organizer);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating organizer profile.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get organizers by institution
        /// </summary>
        [HttpGet("organizers/institution/{institutionId:guid}")]
        public async Task<ActionResult<IEnumerable<OrganizerResponseDto>>> GetOrganizersByInstitution(Guid institutionId)
        {
            try
            {
                var organizers = await _organizerService.GetByInstitutionAsync(institutionId);
                return Ok(organizers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving organizers.", error = ex.Message });
            }
        }

        #endregion

        #region Notification Endpoints

        /// <summary>
        /// Get notifications for current user
        /// </summary>
        [HttpGet("notifications/me")]
        public async Task<ActionResult<IEnumerable<EventNotificationResponseDto>>> GetMyNotifications()
        {
            try
            {
                var userId = GetCurrentUserId();
                var notifications = await _notificationService.GetByUserAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving notifications.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get notifications for an event
        /// </summary>
        [HttpGet("{eventId:guid}/notifications")]
        public async Task<ActionResult<IEnumerable<EventNotificationResponseDto>>> GetEventNotifications(Guid eventId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var canModify = await _eventService.CanUserModifyEventAsync(eventId, userId);

                if (!canModify)
                {
                    return Forbid("You are not authorized to view notifications for this event.");
                }

                var notifications = await _notificationService.GetByEventAsync(eventId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving event notifications.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a custom notification
        /// </summary>
        [HttpPost("notifications")]
        public async Task<ActionResult<EventNotificationResponseDto>> CreateNotification([FromBody] CreateEventNotificationDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var canModify = await _eventService.CanUserModifyEventAsync(createDto.EventId, userId);

                if (!canModify)
                {
                    return Forbid("You are not authorized to create notifications for this event.");
                }

                var notification = await _notificationService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetEventNotifications), new { eventId = createDto.EventId }, notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the notification.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        [HttpDelete("notifications/{id:guid}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            try
            {
                await _notificationService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the notification.", error = ex.Message });
            }
        }

        #endregion
    }
}
