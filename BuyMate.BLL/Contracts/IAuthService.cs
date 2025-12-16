using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels.User;

namespace BuyMate.BLL.Contracts
{
    public interface IAuthService
    {
        Task<Response<bool>> RegisterAsync(RegisterViewModel model);
        Task<Response<bool>> LoginMvcAsync(LoginViewModel model);
        Task<Response<string>> LoginApiAsync(LoginViewModel model);
        Task<Response<bool>> LogoutAsync();
        Task<string> GenerateJwtTokenAsync(BuyMate.Model.Entities.User user);
    }
}
