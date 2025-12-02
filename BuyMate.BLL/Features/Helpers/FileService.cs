using BuyMate.BLL.Contracts.Helpers;
using BuyMate.DTO.Common;
using Microsoft.AspNetCore.Http;

namespace BuyMate.BLL.Features.Helpers
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

        public async Task<Response<string>> SaveImageAsync(IFormFile file, long maxSize, string[] allowedExtensions, string folder, string prefix = "")
        {

            if (file == null || file.Length == 0)
                return Response<string>.Fail("No file provided.");

            var (isValid, errorMessage) = ValidateImage(file, maxSize, allowedExtensions);
            if (!isValid)
                return Response<string>.Fail(errorMessage!);

            string uploadsRoot = Path.Combine(_fileRoot, folder);
            if (!Directory.Exists(uploadsRoot))
                Directory.CreateDirectory(uploadsRoot);

            string ext = Path.GetExtension(file.FileName);
            var fileName = $"{prefix}_{Guid.NewGuid()}{ext}";
            string filePath = Path.Combine(uploadsRoot, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            string path = $"{folder}/{fileName}".Replace("\\", "/");

            return Response<string>.Success(path, "File uploaded successfully.");
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

                    return Response<List<string>>.Fail($"Failed to upload one or more files. Reason: {result.Message}");
                }

                savedFiles.Add(result.Data!);
            }

            return Response<List<string>>.Success(savedFiles, "All files uploaded successfully.");
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
