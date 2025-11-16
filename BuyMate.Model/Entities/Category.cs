using BuyMate.Model.Common;
using System.ComponentModel.DataAnnotations;

namespace BuyMate.Model.Entities
{
    public class Category : ModifiableSoftDeleteEntity
    {
        public Guid? ParentCategoryId { get; set; }

        [Required]
        public string Name { get; set; }   = string.Empty;
       
        public Category? ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    }
}
