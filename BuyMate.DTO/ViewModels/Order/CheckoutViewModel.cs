using BuyMate.DTO.ViewModels.Cart;
using BuyMate.DTO.ViewModels.Shop;

namespace BuyMate.DTO.ViewModels.Order;

public class CheckoutViewModel
{
    public CartViewModel CartVm { get; set; } = new CartViewModel();
    public ShippingAddressViewModel ShippingAddress { get; set; } = new ShippingAddressViewModel();
    public string PaymentMethod { get; set; } = "Card";
    public string DeliveryType { get; set; } = "Standard";
}
