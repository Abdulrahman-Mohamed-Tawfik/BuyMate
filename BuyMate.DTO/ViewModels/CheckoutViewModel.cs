using System;
using System.Collections.Generic;

namespace BuyMate.DTO.ViewModels
{
 public class CheckoutViewModel
 {
 public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();

 public ShippingAddressViewModel ShippingAddress { get; set; } = new ShippingAddressViewModel();
 public string PaymentMethod { get; set; } = "Card";
 public string DeliveryType { get; set; } = "Standard";
 }
}
