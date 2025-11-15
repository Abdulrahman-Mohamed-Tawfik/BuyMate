using BuyMate.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.Model.Entities
{
    public class ProductCategory : ModifiableEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
