using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PengerAPI.Models;

namespace PengerAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<OTP> OTPs { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<AccountType> AccountTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints here
            modelBuilder.Entity<Account>()
                .HasOne(a => a.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OTP>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.AccountType)
                .WithMany(at => at.Accounts)
                .HasForeignKey(a => a.AccountTypeId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Currency)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}