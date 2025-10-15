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
                .Include(e => e.CourseSubject)
                .FirstOrDefaultAsync(e => e.ExamId == req.ExamId);

            if (exam == null)
                return BadRequest(new { message = "Kỳ thi không tồn tại." });

            if (exam.CourseSubject == null)
                return BadRequest(new { message = "Exam này chưa liên kết môn học (Subject)." });

            // Kiểm tra trùng
            var exists = await _context.Registers.AnyAsync(r =>
                r.StudentId == req.StudentId && r.ExamId == req.ExamId && r.Status);

            if (exists)
                return Conflict(new { message = "Bạn đã đăng ký kỳ thi này rồi." });

            var register = new Register
            {
                StudentId = req.StudentId,
                ExamId = req.ExamId,
                SubjectId = exam.CourseSubject.SubjectId,
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
                SubjectId = register.SubjectId,
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
        [HttpGet("student/{studentId}/exams")]
        public async Task<ActionResult<IEnumerable<object>>> GetExamsByStudent(int studentId)
        {
            var exams = await _context.Registers
                .Where(r => r.StudentId == studentId && r.Status)
                .Include(r => r.Exam)
                .ThenInclude(e => e.Room)
                .Select(r => new
                {
                    ExamId = r.Exam.ExamId,
                    ExamName = r.Exam.Name,
                    ExamDay = r.Exam.ExamDay,
                    ExamTime = r.Exam.ExamTime.ToString(@"hh\:mm"),
                    RoomName = r.Exam.Room != null ? r.Exam.Room.Name : null
                })
                .ToListAsync();

            if (!exams.Any())
                return NotFound(new { message = "Không tìm thấy lịch thi nào cho sinh viên này." });

            return Ok(exams);
        }

        // POST: api/admin/registers/{registerId}/create-fake-qrcode
        [HttpPost("{registerId}/create-fake-qrcode")]
        public async Task<ActionResult<object>> CreateFakeQr(int registerId)
        {
            var reg = await _context.Registers
                .Include(r => r.Exam)
                .FirstOrDefaultAsync(r => r.RegisterId == registerId);

            if (reg == null)
                return NotFound();

            var payload = new
            {
                registerId = reg.RegisterId,
                studentId = reg.StudentId,
                examId = reg.ExamId,
                examName = reg.Exam?.Name,
                amount = reg.Exam?.Fee ?? 0,
                timestamp = DateTime.UtcNow
            };

            return Ok(new { qr = System.Text.Json.JsonSerializer.Serialize(payload) });
        }

        // POST: api/admin/registers/{registerId}/confirm-payment
        // POST: api/admin/registers/{registerId}/confirm-payment
        // POST: api/admin/registers/{registerId}/confirm-payment
        [HttpPost("{registerId}/confirm-payment")]
        public async Task<ActionResult<RegisterDto>> ConfirmPayment(int registerId, [FromBody] ConfirmPaymentRequest req)
        {
            var reg = await _context.Registers
                .Include(r => r.Exam)
                .ThenInclude(e => e.Room)
                .FirstOrDefaultAsync(r => r.RegisterId == registerId);

            if (reg == null)
                return NotFound();

            reg.payment = req.method;
            reg.Status = req.success; // true nếu thanh toán thành công
            _context.Entry(reg).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            var dto = new RegisterDto
            {
                RegisterId = reg.RegisterId,
                StudentId = reg.StudentId,
                ExamId = reg.ExamId,
                SubjectId = reg.SubjectId,
                Email = reg.Email,
                Status = reg.Status,
                Payment = reg.payment,
                CreatedAt = reg.CreatedAt,
                ExamName = reg.Exam?.Name,
                ExamFee = reg.Exam?.Fee ?? 0,
                RoomName = reg.Exam?.Room?.Name
            };

            return Ok(dto);
        }






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
        [HttpGet("student/{studentId}/exam-schedules")]
        public async Task<ActionResult<IEnumerable<object>>> GetExamSchedulesByStudent(int studentId)
        {
            var registers = await _context.Registers
                .Where(r => r.StudentId == studentId && r.Status)
                .Include(r => r.Exam)
                    .ThenInclude(e => e.CourseSubject)
                        .ThenInclude(cs => cs.Course)
                .ToListAsync(); // <--- Lấy dữ liệu về trước

            if (!registers.Any())
                return NotFound(new { message = "Không có lịch thi nào cho sinh viên này." });

            // Xử lý format sau khi đã lấy ra khỏi EF Core
            var result = registers.Select(r => new
            {
                CourseName = r.Exam?.CourseSubject?.Course?.Name ?? "Chưa xác định",
                ExamName = r.Exam?.Name ?? "Chưa xác định",
                ExamDay = r.Exam?.ExamDay,
                ExamTime = r.Exam?.ExamTime.ToString(@"hh\:mm") // Giờ xử lý bằng C#
            });

            return Ok(result);
        }


    }
}
