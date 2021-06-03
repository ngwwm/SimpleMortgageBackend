using System;
using System.Collections.Generic;
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
    public class ApplicantsController : ControllerBase
    {
        private readonly SimpleMortgageDbContext _context;

        /* below 2 constants are better put in configuration file or database */
        private static int MAX_ALLOWED_LTV = 90;
        private static int MIN_ALLOWED_AGE = 18;

        public ApplicantsController(SimpleMortgageDbContext context)
        {
            _context = context;
        }

        // GET: api/Applicants
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Applicant>>> GetApplicants()
        {
            return await _context.Applicants.ToListAsync();
        }

        // GET: api/Applicants/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Applicant>> GetApplicant(long id)
        {
            var applicant = await _context.Applicants.FindAsync(id);

            if (applicant == null)
            {
                return NotFound();
            }

            return applicant;
        }

        // GET: api/Applicants/1/Products?PropertyVal=n&DepositAmt=n
        [HttpGet("{id}/Products")]
        public async Task<ActionResult<IEnumerable<Product>>> GetApplicant(int id, double propertyval, double depositamt)
        {
            if (propertyval <= 0)
                return BadRequest(new { error = "Property Value cannot be less than or equal to zero." });

            if (depositamt <= 0)
                return BadRequest(new { error = "Deposit Amount cannot be less than or equal to zero." });

            /* If the LTV is not less than 90%, or the applicant is under 18, no products should be returned. */
            var ltv = (propertyval - depositamt) / propertyval * 100;

            if (ltv > MAX_ALLOWED_LTV)
            {
                return BadRequest(new { error = "Loan to Value (" + ltv + "%) cannot exceed 90%." } );
            }

            var applicant = await _context.Applicants.FindAsync(id);
            if (applicant == null)
            {
                return BadRequest(new { error = "Applicant not exists" });
            }

            var age = (DateTime.Now - applicant.DOB).TotalDays / 365;


            if (age < MIN_ALLOWED_AGE)
            {
                return BadRequest(new { error = "Applicant age (" + Math.Round(age,1) + ") cannot be under 18." });
            }

            return await _context.Products.Where(p => p.LTV >= ltv).ToListAsync();
        }


        // PUT: api/Applicants/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutApplicant(long id, Applicant applicant)
        {
            if (id != applicant.Id)
            {
                return BadRequest();
            }

            _context.Entry(applicant).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApplicantExists(id))
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

        // POST: api/Applicants
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Applicant>> PostApplicant(Applicant applicant)
        {
            var appl = _context.Applicants.Where(e => e.FirstName.ToLower() == applicant.FirstName.Trim().ToLower())
                    .Where(e => e.LastName.ToLower() == applicant.LastName.Trim().ToLower())
                    .Where(e => e.FirstName.ToLower() == applicant.FirstName.Trim().ToLower())
                    .Where(e => e.DOB == applicant.DOB)
                    .Where(e => e.Email == applicant.Email.Trim().ToLower()).SingleOrDefaultAsync();
            
            if ( appl.Result == null) 
            {
                _context.Applicants.Add(applicant);
                await _context.SaveChangesAsync();
                /* StatusCodes.Status201Created */
                return CreatedAtAction("GetApplicant", new { id = applicant.Id }, applicant);
            } else
            {
                return StatusCode(StatusCodes.Status200OK, appl.Result);
            }

        }

        // DELETE: api/Applicants/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApplicant(long id)
        {
            var applicant = await _context.Applicants.FindAsync(id);
            if (applicant == null)
            {
                return NotFound();
            }

            _context.Applicants.Remove(applicant);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ApplicantExists(long id)
        {
            return _context.Applicants.Any(e => e.Id == id);
        }
    }
}
