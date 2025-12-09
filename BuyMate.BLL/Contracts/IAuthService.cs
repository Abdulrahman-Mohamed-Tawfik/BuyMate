using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels.User;

namespace BuyMate.BLL.Contracts
{
    public interface IAuthService
    {
        Task<Response<bool>> RegisterAsync(RegisterViewModel model);
        Task<Response<bool>> LoginAsync(LoginViewModel model);
        Task<Response<bool>> LogoutAsync();
    }
}
