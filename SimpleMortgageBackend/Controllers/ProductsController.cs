using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleMortgageBackend.Data;
using SimpleMortgageBackend.Models;

namespace SimpleMortgageBackend.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly SimpleMortgageDbContext _context;
        private static int MAX_ALLOWED_LTV = 90;
        private static int MIN_ALLOWED_AGE = 18;

        public ProductsController(SimpleMortgageDbContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        //public async Task<ActionResult<IEnumerable<Product>>> GetProduct()
        public async Task<ActionResult<IEnumerable<Product>>> GetProduct(int? applicantid, double? propertyval, double? depositamt)
        {
            if (applicantid == null)
                return await _context.Products.ToListAsync();

            if (propertyval == null || depositamt == null)
                return NotFound(new { error = "Property Value/Deposit Amount is missing." });

            var applicant = await _context.Applicants.FindAsync(applicantid);
            if (applicant == null)
            {
                return NotFound(new { error = "Applicant not exists." });
            }

            /* If the LTV is not less than 90%, or the applicant is under 18, no products should be returned. */
            var ltv = (propertyval - depositamt) / propertyval * 100;
            var age = (DateTime.Now - applicant.DOB).TotalDays / 365;

            if (ltv >= MAX_ALLOWED_LTV || age < MIN_ALLOWED_AGE)
            {
                return NotFound(new { error = "Loan to Value is not less than 90% or applicant is under 18." });
            }

            return await _context.Products.Where(p => p.LTV >= ltv).ToListAsync();
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // GET: api/Products/Applicant/1?PropertyVal=n&DepositAmt=n
        [HttpGet("Applicant/{applicantid}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProduct(int applicantid, int propertyval, int depositamt)
        {
            var applicant = await _context.Applicants.FindAsync(applicantid);
            if (applicant == null)
            {
                return NotFound();
            }

            /* If the LTV is not less than 90%, or the applicant is under 18, no products should be returned. */
            var ltv = (propertyval - depositamt) / propertyval * 100;
            var age = (DateTime.Now - applicant.DOB).TotalDays / 365;
            
            if (ltv > MAX_ALLOWED_LTV || age < MIN_ALLOWED_AGE)
            {
                return NotFound();
            }

            return await _context.Products.Where(p => p.LTV >= ltv).ToListAsync();
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
