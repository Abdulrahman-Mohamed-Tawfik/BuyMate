using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.BLL.Contracts
{
    public interface IOrderService
    {

        public Task<Response<bool>> CreateOrderAsync(CheckoutViewModel model, string userId);

    }
}
