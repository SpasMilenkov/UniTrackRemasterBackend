using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Data.Models.Users;

public class AdminPermission : BaseEntity
{
    public Guid Id { get; set; }
    public required Guid AdminId { get; set; }
    public Admin Admin { get; set; }
    public required PermissionType Permission { get; set; }
    public string? Scope { get; set; }
}