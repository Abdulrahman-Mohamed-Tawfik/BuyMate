using BuyMate.DTO.Common;
using BuyMate.Infrastructure.Contracts;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly string _fileRoot;

        public FileService()
        {
            _fileRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(_fileRoot))
                Directory.CreateDirectory(_fileRoot);
        }

        public (bool IsValid, string? ErrorMessage) ValidateImage(IFormFile file, long maxSize, string[] allowedExtensions)
        {
            if (file.Length > maxSize)
                return (false, $"File size must be less than {maxSize / 1024 / 1024} MB.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                return (false, "Invalid file type. Allowed: jpg, jpeg, png, gif.");

            return (true, null);
        }

        public async Task<Response<string>> SaveImageAsync(IFormFile file, long maxSize, string[] allowedExtensions, string folder,string prefix="")
        {
            
            if (file == null || file.Length == 0)
                return new Response<string> { Status = false, Message = "No file provided." };

            var (isValid, errorMessage) = ValidateImage(file, maxSize, allowedExtensions);
            if (!isValid)
                return new Response<string> { Status = false, Message = errorMessage! };

            string uploadsRoot = Path.Combine(_fileRoot, folder);
            if (!Directory.Exists(uploadsRoot))
                Directory.CreateDirectory(uploadsRoot);

            string ext = Path.GetExtension(file.FileName);
            var fileName = $"{prefix}_{Guid.NewGuid()}{ext}";
            string filePath = Path.Combine(uploadsRoot, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            string path =  $"{folder}/{fileName}".Replace("\\", "/");

            return new Response<string> { Status = true, Message = "File uploaded successfully.", Data = path };
        }


        public async Task<Response<List<string>>> SaveImagesAsync(List<IFormFile> files, long maxSize, string[] allowedExtensions, string folder, string prefix = "")
        {
            var savedFiles = new List<string>();

            foreach (var file in files)
            {
                var result = await SaveImageAsync(file, maxSize, allowedExtensions, folder, prefix);

                if (!result.Status)
                {
                    // ROLLBACK: delete all previously uploaded files
                    foreach (var savedPath in savedFiles)
                    {
                        string fullPath = Path.Combine(_fileRoot, savedPath);
                        if (File.Exists(fullPath))
                            File.Delete(fullPath);
                    }

                    return new Response<List<string>>
                    {
                        Status = false,
                        Message = $"Failed to upload one or more files. Reason: {result.Message}"
                    };
                }

                savedFiles.Add(result.Data!);
            }

            return new Response<List<string>>
            {
                Status = true,
                Message = "All files uploaded successfully.",
                Data = savedFiles
            };
        }
        public async void DeleteImage(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            string fullPath = Path.Combine(_fileRoot, relativePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        
        

    }
}
