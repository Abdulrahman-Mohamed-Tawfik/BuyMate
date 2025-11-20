using BuyMate.Model.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuyMate.Model.Entities
{
    public class Product : ModifiableSoftDeleteEntity
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountPercentage { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; } = 0;

        [Required, MaxLength(100)]
        public string? Brand { get; set; }



        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
        public ICollection<ProductSpecification> ProductSpecifications { get; set; } = new List<ProductSpecification>();

        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    }
}
