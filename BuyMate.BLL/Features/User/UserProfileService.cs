using BuyMate.BLL.Constants;
using BuyMate.BLL.Contracts;
using BuyMate.BLL.Contracts.Helpers;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BuyMate.BLL.Features.User
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UserManager<Model.Entities.User> _userManager;
        private readonly SignInManager<Model.Entities.User> _signInManager;
        private readonly ILogger<UserProfileService> _logger;
        private readonly IFileService _fileService;

        public UserProfileService(UserManager<Model.Entities.User> userManager, SignInManager<Model.Entities.User> signInManager, ILogger<UserProfileService> logger, IFileService fileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<Response<ProfileViewModel>> GetProfileAsync(System.Security.Claims.ClaimsPrincipal userPrincipal)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null)
                return Response<ProfileViewModel>.Fail("User Not Found");

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

            return Response<ProfileViewModel>.Success(profile, "Profile Data retrieved successfully");
        }

        public async Task<Response<bool>> UpdateProfileAsync(ProfileViewModel model, ClaimsPrincipal userPrincipal, IFormFile? avatarFile)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null)
                return Response<bool>.Fail("User not found.");

            // Friendly duplicate checks
            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existingByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingByEmail is not null)
                    return Response<bool>.Fail("Email is already registered.");

                user.Email = model.Email;
                user.UserName = model.Email.Split('@')[0];
            }

            if (!string.Equals(user.PhoneNumber, model.Phone, StringComparison.OrdinalIgnoreCase))
            {
                var phoneExists = _userManager.Users.Any(u => u.PhoneNumber == model.Phone && u.Id != user.Id);
                if (phoneExists)
                    return Response<bool>.Fail("Phone Number is already registered.");
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
                try
                {
                    var safeUser = string.IsNullOrWhiteSpace(user.UserName) ? user.Id.ToString() : user.UserName;

                    var response = await _fileService.SaveImageAsync(avatarFile, AppConstants.MaxImageFileSizeBytes, AppConstants.AllowedImageExtensions, AppConstants.UserProfileImagesFolder, safeUser);

                    if (!response.Status)
                        return Response<bool>.Fail(response.Message);

                    // Save the image using the file service
                    var relativePath = response.Data;

                    // Delete previous custom image if it was not the default
                    var currentProfileImage = user.ProfileImageUrl;
                    if (!IsDefaultImage(currentProfileImage))
                    {
                        try
                        {
                            _fileService.DeleteImage(currentProfileImage);
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
                    return Response<bool>.Fail("Failed to save avatar.");
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                string errs = string.Join("; ", result.Errors.Select(e => e.Description));
                return Response<bool>.Fail(errs);
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

            return Response<bool>.Success(true, "Profile updated successfully");
        }

        private bool IsDefaultImage(string? profileImageUrl)
        {
            if (string.IsNullOrWhiteSpace(profileImageUrl)) return true;
            var normalized = profileImageUrl.Replace("\\", "/");
            // Check if the URL ends with the default image filename or is just the filename
            return normalized.EndsWith($"/{AppConstants.DefaultProfileImageFileName}", StringComparison.OrdinalIgnoreCase)
                   || normalized.Equals(AppConstants.DefaultProfileImageFileName, StringComparison.OrdinalIgnoreCase)
                   || normalized.EndsWith($"{AppConstants.DefaultProfileImageFileName}", StringComparison.OrdinalIgnoreCase) && normalized.Contains(AppConstants.UserProfileImagesFolder, StringComparison.OrdinalIgnoreCase);
        }

    }
}
