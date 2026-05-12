using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public static class DataSeeder
    {
        public static async Task SeedDatabaseAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

            // Check if data already exists to respect the KISS principle
            if (await context.Customers.AnyAsync())
                return;

            var customers = new List<Customer>();
            var turkishNames = new[] { "Ahmet Yılmaz", "Ayşe Demir", "Mehmet Kaya", "Fatma Çelik", "Canan Özkan", "Ali Yıldız", "Zeynep Arslan", "Mustafa Şahin", "Hatice Doğan", "Burak Çetin" };
            
            var periods = new[] { "2025_11", "2025_12", "2026_01", "2026_02", "2026_03", "2026_04" };
            var random = new Random(42); // deterministic seed

            var now = dateTimeProvider.UtcNow;

            for (int i = 0; i < 35; i++)
            {
                var nameParts = turkishNames[i % turkishNames.Length].Split(' ');
                var firstName = nameParts[0];
                var lastName = nameParts[1];
                var identityNumber = (10000000001L + i).ToString();

                var cleanFirstName = NormalizeString(firstName);
                var cleanLastName = NormalizeString(lastName);

                var email = $"{cleanFirstName}.{cleanLastName}{i}@example.com";

                var customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    FirstName = firstName,
                    LastName = lastName,
                    IdentityNumber = identityNumber,
                    Email = email,
                    PhoneNumber = $"555100{i:D4}",
                    IsDeleted = false,
                    Subscriptions = new List<Subscription>()
                };

                // Generate ~3-4 subscriptions per customer
                int subCount = random.Next(3, 5);
                var possibleServices = new[] 
                { 
                    (SubscriptionType.Elektrik, "EnerjiSA"),
                    (SubscriptionType.Su, "İSKİ"),
                    (SubscriptionType.Doğalgaz, "İGDAŞ"),
                    (SubscriptionType.İnternet, "TurkNet"),
                    (SubscriptionType.CepTelefonu, "Turkcell"),
                    (SubscriptionType.CepTelefonu, "Vodafone"),
                    (SubscriptionType.Televizyon, "Digiturk"),
                    (SubscriptionType.Sigorta, "Allianz")
                };

                for (int j = 0; j < subCount; j++)
                {
                    var service = possibleServices[random.Next(possibleServices.Length)];
                    var subscription = new Subscription
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = customer.Id,
                        SubscriptionType = service.Item1,
                        ServiceProviderName = service.Item2,
                        SubscriptionNumber = $"SUB-{i}-{j}-{random.Next(1000, 9999)}",
                        IsActive = true,
                        IsDeleted = false,
                        // NextDueDate between -5 and +25 days
                        NextDueDate = now.AddDays(random.Next(-5, 26)),
                        // Persistent realistic CurrentDebtAmount
                        CurrentDebtAmount = random.NextDouble() > 0.85 ? 0.00m : Math.Round((decimal)(random.NextDouble() * 750 + 50), 2),
                        Payments = new List<Payment>()
                    };

                    // Generate payment history
                    // 4 to 5 historical payments
                    int paymentCount = random.Next(4, 6);
                    for (int k = 0; k < paymentCount; k++)
                    {
                        var period = periods[k]; // Deterministic, ensuring no duplicates per subscription
                        var parts = period.Split('_');
                        var year = int.Parse(parts[0]);
                        var month = int.Parse(parts[1]);
                        var basePaymentDate = new DateTime(year, month, random.Next(1, 28), random.Next(8, 18), random.Next(0, 60), 0, DateTimeKind.Utc);
                        
                        // One successful payment
                        subscription.Payments.Add(new Payment
                        {
                            Id = Guid.NewGuid(),
                            SubscriptionId = subscription.Id,
                            Amount = Math.Round((decimal)(random.NextDouble() * 450 + 50), 2),
                            PaymentDate = basePaymentDate,
                            Period = period,
                            IsSuccessful = true
                        });

                        // Add a failed attempt to make realistic payment scenarios
                        if (random.NextDouble() > 0.7)
                        {
                            subscription.Payments.Add(new Payment
                            {
                                Id = Guid.NewGuid(),
                                SubscriptionId = subscription.Id,
                                Amount = Math.Round((decimal)(random.NextDouble() * 450 + 50), 2),
                                PaymentDate = basePaymentDate.AddDays(-random.Next(1, 3)),
                                Period = period,
                                IsSuccessful = false
                            });
                        }
                    }

                    customer.Subscriptions.Add(subscription);
                }

                customers.Add(customer);
            }

            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();
        }
        
        private static string NormalizeString(string text)
        {
            return text.ToLowerInvariant()
                .Replace('ı', 'i')
                .Replace('ş', 's')
                .Replace('ç', 'c')
                .Replace('ğ', 'g')
                .Replace('ö', 'o')
                .Replace('ü', 'u');
        }
    }
}
