using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.DTO.ViewModels
{
    public class HomeViewModel
    {
        public List<CategoryViewModel> Categories { get; set; } = new();
        public List<ProductViewModel> FeaturedProducts { get; set; } = new();
        public List<ProductViewModel> BestSellers { get; set; } = new();
    }

}
