using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace BuyMate.BLL.Contracts
{
    public interface IAuthService
    {
        Task<Response<IdentityResult>> RegisterAsync(RegisterViewModel model);

    }
}
