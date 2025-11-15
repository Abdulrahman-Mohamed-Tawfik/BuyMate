using System;
using System.Collections.Generic;
using System.Linq;

namespace BuyMate.DTO.ViewModels
{
 public class CartViewModel
 {
 public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

 public decimal Subtotal => Items.Sum(i => i.TotalPrice);
 public decimal Shipping =>0m;
 public decimal Tax =>0m;
 public decimal Total => Subtotal + Shipping + Tax;
 }
}
