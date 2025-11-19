using BuyMate.BLL.Contracts;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BuyMate.BLL.Features.User
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UserManager<Model.Entities.User> _userManager;
        private readonly SignInManager<Model.Entities.User> _signInManager;
        private readonly ILogger<UserProfileService> _logger;

        // Configuration constants
        private const string ProfilesFolderName = "UserProfileImages";
        private const string DefaultImageFileName = "Default.jpg";
        private static readonly string[] AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        private const long MaxFileSizeBytes = 2 * 1024 * 1024; //2 MB

        public UserProfileService(UserManager<Model.Entities.User> userManager, SignInManager<Model.Entities.User> signInManager, ILogger<UserProfileService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<Response<ProfileViewModel>> GetProfileAsync(System.Security.Claims.ClaimsPrincipal userPrincipal)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null)
                return new Response<ProfileViewModel>
                {
                    Data = null,
                    Status = false,
                    Message = "User Not Found"
                };

            var profile = new ProfileViewModel
            {
                Id = user.Id.ToString(),
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber ?? string.Empty,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                Address = user.Address,
                Avatar = user.ProfileImageUrl,
                IsAdmin = await _userManager.IsInRoleAsync(user, "Admin")
            };

            return new Response<ProfileViewModel>
            {
                Data = profile,
                Status = true,
                Message = "Profile"
            };

        }

        public async Task<Response<bool>> UpdateProfileAsync(ProfileViewModel model, ClaimsPrincipal userPrincipal, IFormFile? avatarFile)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null)
            {
                return new Response<bool>
                {
                    Status = false,
                    Message = "User not found",
                    Data = false
                };
            }

            // Friendly duplicate checks
            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existingByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingByEmail is not null)
                {
                    return new Response<bool>
                    {
                        Data = false,
                        Status = false,
                        Message = "Email is already registered"
                    };
                }
                user.Email = model.Email;
                user.UserName = model.Email.Split('@')[0];
            }

            if (!string.Equals(user.PhoneNumber, model.Phone, StringComparison.OrdinalIgnoreCase))
            {
                var phoneExists = _userManager.Users.Any(u => u.PhoneNumber == model.Phone && u.Id != user.Id);
                if (phoneExists)
                {
                    return new Response<bool>
                    {
                        Data = false,
                        Status = false,
                        Message = "Phone Number is already registered."
                    };
                }
            }


            // Update profile fields
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.Phone;
            user.BirthDate = model.BirthDate;
            user.Gender = model.Gender;
            user.Address = model.Address;


            // First use previous avatar and check if it exists
            if (!string.IsNullOrEmpty(model.Avatar))
            {
                user.ProfileImageUrl = model.Avatar;
            }

            // Handle avatar file upload if provided
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var validation = ValidateAvatarFile(avatarFile);
                if (!validation.IsValid)
                {
                    return new Response<bool>
                    {
                        Data = false,
                        Status = false,
                        Message = validation.ErrorMessage
                    };
                }

                try
                {
                    var uploadsRoot = GetUploadsRoot();
                    if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);

                    var ext = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
                    var safeUser = string.IsNullOrWhiteSpace(user.UserName) ? user.Id.ToString() : user.UserName;
                    var fileName = $"{safeUser}_{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsRoot, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }

                    // Prepare relative path without leading slash to match layout expectation
                    var relativePath = Path.Combine(ProfilesFolderName, fileName).Replace("\\", "/");

                    // Delete previous custom image if it was not the default
                    var currentProfileImage = user.ProfileImageUrl;
                    if (!IsDefaultImage(currentProfileImage))
                    {
                        try
                        {
                            DeleteOldFileIfExists(currentProfileImage, uploadsRoot);
                        }
                        catch (Exception ex)
                        {
                            // Log and continue; failure to delete old file should not block profile update
                            _logger.LogWarning(ex, "Failed to delete old profile image for user {UserId}", user.Id);
                        }
                    }

                    user.ProfileImageUrl = relativePath;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving avatar for user {UserId}", user.Id);
                    return new Response<bool>
                    {
                        Data = false,
                        Status = false,
                        Message = "Failed to save avatar."
                    };
                }
            }
           
              


            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                string errs = string.Join("; ", result.Errors.Select(e => e.Description));
                return new Response<bool>
                {
                    Status = false,
                    Message = errs,
                    Data = false
                };
            }

            // Update avatar claim
            var oldClaim = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "avatar");
            if (oldClaim != null)
                await _userManager.RemoveClaimAsync(user, oldClaim);

            var avatarUrl = user.ProfileImageUrl ?? string.Empty;
            await _userManager.AddClaimAsync(user, new Claim("avatar", avatarUrl));

            // Refresh sign-in so new claims are applied to the authenticated principal
            try
            {
                await _signInManager.RefreshSignInAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refresh sign-in for user {UserId}", user.Id);
            }

            return new Response<bool>
            {
                Status = true,
                Message = "Profile updated successfully",
                Data = true
            };
        }

        private (bool IsValid, string? ErrorMessage) ValidateAvatarFile(IFormFile file)
        {
            if (file.Length > MaxFileSizeBytes)
            {
                return (false, $"Avatar file size must be less than {MaxFileSizeBytes / 1024 / 1024} MB.");
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                return (false, "Invalid file type. Allowed types: jpg, jpeg, png, gif.");
            }

            return (true, null);
        }

        private string GetUploadsRoot()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", ProfilesFolderName);
        }

        private bool IsDefaultImage(string? profileImageUrl)
        {
            if (string.IsNullOrWhiteSpace(profileImageUrl)) return true;
            var normalized = profileImageUrl.Replace("\\", "/");
            // Support both "UserProfileImages/Default.jpg" and "/UserProfileImages/Default.jpg"
            return normalized.EndsWith($"/{DefaultImageFileName}", StringComparison.OrdinalIgnoreCase)
                   || normalized.Equals(DefaultImageFileName, StringComparison.OrdinalIgnoreCase)
                   || normalized.EndsWith($"{DefaultImageFileName}", StringComparison.OrdinalIgnoreCase) && normalized.Contains(ProfilesFolderName, StringComparison.OrdinalIgnoreCase);
        }

        private void DeleteOldFileIfExists(string? profileImageUrl, string uploadsRoot)
        {
            if (string.IsNullOrWhiteSpace(profileImageUrl)) return;
            var normalized = profileImageUrl.Replace("\\", "/").TrimStart('/');
            // expected normalized like "UserProfileImages/filename.jpg" or "UserProfileImages/Default.jpg"
            var fileName = Path.GetFileName(normalized);
            if (string.IsNullOrWhiteSpace(fileName)) return;

            if (string.Equals(fileName, DefaultImageFileName, StringComparison.OrdinalIgnoreCase))
            {
                // Never delete the default image
                return;
            }

            var fullPath = Path.Combine(uploadsRoot, fileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
