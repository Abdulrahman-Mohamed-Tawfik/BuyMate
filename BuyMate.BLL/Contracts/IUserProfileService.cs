using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BuyMate.BLL.Contracts
{
    public interface IUserProfileService
    {
        Task<Response<ProfileViewModel>> GetProfileAsync(System.Security.Claims.ClaimsPrincipal user);
        Task<Response<bool>> UpdateProfileAsync(ProfileViewModel model, ClaimsPrincipal userPrincipal, IFormFile? avatarFile);
    }
}
