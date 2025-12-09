using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels.Order;

namespace BuyMate.BLL.Contracts
{
    public interface IOrderService
    {

        public Task<Response<bool>> CreateOrderAsync(CheckoutViewModel model, string userId);
        public Task<Response<OrderViewModel>> GetUserOrderByIDForAdminAsync(Guid orderId);

        public Task<Response<OrderViewModel>> GetUserOrderByIDForUserAsync (Guid orderId, string userId);

        public Task<Response<List<OrderViewModel>>> GetUserOrdersAsync(string userId);

        public Task<Response<List<OrderViewModel>>> GetAllOrdersAsync();

        public Task<Response<bool>> CancelOrderAsync(Guid orderId, string userId);

        public Task<Response<bool>> DeleteOrderByAdminAsync(Guid orderId);
        public Task<Response<bool>> UpdateOrderStatusByAdminAsync(Guid orderId, int status);
      




    }
}
