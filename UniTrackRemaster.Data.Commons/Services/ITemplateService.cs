namespace UniTrackRemaster.Commons.Services;

public interface ITemplateService
{
    Task<string> GetTemplateAsync(string templateType);
}