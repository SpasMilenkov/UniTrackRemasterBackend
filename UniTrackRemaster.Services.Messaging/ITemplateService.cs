namespace UniTrackRemaster.Services.Messaging;

public interface ITemplateService
{
    Task<string> GetTemplateAsync(string templateType);
}