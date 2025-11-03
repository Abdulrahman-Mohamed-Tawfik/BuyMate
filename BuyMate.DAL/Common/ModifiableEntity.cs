namespace BuyMate.DAL.Common
{
    public class ModifiableEntity : BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
