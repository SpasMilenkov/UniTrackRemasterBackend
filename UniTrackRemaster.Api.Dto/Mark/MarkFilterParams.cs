using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Mark;

/// <summary>
/// MarkFilterParams with semester support
/// </summary>
public class MarkFilterParams
{
    public Guid? StudentId { get; set; }
    public Guid? TeacherId { get; set; }
    public Guid? SubjectId { get; set; }
    public Guid? SemesterId { get; set; }
    public MarkType? Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
