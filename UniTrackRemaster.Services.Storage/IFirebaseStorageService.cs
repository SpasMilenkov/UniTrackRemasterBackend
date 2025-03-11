using Microsoft.AspNetCore.Http;

namespace UniTrackRemaster.Services.Storage;

public interface IFirebaseStorageService
{
    Task<string> UploadFileAsync(IFormFile file);
    Task<string> UploadFileAsync(Stream stream, string customPath);
    Task<List<string>> UploadFilesAsync(IEnumerable<IFormFile> files, string basePath);
    Task<string> CreateSignedUrl(string objectName);
    Task DeleteFileAsync(string objectPath);
}