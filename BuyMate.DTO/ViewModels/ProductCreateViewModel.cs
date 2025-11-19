using System;
using System.ComponentModel.DataAnnotations;

namespace BuyMate.DTO.ViewModels
{
    public class ProductCreateViewModel
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public List<string> ImageUrls { get; set; } = new();
        public List<Guid> CategoryIds { get; set; } = new();
    }
}