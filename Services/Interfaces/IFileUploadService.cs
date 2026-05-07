using Microsoft.AspNetCore.Http;

namespace HR_Management_System.Services.Interfaces
{
    public interface IFileUploadService
    {
        Task<string?> UploadFileAsync(IFormFile file, string subDirectory);
        Task<bool> DeleteFileAsync(string filePath);
        string GetFileUrl(string filePath);
    }
}