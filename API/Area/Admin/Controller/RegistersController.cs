using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;

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

        // GET: api/admin/registers?name=&status=
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Register>>> GetRegisters([FromQuery] string? name, string? status)
        {
            var query = _context.Registers
                .Include(r => r.Student)
                .Include(r => r.Exam)
                .ThenInclude(e => e.Room)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(r => r.Student != null && r.Student.Name.ToLower().Contains(name.ToLower().Trim()));

            if (!string.IsNullOrWhiteSpace(status) && bool.TryParse(status, out var st))
                query = query.Where(cs => cs.Status == st);

            return await query.ToListAsync();
        }

        // GET: api/admin/registers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Register>> GetRegister(int id)
        {
            var register = await _context.Registers
                .Include(r => r.Student)
                .Include(r => r.Exam)
                .ThenInclude(e => e.Room)
                .FirstOrDefaultAsync(r => r.RegisterId == id);

            if (register == null)
                return NotFound();

            return register;
        }

        // POST: api/admin/registers
        // POST: api/admin/registers
        [HttpPost]
        public async Task<ActionResult<object>> CreateRegister([FromBody] TempRegisterRequest req)
        {
            var student = await _context.Accounts.FindAsync(req.StudentId);
            if (student == null || student.Role != 2)
                return BadRequest(new { message = "Chỉ sinh viên (role=2) được đăng ký." });

            var exam = await _context.Exams
                .Include(e => e.Room)
                .FirstOrDefaultAsync(e => e.ExamId == req.ExamId);

            if (exam == null)
                return BadRequest(new { message = "Kỳ thi không tồn tại." });


            // Kiểm tra trùng
            var exists = await _context.Registers.AnyAsync(r =>
                r.StudentId == req.StudentId && r.ExamId == req.ExamId && r.Status);

            if (exists)
                return Conflict(new { message = "Bạn đã đăng ký kỳ thi này rồi." });

            var register = new Register
            {
                StudentId = req.StudentId,
                ExamId = req.ExamId,
                Status = false,                     // Chưa thanh toán
                payment = "Chưa thanh toán",        // ✅ tránh null
                CreatedAt = DateTime.UtcNow,
                Email = student.Email
            };


            _context.Registers.Add(register);
            await _context.SaveChangesAsync();  // Đây là lúc registerId thật được sinh

            var registerDto = new RegisterDto
            {
                RegisterId = register.RegisterId,
                StudentId = register.StudentId,
                ExamId = register.ExamId,
                SubjectId = register.CourseSubjectId,
                Email = register.Email,
                Status = register.Status,
                Payment = register.payment,
                CreatedAt = register.CreatedAt,
                ExamName = exam.Name,
                ExamFee = exam.Fee,
                RoomName = exam.Room?.Name
            };

            var qrPayload = new
            {
                RegisterId = register.RegisterId,
                StudentId = register.StudentId,
                ExamId = register.ExamId,
                ExamName = exam.Name,
                Amount = exam.Fee,
                Timestamp = DateTime.UtcNow
            };

            return Created("", new { register = registerDto, qr = System.Text.Json.JsonSerializer.Serialize(qrPayload) });
        }


        // POST: api/admin/registers/temp-register
        // POST: api/admin/registers/temp-register




        // GET: api/admin/registers/student/5
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<Register>>> GetRegistersByStudent(int studentId)
        {
            var regs = await _context.Registers
                .Where(r => r.StudentId == studentId && r.Status)
                .Include(r => r.Exam)
                .ThenInclude(e => e.Room)
                .ToListAsync();

            return Ok(regs);
        }

        // GET: api/admin/registers/student/5/exams
        

        // POST: api/admin/registers/{registerId}/create-fake-qrcode
        

        // POST: api/admin/registers/{registerId}/confirm-payment
        // POST: api/admin/registers/{registerId}/confirm-payment
        // POST: api/admin/registers/{registerId}/confirm-payment
        






        // DELETE: api/admin/registers/{id} (soft delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegister(int id)
        {
            var register = await _context.Registers.FindAsync(id);
            if (register == null)
                return NotFound();

            register.Status = false;
            _context.Entry(register).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        // GET: api/admin/registers/student/{studentId}/exam-schedules
        


    }
}
