using System;

namespace PengerAPI.Models
{
    public class OTP : BaseEntity
    {
        public int UserId { get; set; }
        public string Code { get; set; }
        public DateTime ExpiresAt { get; set; }
        
        // Navigation property
        public virtual User User { get; set; }
    }
}
