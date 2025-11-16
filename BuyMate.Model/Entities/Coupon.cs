using BuyMate.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.Model.Entities
{
    public class Coupon : ModifiableEntity
    {
        [Required]
        public string Code { get; set; }

        // "Percentage" or "Fixed"
        public string DiscountType { get; set; } = "Percentage";

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public int MaxUsage { get; set; } = 1; // how many times it can be used
        public int UsageCount { get; set; } = 0; // how many times it has been used


        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
