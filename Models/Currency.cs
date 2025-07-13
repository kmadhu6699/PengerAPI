using System.Collections.Generic;

namespace PengerAPI.Models
{
    public class Currency : BaseEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation property
        public virtual ICollection<Account> Accounts { get; set; }
    }
}
