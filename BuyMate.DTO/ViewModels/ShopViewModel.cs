using System;
using System.Collections.Generic;

namespace BuyMate.DTO.ViewModels
{
    public class ShopViewModel
    {
        public string? SelectedCategory { get; set; }

        // ⭐ مطلوب عشان الفلترة بالـ Id
        public Guid? SelectedCategoryId { get; set; }

        public List<CategoryViewModel> Categories { get; set; } = new();
        public List<ProductViewModel> Products { get; set; } = new();

        // Search
        public string? Search { get; set; }

        // Filters
        public List<string> Brands { get; set; } = new();
        public string? SelectedBrand { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public decimal? SelectedMinPrice { get; set; }
        public decimal? SelectedMaxPrice { get; set; }

        public bool? HasDiscount { get; set; }
        public bool? IsFeatured { get; set; }

        // Sorting
        public string? OrderBy { get; set; }
        public bool Asc { get; set; } = true;

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalCount { get; set; }
    }
}
