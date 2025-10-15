using API.Models;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistersController : ControllerBase
    {
        private readonly FlutterContext _context;

        public RegistersController(FlutterContext context)
        {
            _context = context;
        }

        // =========================
        // 1️⃣ Tạo đăng ký tạm (tạo QR)
        // POST: api/admin/registers
        [HttpPost]
        public async Task<ActionResult<object>> CreateTempRegister([FromBody] TempRegisterRequest req)
        {
            var student = await _context.Accounts.FindAsync(req.StudentId);
            if (student == null || student.Role != 2)
                return BadRequest(new { message = "Chỉ sinh viên (role=2) được đăng ký." });

            var exam = await _context.Exams
                .Include(e => e.Room)
                .Include(e => e.CourseSubject)
                    .ThenInclude(cs => cs.Subject)
                .FirstOrDefaultAsync(e => e.ExamId == req.ExamId);

            if (exam == null)
                return BadRequest(new { message = "Kỳ thi không tồn tại." });

            if (await _context.Registers.AnyAsync(r =>
                r.StudentId == req.StudentId && r.ExamId == req.ExamId && r.Status))
            {
                return Conflict(new { message = "Bạn đã đăng ký kỳ thi này rồi." });
            }

            var register = new Register
            {
                StudentId = req.StudentId,
                ExamId = req.ExamId,
                CourseSubjectId = exam.CourseSubject.CourseSubjectId,
                Email = student.Email,
                Status = false,
                payment = "Chưa thanh toán",
                CreatedAt = DateTime.UtcNow
            };

            _context.Registers.Add(register);
            await _context.SaveChangesAsync();

            // DTO + QR
            var registerDto = new RegisterDto
            {
                RegisterId = register.RegisterId,
                StudentId = register.StudentId,
                ExamId = register.ExamId,
                CourseSubjectId = register.CourseSubjectId,
                SubjectId = exam.CourseSubject.Subject?.SubjectId ?? 0,
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
                register.RegisterId,
                register.StudentId,
                register.ExamId,
                ExamName = exam.Name,
                Amount = exam.Fee,
                Timestamp = DateTime.UtcNow
            };

            // 🔹 Trả về đúng format cho Flutter
            return Created("", new { register = registerDto, qr = qrPayload });
        }




        // =========================
        // 2️⃣ Xác nhận thanh toán
        // POST: api/admin/registers/{registerId}/confirm-payment
        [HttpPost("confirm-payment/{id}")]
        public async Task<IActionResult> ConfirmPayment(int id, [FromBody] ConfirmPaymentRequest request)
        {
            var register = await _context.Registers
                .Include(r => r.Exam)
                    .ThenInclude(e => e.Room)
                .Include(r => r.CourseSubject)
                    .ThenInclude(cs => cs.Subject)
                .FirstOrDefaultAsync(r => r.RegisterId == id);

            if (register == null)
                return NotFound("Register not found");

            if (request.success)
            {
                register.Status = true;
                register.payment = request.method; // gán phương thức thanh toán
                await _context.SaveChangesAsync(); // lưu thay đổi vào DB
            }

            var dto = new RegisterDto
            {
                RegisterId = register.RegisterId,
                StudentId = register.StudentId,
                ExamId = register.ExamId,
                CourseSubjectId = register.CourseSubjectId,
                SubjectId = register.CourseSubject?.Subject?.SubjectId ?? 0, // ✅ đảm bảo không null
                Email = register.Email,
                Status = register.Status,
                Payment = register.payment,
                CreatedAt = register.CreatedAt,
                ExamName = register.Exam?.Name,
                ExamFee = register.Exam?.Fee ?? 0,
                RoomName = register.Exam?.Room?.Name
            };


            return Ok(dto);
        }







        // =========================
        // 3️⃣ Lấy lịch thi theo sinh viên
        // GET: api/admin/registers/student/{studentId}/exam-schedules
        [HttpGet("student/{studentId}/exam-schedules")]
        public async Task<ActionResult<IEnumerable<object>>> GetExamSchedulesByStudent(int studentId)
        {
            var registers = await _context.Registers
                .Where(r => r.StudentId == studentId && r.Status)
                .Include(r => r.Exam)
                    .ThenInclude(e => e.CourseSubject)
                        .ThenInclude(cs => cs.Course)
                .Include(r => r.Exam) // include Room
                    .ThenInclude(e => e.Room)
                .ToListAsync();

            if (!registers.Any())
                return NotFound(new { message = "Không có lịch thi nào." });

            var result = registers.Select(r => new
            {
                CourseName = r.Exam?.CourseSubject?.Course?.Name ?? "Chưa xác định",
                ExamName = r.Exam?.Name,
                ExamDay = r.Exam?.ExamDay,
                ExamTime = r.Exam?.ExamTime.ToString(@"hh\:mm"),
                RoomName = r.Exam?.Room?.Name ?? "Chưa có"
            });

            return Ok(result);
        }


        // =========================
        // 4️⃣ Lấy danh sách thi lại (chưa pass)
        // GET: api/admin/registers/retake/{studentId}
        [HttpGet("retake/{studentId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetRetakeExams(int studentId)
        {
            var retakes = await _context.AccountExams
                .Where(ae => ae.StudentId == studentId && !ae.IsPass)
                .Include(ae => ae.Exam)
                .Include(ae => ae.CourseSubject)
                .ThenInclude(cs => cs.Subject)
                .Select(ae => new
                {
                    ae.ExamId,
                    ExamName = ae.Exam.Name,
                    SubjectName = ae.CourseSubject.Subject.Name,
                    ae.Score
                })
                .ToListAsync();

            if (!retakes.Any())
                return NotFound(new { message = "Không có môn thi lại." });

            return Ok(retakes);
        }
    }

}
