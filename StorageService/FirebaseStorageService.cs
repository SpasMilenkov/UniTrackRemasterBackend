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
            .CreateScoped(Google.Apis.Storage.v1.StorageService.Scope.CloudPlatform);

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

    public async Task<string> CreateSignedUrl(string objectName)
    {
        var urlSigner = _storageClient.CreateUrlSigner();
        if(urlSigner is null) throw new InvalidOperationException("Url signer is null");
        // Generate a signed URL with read-only access
        var signedUrl = await urlSigner.SignAsync(
            _bucketName,
            objectName,
            duration: new TimeSpan(0, 15, 0), // Expires in 15 minutes
            HttpMethod.Get);
        
        return signedUrl;
    }
}