using BuyMate.DTO.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using BuyMate.DTO.ViewModels.User;

namespace BuyMate.BLL.Contracts
{
    public interface IUserProfileService
    {
        Task<Response<ProfileViewModel>> GetProfileAsync(System.Security.Claims.ClaimsPrincipal user);
        Task<Response<bool>> UpdateProfileAsync(ProfileViewModel model, ClaimsPrincipal userPrincipal, IFormFile? avatarFile);
    }
}
