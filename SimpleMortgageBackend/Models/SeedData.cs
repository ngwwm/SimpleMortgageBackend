using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleMortgageBackend.Data;

namespace SimpleMortgageBackend.Models
{
    public static class SeedData
    {
        //public static void Initialize(SimpleMortgageDbContext context)
        public static void Initialize(IServiceProvider serviceProvider)
        {
            //public SimpleMortgageDbContext(DbContextOptions<SimpleMortgageDbContext> options) : base(options)
            using (var scope = serviceProvider.CreateScope())
            {
                using (var context = new SimpleMortgageDbContext(
                    scope.ServiceProvider.GetRequiredService<DbContextOptions<SimpleMortgageDbContext>>()))
                {
                    if (!context.Products.Any())
                    {
                        context.Products.AddRange(
                             new Product
                             {
                                 Lender = "Bank A",
                                 InterestRate = 2,
                                 InterestTerm = "Variale",
                                 LTV = 60
                             },
                             new Product
                             {
                                 Lender = "Bank B",
                                 InterestRate = 3,
                                 InterestTerm = "Fixed",
                                 LTV = 60
                             },
                             new Product
                             {
                                 Lender = "Bank C",
                                 InterestRate = 4,
                                 InterestTerm = "Variale",
                                 LTV = 90
                             }
                        );
                        context.SaveChanges();
                    }
                    if (!context.Applicants.Any())
                    {
                        context.Applicants.AddRange(
                             new Applicant
                             {
                                 FirstName = "Gordon",
                                 LastName = "Ramsey 17",
                                 DOB = DateTime.Parse("2003-01-01"),
                                 Email = "gordon@fakemail.com"
                             },
                             new Applicant
                             {
                                 FirstName = "Gordon",
                                 LastName = "Ramsey 18",
                                 DOB = DateTime.Parse("2003-10-01"),
                                 Email = "ramsey@fakemail.com"
                             }
                        );
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}
