using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StorageService;

namespace UniTrackRemaster.Messaging;

public class TemplateService(IConfiguration configuration, ILogger<TemplateService> logger, FirebaseStorageService? firebaseStorage = null)
    : ITemplateService
{
    private readonly ConcurrentDictionary<string, string> _templateCache = new();
    private readonly string _templateSource = configuration["TemplateSettings:Source"] ?? "Embedded";
    private readonly string _templatesPath = configuration["TemplateSettings:LocalPath"] ?? "Templates";

    public async Task<string> GetTemplateAsync(string templateType)
    {
        // Remove "Template" suffix if it exists to normalize the template name
        templateType = templateType.Replace("Template", "");
        
        // Try to get from cache first
        if (_templateCache.TryGetValue(templateType, out string? cachedTemplate))
        {
            return cachedTemplate;
        }

        var template = _templateSource.ToLower() switch
        {
            "embedded" => await GetEmbeddedTemplateAsync(templateType),
            "local" => await GetLocalTemplateAsync(templateType),
            "firebase" => await GetFirebaseTemplateAsync(templateType),
            _ => throw new ArgumentException($"Unsupported template source: {_templateSource}")
        };

        // Cache the template
        _templateCache.TryAdd(templateType, template);
        
        return template;
    }

    private async Task<string> GetEmbeddedTemplateAsync(string templateType)
    {
        var assembly = typeof(TemplateService).Assembly;
        var assemblyName = assembly.GetName().Name;
        
        var possibleResourceNames = new[]
        {
            $"{assemblyName}.Templates.{templateType}.html",
            $"{assemblyName}.Templates.{templateType}Template.html",
            $"Templates.{templateType}.html",
            $"Templates.{templateType}Template.html"
        };

        foreach (var resourceName in possibleResourceNames)
        {
            logger.LogInformation("Trying resource name: {ResourceName}", resourceName);
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
        }

        throw new FileNotFoundException($"Template not found: {templateType}. Tried resources: {string.Join(", ", possibleResourceNames)}");
    }

    private async Task<string> GetLocalTemplateAsync(string templateType)
    {
        var possiblePaths = new[]
        {
            Path.Combine(_templatesPath, $"{templateType}.html"),
            Path.Combine(_templatesPath, $"{templateType}Template.html")
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return await File.ReadAllTextAsync(path);
            }
        }
            
        throw new FileNotFoundException($"Template not found: {templateType}. Tried paths: {string.Join(", ", possiblePaths)}");
    }

    private async Task<string> GetFirebaseTemplateAsync(string templateType)
    {
        if (firebaseStorage == null)
            throw new InvalidOperationException("Firebase storage service not configured");

        var objectName = $"templates/{templateType}Template.html";
        var signedUrl = await firebaseStorage.CreateSignedUrl(objectName);
        
        using var client = new HttpClient();
        return await client.GetStringAsync(signedUrl);
    }
}