using System.Collections.Generic;
using System;

namespace BuyMate.DTO.ViewModels
{
 public class ShopViewModel
 {
 public string? SelectedCategory { get; set; }
 public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
 public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
 }
}
