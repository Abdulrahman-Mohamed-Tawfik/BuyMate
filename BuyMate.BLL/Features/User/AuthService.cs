using BuyMate.BLL.Contracts;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using BuyMate.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System;

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
                    return new Response<bool>
                    {
                        Data = false,
                        Status = false,
                        Message = "Email is already in use."
                    };
                }

                var phoneExists = _userManager.Users.Any(u => u.PhoneNumber == model.Phone);
                if (phoneExists)
                {
                    return new Response<bool>
                    {
                        Data = false,
                        Status = false,
                        Message = "Phone number is already registered."
                    };
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

                    return new Response<bool>
                    {
                        Data = false,
                        Status = false,
                        Message = string.IsNullOrWhiteSpace(errors) ? "User registration failed." : errors
                    };
                }

                return new Response<bool>
                {
                    Data = true,
                    Status = true,
                    Message = "User registered successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for {Email}", model.Email);
                return new Response<bool>
                {
                    Data = false,
                    Status = false,
                    Message = "An unexpected error occurred while creating the account."
                };
            }
        }

        public async Task<Response<bool>> LoginAsync(LoginViewModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user is null)
                {
                    return Failure("Invalid email or password.");
                }

                var passwordMatch = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!passwordMatch)
                {
                    return Failure("Invalid email or password.");
                }

                var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (!signInResult.Succeeded)
                {
                    _logger.LogInformation("Sign-in failed for user {UserId}. Result: {Result}", user.Id, signInResult);
                    return Failure("Login failed. Invalid email or password.");
                }

                // Ensure avatar claim exists and is up-to-date
                await UpdateAvatarClaimAsync(user);

                // Refresh sign-in to apply claims (if needed)
                await _signInManager.RefreshSignInAsync(user);

                return new Response<bool>
                {
                    Data = true,
                    Status = true,
                    Message = "Logged in successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for {Email}", model.Email);
                return Failure("An unexpected error occurred during login.");
            }

            static Response<bool> Failure(string message) => new Response<bool> { Data = false, Status = false, Message = message };
        }

        public async Task<Response<bool>> LogoutAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return new Response<bool>
                {
                    Status = true,
                    Message = "Logged out successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return new Response<bool>
                {
                    Status = false,
                    Message = "An error occurred while logging out.",
                    Data = false
                };
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
            var avatarUrl = user.ProfileImageUrl ?? "UserProfileImages/Default.jpg";
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
