using System;
using System.Collections.Generic;

namespace BuyMate.DTO.ViewModels
{
 public class SearchViewModel
 {
 public string? Query { get; set; }
 public List<ProductViewModel> Results { get; set; } = new List<ProductViewModel>();
 }
}
