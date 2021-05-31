using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleMortgageBackend.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Lender { get; set; }
        public double InterestRate { get; set; }
        public string InterestTerm { get; set; }
        public double LTV { get; set; }
    }
}
