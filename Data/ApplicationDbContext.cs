using Microsoft.EntityFrameworkCore;
using PengerApi.Models;

namespace PengerApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add DbSets for your entities here
        // public DbSet<User> Users { get; set; }
        // public DbSet<Currency> Currencies { get; set; }
        // public DbSet<AccountType> AccountTypes { get; set; }
        // public DbSet<Account> Accounts { get; set; }
        // public DbSet<OTP> OTPs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure entity relationships and constraints here
        }
    }
}