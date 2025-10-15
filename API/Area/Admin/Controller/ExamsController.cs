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
    public class ExamsController : ControllerBase
    {
        private readonly FlutterContext _context;

        public ExamsController(FlutterContext context)
        {
            _context = context;
        }

        // GET: api/Exams
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExamDto>>> GetExams([FromQuery] string? name, string? status)
        {
            var query = _context.Exams.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(e => e.Name.ToLower().Contains(name.ToLower().Trim()));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                bool isActive = status.ToLower().Trim() == "true";
                query = query.Where(e => e.Status == isActive);
            }

            var result = await query
                .Include(e => e.Account)
                .Include(e => e.CourseSubject)
                    .ThenInclude(cs => cs.Subject)
                .Include(e => e.CourseSubject)
                    .ThenInclude(cs => cs.Course)
                .Include(e => e.Room)
                .Select(e => new ExamDto
                {
                    ExamId = e.ExamId,
                    Name = e.Name,
                    AccountId = e.AccountId,
                    AccountName = e.Account.Name,
                    ExamDayString = e.ExamDay.ToString("yyyy-MM-dd"),
                    ExamTimeString = e.ExamTime.ToString(@"hh\:mm"),
                    CourseSubjectId = e.CourseSubjectId,
                    SubjectName = e.CourseSubject.Subject != null ? e.CourseSubject.Subject.Name : "Không rõ môn",
                    CourseName = e.CourseSubject.Course != null ? e.CourseSubject.Course.Name : "Không rõ khóa học",
                    RoomId = e.RoomId,
                    RoomName = e.Room.Name,
                    Status = e.Status,
                    Fee = e.Fee,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return result;
        }


        // GET: api/Exams/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Exam>> GetExam(int id)
        {
            var exam = await _context.Exams.FindAsync(id);

            if (exam == null)
            {
                return NotFound();
            }

            return exam;
        }

        // PUT: api/Exams/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExam(int id, Exam exam)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != exam.ExamId)
            {
                return BadRequest();
            }

            _context.Entry(exam).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExamExists(id))
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

        // POST: api/Exams
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Exam>> PostExam(Exam exam)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetExam", new { id = exam.ExamId }, exam);
        }

        // DELETE: api/Exams/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExam(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null)
            {
                return NotFound();
            }

            exam.Status = false;
            _context.Entry(exam).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ExamExists(int id)
        {
            return _context.Exams.Any(e => e.ExamId == id);
        }
    }
}
