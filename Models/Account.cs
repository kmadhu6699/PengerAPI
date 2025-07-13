namespace PengerAPI.Models
{
    public class Account : BaseEntity
    {
        public int UserId { get; set; }
        public int AccountTypeId { get; set; }
        public int CurrencyId { get; set; }
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual AccountType AccountType { get; set; }
        public virtual Currency Currency { get; set; }
    }
}
