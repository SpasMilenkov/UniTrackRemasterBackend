using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;

namespace StorageService;

public class FirebaseStorageService: IFirebaseStorageService
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;

    public FirebaseStorageService(string credentialsPath, string bucketName)
    {
        
        var credential = Google.Apis.Auth.OAuth2.GoogleCredential
            .FromFile(credentialsPath)
            .CreateScoped(Google.Apis.Storage.v1.StorageService.Scope.DevstorageFullControl);

        _storageClient = StorageClient.Create(credential);
        _bucketName = bucketName;
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var dataObject = await _storageClient.UploadObjectAsync(
            _bucketName,
            $"uploads/{uniqueFileName}",
            file.ContentType,
            memoryStream);

        return dataObject.Name;
    }
    
    public async Task<string> UploadFileAsync(Stream stream, string customPath)
    {
        var dataObject = await _storageClient.UploadObjectAsync(
            _bucketName,
            customPath,
            "application/octet-stream", // Default content type, you might want to make this parameterizable
            stream);

        return dataObject.Name;
    }

    public async Task<string> CreateSignedUrl(string objectName)
    {
        var urlSigner = _storageClient.CreateUrlSigner();
        if (urlSigner is null) throw new InvalidOperationException("Url signer is null");
        // Generate a signed URL with read-only access
        var signedUrl = await urlSigner.SignAsync(
            _bucketName,
            objectName,
            duration: new TimeSpan(0, 15, 0), // Expires in 15 minutes
            HttpMethod.Get);

        return signedUrl;
    }

    public async Task DeleteFileAsync(string objectPath)
    {
        try
        {
            await _storageClient.DeleteObjectAsync(_bucketName, objectPath);
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Object doesn't exist - you might want to log this or handle it differently
            throw new FileNotFoundException($"File not found at path: {objectPath}", ex);
        }
        catch (Exception ex)
        {
            // Handle or rethrow other exceptions
            throw new Exception($"Failed to delete file at path: {objectPath}", ex);
        }
    }
    public async Task<List<string>> UploadFilesAsync(IEnumerable<IFormFile> files, string basePath)
    {
        var uploadTasks = new List<Task<string>>();
        
        foreach (var file in files)
        {
            if (file.Length <= 0) continue;
            
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = $"{basePath}/{uniqueFileName}";
            
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var uploadTask = _storageClient.UploadObjectAsync(
                _bucketName,
                fullPath,
                file.ContentType,
                memoryStream);

            uploadTasks.Add(uploadTask.ContinueWith(t => t.Result.Name));
        }

        var results = await Task.WhenAll(uploadTasks);
        return results.ToList();
    }
}