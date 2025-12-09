using BuyMate.BLL.Contracts;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BuyMate.BLL.Features.User
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<Model.Entities.User> _userManager;
        private readonly SignInManager<Model.Entities.User> _signInManager;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<Model.Entities.User> userManager, SignInManager<Model.Entities.User> signInManager, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<Response<bool>> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                // Friendly duplicate checks
                var existingByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingByEmail is not null)
                {
                    return Response<bool>.Fail("Email is already registered.");
                }

                var phoneExists = _userManager.Users.Any(u => u.PhoneNumber == model.Phone);
                if (phoneExists)
                {
                    return Response<bool>.Fail("Phone number is already registered.");
                }

                var user = new Model.Entities.User
                {
                    UserName = BuildUserNameFromEmail(model.Email),
                    Email = model.Email,
                    PhoneNumber = model.Phone,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("User registration failed for {Email}: {Errors}", model.Email, errors);

                    return Response<bool>.Fail("Registration failed: " + errors);
                }

                // Add default 'user' role
                var roleResult = await _userManager.AddToRoleAsync(user, "user");
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Adding role 'user' failed for {Email}: {Errors}", model.Email, errors);
                    // Continue but inform caller
                    return Response<bool>.Fail("Account created but failed to assign default role: " + errors);
                }

                return Response<bool>.Success(true, "Account created successfully. Please log in.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for {Email}", model.Email);
                return Response<bool>.Fail("An unexpected error occurred during registration.");
            }
        }

        public async Task<Response<bool>> LoginAsync(LoginViewModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                
                var valid = user != null && await _userManager.CheckPasswordAsync(user, model.Password);

                if (user is null || !valid)
                {
                    return Response<bool>.Fail("Invalid email or password.");
                }

                var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (!signInResult.Succeeded)
                {
                    _logger.LogInformation("Sign-in failed for user {UserId}. Result: {Result}", user.Id, signInResult);
                    return Response<bool>.Fail("Login failed. Please try again.");
                }

                // Ensure avatar claim exists and is up-to-date
                await UpdateAvatarClaimAsync(user);

                // Refresh sign-in to apply claims (if needed)
                await _signInManager.RefreshSignInAsync(user);

                return Response<bool>.Success(true, "Login successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for {Email}", model.Email);
                return Response<bool>.Fail("An unexpected error occurred during login.");
            }

        }

        public async Task<Response<bool>> LogoutAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Response<bool>.Success(true, "Logout successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return Response<bool>.Fail("An unexpected error occurred during logout.");
            }
        }

        private async Task UpdateAvatarClaimAsync(Model.Entities.User user)
        {
            // Remove existing avatar claim if present
            var existingClaim = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "avatar");
            if (existingClaim != null)
            {
                await _userManager.RemoveClaimAsync(user, existingClaim);
            }

            // Add current profile image url (may be empty)
            var avatarUrl = user.ProfileImageUrl ?? "UserProfileImages/Default.webp";
            await _userManager.AddClaimAsync(user, new Claim("avatar", avatarUrl));
        }

        private static string BuildUserNameFromEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return Guid.NewGuid().ToString();
            var parts = email.Split('@');
            return parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]) ? parts[0] : Guid.NewGuid().ToString();
        }
    }
}
