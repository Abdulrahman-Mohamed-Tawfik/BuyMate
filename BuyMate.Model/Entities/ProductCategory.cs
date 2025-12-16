using BuyMate.Model.Common;

namespace BuyMate.Model.Entities
{
    public class ProductCategory : ModifiableEntity
    {
        public Guid ProductId { get; set; }
        public Guid CategoryId { get; set; }

        public Product? Product { get; set; }
        public Category? Category { get; set; }
    }
}
