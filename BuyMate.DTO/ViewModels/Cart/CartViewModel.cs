namespace BuyMate.DTO.ViewModels.Cart;

public class CartViewModel
{
    public Guid CartId { get; set; }
    public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

    public decimal Subtotal => Items.Sum(i => i.TotalPrice);
    public decimal Shipping => 0m;
    public decimal Tax => 0m;
    public decimal Total => Subtotal + Shipping + Tax;
}
