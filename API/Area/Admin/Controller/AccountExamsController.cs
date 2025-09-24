using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Area.Admin.Controller
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class AccountExamsController : ControllerBase
    {
        private readonly FlutterContext _context;

        public AccountExamsController(FlutterContext context)
        {
            _context = context;
        }

        // GET: api/AccountExams
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountExam>>> GetAccountExams([FromQuery] string? name)
        {
            var query = _context.AccountExams.AsQueryable();
            if (string.IsNullOrWhiteSpace(name))
            {
                //query = query.Where(acc =>
                //    acc..ToLower().Contains(name.ToLower())
                //);
            }
            return await query.Include(ae => ae.Exam).Include(ae => ae.Student).ToListAsync();
        }

        // GET: api/AccountExams/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AccountExam>> GetAccountExam(int id)
        {
            var accountExam = await _context.AccountExams.FindAsync(id);

            if (accountExam == null)
            {
                return NotFound();
            }

            return accountExam;
        }

        // PUT: api/AccountExams/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAccountExam(int id, AccountExam accountExam)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != accountExam.AccountExamId)
            {
                return BadRequest();
            }

            _context.Entry(accountExam).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountExamExists(id))
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

        // POST: api/AccountExams
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AccountExam>> PostAccountExam(AccountExam accountExam)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.AccountExams.Add(accountExam);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAccountExam", new { id = accountExam.AccountExamId }, accountExam);
        }

        // DELETE: api/AccountExams/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccountExam(int id)
        {
            var accountExam = await _context.AccountExams.FindAsync(id);
            if (accountExam == null)
            {
                return NotFound();
            }

            accountExam.Status = false;
            _context.Entry(accountExam).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AccountExamExists(int id)
        {
            return _context.AccountExams.Any(e => e.AccountExamId == id);
        }
    }
}
