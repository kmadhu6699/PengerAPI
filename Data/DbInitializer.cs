using Microsoft.EntityFrameworkCore;
using PengerAPI.Models;

namespace PengerAPI.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();

                // Ensure database is created
                await context.Database.MigrateAsync();

                // Seed data
                await SeedCurrenciesAsync(context);
                await SeedAccountTypesAsync(context);

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task SeedCurrenciesAsync(ApplicationDbContext context)
        {
            if (await context.Currencies.AnyAsync())
            {
                return; // DB has already been seeded with currencies
            }

            var currencies = new List<Currency>
            {
                new Currency { Code = "USD", Name = "US Dollar", Symbol = "$" },
                new Currency { Code = "EUR", Name = "Euro", Symbol = "€" },
                new Currency { Code = "GBP", Name = "British Pound", Symbol = "£" },
                new Currency { Code = "JPY", Name = "Japanese Yen", Symbol = "¥" },
                new Currency { Code = "INR", Name = "Indian Rupee", Symbol = "₹" }
            };

            await context.Currencies.AddRangeAsync(currencies);
        }

        private static async Task SeedAccountTypesAsync(ApplicationDbContext context)
        {
            if (await context.AccountTypes.AnyAsync())
            {
                return; // DB has already been seeded with account types
            }

            var accountTypes = new List<AccountType>
            {
                new AccountType { Name = "Savings", Description = "A basic savings account for storing money and earning interest" },
                new AccountType { Name = "Checking", Description = "A transactional account for day-to-day expenses" },
                new AccountType { Name = "Investment", Description = "An account for investment activities" },
                new AccountType { Name = "Credit Card", Description = "A credit card account" },
                new AccountType { Name = "Loan", Description = "A loan account" }
            };

            await context.AccountTypes.AddRangeAsync(accountTypes);
        }
    }
}
