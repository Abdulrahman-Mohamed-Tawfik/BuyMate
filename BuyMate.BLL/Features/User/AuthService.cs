using BuyMate.BLL.Contracts;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace BuyMate.BLL.Features.User
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<Model.Entities.User> _userManager;
        private readonly SignInManager<Model.Entities.User> _signInManager;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<Model.Entities.User> userManager, SignInManager<Model.Entities.User> signInManager, ILogger<AuthService> logger, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _configuration = configuration;
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

        // MVC login: cookie sign-in + claims refresh
        public async Task<Response<bool>> LoginMvcAsync(LoginViewModel model)
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
                    _logger.LogInformation($"Sign-in failed for user {user.Id}. Result: {signInResult}", user.Id, signInResult);
                    return Response<bool>.Fail("Login failed. Please try again.");
                }

                await UpdateAvatarClaimAsync(user);
                await _signInManager.RefreshSignInAsync(user);
                return Response<bool>.Success(true, "Login successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for {Email}", model.Email);
                return Response<bool>.Fail("An unexpected error occurred during login.");
            }
        }

        // API login: validate, issue JWT, persist token
        public async Task<Response<string>> LoginApiAsync(LoginViewModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var valid = user != null && await _userManager.CheckPasswordAsync(user, model.Password);
                if (user is null || !valid)
                {
                    return Response<string>.Fail("Invalid email or password.");
                }

                // generate JWT token
                var token = await GenerateJwtTokenAsync(user);
                user.SetToken(token);
                await _userManager.UpdateAsync(user);
                return Response<string>.Success(token, "Login successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during API login for {Email}", model.Email);
                return Response<string>.Fail("An unexpected error occurred during login.");
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

        public async Task<string> GenerateJwtTokenAsync(Model.Entities.User user)
        {
            var secretKey = _configuration["SecretKey"] ?? string.Empty;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
                new Claim("avatar", user.ProfileImageUrl ?? "UserProfileImages/Default.webp")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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
