using Microsoft.AspNetCore.Http;

namespace StorageService;

public interface IFirebaseStorageService
{
    Task<string> UploadFileAsync(IFormFile file);
}