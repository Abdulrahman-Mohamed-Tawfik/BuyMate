using BuyMate.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.Model.Entities
{
    public class Category : ModifiableEntity
    {

        public Guid? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }



        [Required]
        public string Name { get; set; }   = string.Empty;
       

        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();


    }
}
