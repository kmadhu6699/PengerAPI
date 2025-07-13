using System.Collections.Generic;

namespace PengerAPI.Models
{
    public class AccountType : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        
        // Navigation property
        public virtual ICollection<Account> Accounts { get; set; }
    }
}
