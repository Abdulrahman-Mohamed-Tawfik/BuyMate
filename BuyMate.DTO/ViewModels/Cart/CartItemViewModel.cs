namespace BuyMate.DTO.ViewModels.Cart;

public class CartItemViewModel
{
    public Guid ItemId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal PriceAtAddition { get; set; }

    public decimal TotalPrice => PriceAtAddition * Quantity;
}
