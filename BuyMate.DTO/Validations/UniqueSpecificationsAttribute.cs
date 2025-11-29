using BuyMate.DTO.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.DTO.Validations
{
    public class UniqueSpecificationsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var list = value as List<ProductSpecficationInput>;
            if (list == null || !list.Any())
                return ValidationResult.Success;

            // Check for duplicate keys (case-insensitive)
            var duplicates = list
                .GroupBy(x => x.Key?.Trim().ToLower())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicates.Any())
                return new ValidationResult("Each specification key must be unique.");

            return ValidationResult.Success;
        }
    }
}
