using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.DTO.ViewModels
{
    public class ProductViewModel
    {
       
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public decimal Price { get; set; }

            // Primary image URL for product thumbnails
            public string? ImageUrl { get; set; }

            // Optional additional images for gallery
            public List<string> ImageUrls { get; set; } = new();
            public List<CategoryViewModel> Categories { get; set; } = new();

            // Optional original price when product is on discount
            public decimal? OriginalPrice { get; set; }

            // Discount percentage (e.g.20 for20%)
            public int? Discount { get; set; }

            // Brand name
            public string? Brand { get; set; }

            // Average rating (0-5) - allow fractional rating
            public double Rating { get; set; } =0.0;

            // Number of reviews
            public int ReviewCount { get; set; } =0;

            // Stock quantity available
            public int Stock { get; set; } =0;

            // Product specifications
            public Dictionary<string, string> Specifications { get; set; } = new();

            // Whether the product is part of a flash deal
            public bool IsFlashDeal { get; set; } = false;

            public bool IsFeatured { get; set; }
            public bool IsBestSeller { get; set; }
        

    }
}
