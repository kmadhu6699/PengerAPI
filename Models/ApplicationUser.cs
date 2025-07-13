using Microsoft.AspNetCore.Identity;

namespace PengerAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }
        public string RememberToken { get; set; }

        // Navigation properties
        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<OTP> OTPs { get; set; }
    }
}
