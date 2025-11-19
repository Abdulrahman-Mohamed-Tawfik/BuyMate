using BuyMate.Model.Common;

namespace BuyMate.Model.Entities
{
    public class ProductSpecification: ModifiableSoftDeleteEntity
    {
        public Guid ProductId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public Product Product { get; set; }
    }
}
