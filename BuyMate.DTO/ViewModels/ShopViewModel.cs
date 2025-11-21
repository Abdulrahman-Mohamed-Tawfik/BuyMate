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
    }
}
