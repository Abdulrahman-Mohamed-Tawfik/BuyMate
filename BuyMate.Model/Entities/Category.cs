using BuyMate.Model.Common;

namespace BuyMate.Model.Entities
{
    public class Category : ModifiableSoftDeleteEntity
    {
        public Guid? ParentCategoryId { get; set; }
        public required string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

       
        public Category? ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    }
}
