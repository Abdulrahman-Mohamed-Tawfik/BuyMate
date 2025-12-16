using BuyMate.DTO.Common;
using Microsoft.AspNetCore.Http;

namespace BuyMate.BLL.Contracts.Helpers
{
    public interface IFileService
    {
        (bool IsValid, string? ErrorMessage) ValidateImage(IFormFile file, long maxSize, string[] allowedExtensions);
        Task<Response<string>> SaveImageAsync(IFormFile file, long maxSize, string[] allowedExtensions, string folder, string prefix = "");
        Task<Response<List<string>>> SaveImagesAsync(List<IFormFile> files, long maxSize, string[] allowedExtensions, string folder, string prefix = "");
        void DeleteImage(string? relativePath);
    }
}
