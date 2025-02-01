using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Data.Models.Organizations;

public class Application : BaseEntity
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string Code { get; set; }
    public ApplicationStatus Status { get; set; }
    public Guid InstitutionId { get; set; }
    public Institution? Institution { get; set; }
}