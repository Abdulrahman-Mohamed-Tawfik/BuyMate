using BuyMate.BLL.Contracts;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.BLL.Features.User
{
    public class UserProfileService: IUserProfileService
    {
        private readonly UserManager<Model.Entities.User> _userManager;

        public UserProfileService(UserManager<Model.Entities.User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Response<ProfileViewModel>> GetProfileAsync(System.Security.Claims.ClaimsPrincipal userPrincipal)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null) return null;

            var profile =  new ProfileViewModel
            {
                Name = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                Phone = user.PhoneNumber,
                Avatar = user.ProfileImageUrl
            };

            return new Response<ProfileViewModel>
            {
                Data = profile,
                Status = true,
                Message="Profile"
            };

        }
    }
}
