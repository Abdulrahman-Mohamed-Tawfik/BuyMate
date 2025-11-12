using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.BLL.Contracts
{
    public interface IUserProfileService
    {
        Task<Response<ProfileViewModel>> GetProfileAsync(System.Security.Claims.ClaimsPrincipal user);
    }
}
