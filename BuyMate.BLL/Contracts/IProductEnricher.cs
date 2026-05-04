using BuyMate.DTO.ViewModels.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.BLL.Contracts
{
    public interface IProductEnricher
    {
        Task EnrichAsync(IEnumerable<ProductViewModel> products, ClaimsPrincipal user);

    }
}
