using System;

namespace BuyMate.DTO.Common
{
    public class ProductFilter
    {
        public string? Search { get; set; }
        public string? OrderBy { get; set; }
        public bool Asc { get; set; } = true;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 6;

        public Guid? CategoryId { get; set; }
        public string? Brand { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? HasDiscount { get; set; }
        public bool? IsFeatured { get; set; }
        
        public bool? Paginate { get; set; } = false;
    }
}