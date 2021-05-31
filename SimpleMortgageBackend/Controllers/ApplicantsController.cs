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
            //var b = _context.Applicants.SingleOrDefaultAsync(e => e.Id == applicant.Id);
            if ( appl.Result == null) 
            {
                _context.Applicants.Add(applicant);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetApplicant", new { id = applicant.Id }, applicant);
            } else
            {
                return appl.Result;
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
