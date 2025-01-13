namespace UniTrackRemaster.Messaging;

public interface ITemplateService
{
    Task<string> GetTemplateAsync(string templateType);
}