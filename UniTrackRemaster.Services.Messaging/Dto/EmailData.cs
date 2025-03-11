namespace UniTrackRemaster.Services.Messaging.Dto;

public class EmailData
{
    public string Subject { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Body { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
}