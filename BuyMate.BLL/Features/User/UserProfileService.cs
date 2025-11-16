using BuyMate.BLL.Contracts;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BuyMate.BLL.Features.User
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UserManager<Model.Entities.User> _userManager;
        private readonly SignInManager<Model.Entities.User> _signInManager;

        public UserProfileService(UserManager<Model.Entities.User> userManager, SignInManager<Model.Entities.User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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

        public async Task<Response<bool>> UpdateProfileAsync(ProfileViewModel model, ClaimsPrincipal userPrincipal)
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

            if (user.Email != model.Email)
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

            if(user.PhoneNumber != model.Phone)
            {
                var phoneExists = _userManager.Users.Any(u => u.PhoneNumber == model.Phone);
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

            if (!string.IsNullOrEmpty(model.Avatar))
            {
                // In this simple implementation we accept avatar URL
                var prop = user.GetType().GetProperty("ProfileImageUrl");
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(user, model.Avatar);
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

            //Adding avatar image to be used with user claim
            //  Remove old avatar claim(if exists)
            var oldClaim = (await _userManager.GetClaimsAsync(user))
                          .FirstOrDefault(c => c.Type == "avatar");
            if (oldClaim != null)
                await _userManager.RemoveClaimAsync(user, oldClaim);

            //Add new avatar claim
            var avatarUrl = user.ProfileImageUrl;

            await _userManager.AddClaimAsync(user, new Claim("avatar", avatarUrl));

            await _signInManager.RefreshSignInAsync(user);



            return new Response<bool>
            {
                Status = true,
                Message = "Profile updated successfully",
                Data = true
            };
        }
    }
}
