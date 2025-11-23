using BuyMate.DTO.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.Infrastructure.Contracts
{
    public interface IFileService
    {

        (bool IsValid, string? ErrorMessage) ValidateImage(IFormFile file, long maxSize, string[] allowedExtensions);
        Task<Response<string>> SaveImageAsync(IFormFile file, long maxSize, string[] allowedExtensions, string folder, string prefix = "");
        Task<Response<List<string>>> SaveImagesAsync(List<IFormFile> files, long maxSize, string[] allowedExtensions, string folder, string prefix = "");
        void DeleteImage(string? relativePath);
    }
}
