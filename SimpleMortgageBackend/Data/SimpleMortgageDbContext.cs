using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleMortgageBackend.Models;

namespace SimpleMortgageBackend.Data
{
    public class SimpleMortgageDbContext : DbContext
    {
        public SimpleMortgageDbContext(DbContextOptions<SimpleMortgageDbContext> options) : base(options)
        {
        }

        public DbSet<Applicant> Applicants { get; set; }

        public DbSet<Product> Products { get; set; }
    }
}
