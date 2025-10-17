using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Area.Admin.Controller
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class RegistersController : ControllerBase
    {
        private readonly FlutterContext _context;

        public RegistersController(FlutterContext context)
        {
            _context = context;
        }

        // GET: api/Registers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegisterDto>>> GetRegisters([FromQuery] string? name, string? status)
        {
            var query = _context.Registers
                        .Include(r => r.Student)
                        .Include(r => r.Exam)
                            .ThenInclude(e => e.Room)
                        .Include(r => r.CourseSubject)
                            .ThenInclude(cs => cs.Subject)
                        .Include(r => r.CourseSubject)
                            .ThenInclude(cs => cs.Course)
                        .AsQueryable();
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(r =>
                    r.Student.Name.ToLower().Contains(name.ToLower().Trim())
                );
            }
            if (!string.IsNullOrWhiteSpace(status) && status.ToLower().Trim().Equals("true"))
            {
                query = query.Where(cs => cs.Status == bool.Parse(status.ToLower().Trim()));
            }
            var result = await query.Select(r => new RegisterDto
            {
                RegisterId = r.RegisterId,
                StudentId = r.StudentId,
                StudentName = r.Student.Name,
                SubjectId = r.CourseSubject.SubjectId,
                ExamId = r.ExamId,
                Email = r.Email,
                Status = r.Status,
                Payment = r.payment,
                CreatedAt = r.CreatedAt,
                ExamName = r.Exam.Name,
                RoomName = r.Exam.Room.Name,
                ExamFee = r.Exam.Fee
            }).ToListAsync();
            return result;
        }

        // GET: api/Registers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RegisterDto>> GetRegister(int id)
        {
            var register = await _context.Registers
                                .Include(r => r.Student)
                                .Include(r => r.Exam)
                                    .ThenInclude(e => e.Room)
                                .Include(r => r.CourseSubject)
                                    .ThenInclude(cs => cs.Subject)
                                .Include(r => r.CourseSubject)
                                    .ThenInclude(cs => cs.Course)
                                .Where(r => r.RegisterId == id) 
        .Select(r => new RegisterDto
        {
            RegisterId = r.RegisterId,
            StudentId = r.StudentId,
            StudentName = r.Student.Name,
            SubjectId = r.CourseSubject.SubjectId,
            ExamId = r.ExamId,
            Email = r.Email,
            Status = r.Status,
            Payment = r.payment,
            CreatedAt = r.CreatedAt,
            ExamName = r.Exam.Name,
            RoomName = r.Exam.Room.Name,
            ExamFee = r.Exam.Fee
        })
        .FirstOrDefaultAsync();

            if (register == null)
                return NotFound();

            return register;
        }

        // PUT: api/Registers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRegister(int id, Register register)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != register.RegisterId)
            {
                return BadRequest();
            }

            _context.Entry(register).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RegisterExists(id))
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

        // POST: api/Registers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Register>> PostRegister(Register register)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.Registers.Add(register);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRegister", new { id = register.RegisterId }, register);
        }

        // DELETE: api/Registers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegister(int id)
        {
            var register = await _context.Registers.FindAsync(id);
            if (register == null)
            {
                return NotFound();
            }

            register.Status = false;
            _context.Entry(register).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RegisterExists(int id)
        {
            return _context.Registers.Any(e => e.RegisterId == id);
        }
    }
}
