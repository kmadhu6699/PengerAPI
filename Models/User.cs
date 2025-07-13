using System;
using System.Collections.Generic;

namespace PengerAPI.Models
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }
        public string RememberToken { get; set; }
        
        // Navigation properties
        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<OTP> OTPs { get; set; }
    }
}
