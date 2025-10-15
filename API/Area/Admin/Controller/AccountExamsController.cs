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
        public async Task<ActionResult<IEnumerable<AccountExamDto>>> GetAccountExams([FromQuery] string? name, string? status)
        {
            var query = _context.AccountExams.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(ae => ae.Student.Name.Contains(name));
            }
            if (!string.IsNullOrWhiteSpace(status) && status.ToLower().Trim().Equals("true"))
            {
                query = query.Where(cs => cs.Status == bool.Parse(status.ToLower().Trim()));
            }

            var result = await query
                .Include(ae => ae.Exam)
                .Include(ae => ae.Student)
                .Include(ae => ae.CourseSubject)
                    .ThenInclude(cs => cs.Subject)
                .Include(ae => ae.CourseSubject)
                    .ThenInclude(cs => cs.Course)
                .Select(ae => new AccountExamDto
                {
                    AccountExamId = ae.AccountExamId,
                    ExamId = ae.ExamId,
                    ExamName = ae.Exam.Name,
                    StudentId = ae.StudentId,
                    StudentName = ae.Student.Name,

                    Subject = ae.CourseSubject.Subject.Name,
                    SubjectId = ae.CourseSubject.SubjectId,

                    Course = ae.CourseSubject.Course.Name,
                    CourseId = ae.CourseSubject.CourseId,

                    Score = ae.Score,
                    IsPass = ae.IsPass,
                    Status = ae.Status,
                })
                .ToListAsync();

            return result;
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
