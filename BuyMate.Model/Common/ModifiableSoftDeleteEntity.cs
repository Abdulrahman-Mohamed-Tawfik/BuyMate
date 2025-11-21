namespace BuyMate.Model.Common
{
    public class ModifiableSoftDeleteEntity : ModifiableEntity
    {
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
