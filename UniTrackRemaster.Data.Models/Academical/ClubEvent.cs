namespace UniTrackRemaster.Data.Models.Academical;

public class ClubEvent : BaseEntity
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; }
    
    public Guid ClubId { get; set; }
    public Club Club { get; set; }
}
