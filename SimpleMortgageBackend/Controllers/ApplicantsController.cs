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
        private const int MAX_ALLOWED_LTV = 90;
        private const int MIN_ALLOWED_AGE = 18;

        private const string VALI_PROPVAL_MSG001 = "Property Value cannot be less than or equal to zero";
        private const string VALI_DEPOSIT_MSG001 = "Deposit Amount cannot be less than or equal to zero.";
        private const string VALI_DEPOSIT_MSG002 = "Deposit Amount cannot be equal to or greater than Property Value.";
        private const string VALI_LNTOVAL_MSG001 = "Loan to Value ({0}%) cannot exceed {1}%.";
        private const string VALI_APPLCNT_MSG001 = "Applicant does not exist.";
        private const string VALI_APPLCNT_MSG002 = "Applicant age ({0}) cannot be under {1}.";

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
        public async Task<ActionResult<Applicant>> GetApplicant(int id)
        {
            var applicant = await _context.Applicants.FindAsync(id);

            if (applicant == null)
            {
                return NotFound();
            }

            return applicant;
        }

        private void PushErrorMessages(string key, IDictionary<string, List<string>> errors, string mesg) 
        {
            if (errors.ContainsKey(key))
                errors[key].Add(mesg);            
            else
                errors.Add(key, new List<string> { mesg });
        }

        // GET: api/Applicants/1/Products?PropertyVal=n&DepositAmt=n
        [HttpGet("{id}/Products")]
        public async Task<ActionResult<IEnumerable<Product>>> GetApplicant(int id, double propertyval, double depositamt)
        {
            /* validation errors */
            IDictionary<string, List<string>> errors = new Dictionary<string, List<string>>();
            
            if (propertyval <= 0)
                PushErrorMessages("propertyval", errors, VALI_PROPVAL_MSG001);

            if (depositamt <= 0)
                PushErrorMessages("depositamt", errors, VALI_DEPOSIT_MSG001);
            
            if (propertyval <= depositamt)
                PushErrorMessages("depositamt", errors, VALI_DEPOSIT_MSG002);

            /* If the LTV is not less than 90%, or the applicant is under 18, no products should be returned. */
            var ltv = 0.0;
            if (propertyval != 0) {
                ltv = (propertyval - depositamt) / propertyval * 100;

                if (ltv > MAX_ALLOWED_LTV)
                    PushErrorMessages("loan-to-value", errors, String.Format(VALI_LNTOVAL_MSG001, ltv, MAX_ALLOWED_LTV));                                    
            }

            var applicant = await _context.Applicants.FindAsync(id);
            if (applicant == null)
            {
                PushErrorMessages("applicant", errors, VALI_APPLCNT_MSG001);
            }
            else
            {
                var age = (DateTime.Now - applicant.DOB).TotalDays / 365;
                if (age < MIN_ALLOWED_AGE)
                    PushErrorMessages("applicant", errors, String.Format(VALI_APPLCNT_MSG002, Math.Round(age, 1), MIN_ALLOWED_AGE));
            }

            /* return any validation errors */
            if (errors.Count > 0)
            {
                IDictionary<string, string[]> vErrors = new Dictionary<string, string[]>();

                foreach(var e in errors)
                {
                    vErrors.Add(e.Key, e.Value.ToArray());
                }
                var problemDetails = new ValidationProblemDetails(vErrors);

                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Product Search";
                problemDetails.Detail = "Product Search Validation Failed";
                
                return ValidationProblem(problemDetails);
            } else
                return await _context.Products.Where(p => p.LTV >= ltv).ToListAsync();
        }


        // PUT: api/Applicants/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutApplicant(int id, Applicant applicant)
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
        public async Task<IActionResult> DeleteApplicant(int id)
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

        private bool ApplicantExists(int id)
        {
            return _context.Applicants.Any(e => e.Id == id);
        }
    }
}
