using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.DTO.ViewModels
{
    public class CategoryViewModel
    {
        
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty; // added for listing

            // Number of products in this category (used in shop views)
            public int ProductCount { get; set; } =0;
    }
}
