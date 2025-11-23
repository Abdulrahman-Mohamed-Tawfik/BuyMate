using BuyMate.DTO.Common;
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

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be 0 or greater.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[\p{L}\p{N}\s\.\-']+$", ErrorMessage = "Brand may include letters, numbers, spaces and .-' only.")]
        public string Brand { get; set; } = string.Empty;

        public List<string> ImageUrls { get; set; } = new();
        public List<Guid> CategoryIds { get; set; } = new();

        public List<ProductSpecficationInput> Specifications { get; set; } = new();


    }

}