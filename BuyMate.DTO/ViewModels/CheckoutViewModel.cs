namespace BuyMate.DTO.ViewModels;

public class CheckoutViewModel
{
    public CartViewModel CartVm { get; set; } = new CartViewModel();

    public ShippingAddressViewModel ShippingAddress { get; set; } = new ShippingAddressViewModel();
    public string PaymentMethod { get; set; } = "Card";
    public string DeliveryType { get; set; } = "Standard";
}
