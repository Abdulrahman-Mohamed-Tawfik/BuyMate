using System;

namespace BuyMate.DTO.ViewModels
{
    public class CartItemViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal PriceAtAddition { get; set; }

        public decimal TotalPrice => PriceAtAddition * Quantity;
    }
}
