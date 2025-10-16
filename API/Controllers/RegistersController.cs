using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
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
        [HttpPost]
        public async Task<ActionResult<object>> CreateTempRegister([FromBody] TempRegisterRequest req)
        {
            // 1️⃣ Kiểm tra sinh viên
            var student = await _context.Accounts.FindAsync(req.StudentId);
            if (student == null || student.Role != 2)
                return BadRequest(new { message = "Chỉ sinh viên (role=2) được đăng ký." });

            // 2️⃣ Lấy kỳ thi và phòng thi
            var exam = await _context.Exams
                .Include(e => e.Room)
                .FirstOrDefaultAsync(e => e.ExamId == req.ExamId);

            if (exam == null)
                return BadRequest(new { message = "Kỳ thi không tồn tại." });

            // 3️⃣ Lấy CourseSubject của sinh viên qua AccountId
            var accountExam = await _context.AccountExams
                .Include(ae => ae.CourseSubject)
                    .ThenInclude(cs => cs.Course)
                .Include(ae => ae.CourseSubject)
                    .ThenInclude(cs => cs.Subject)
                .FirstOrDefaultAsync(ae => ae.StudentId == req.StudentId && ae.ExamId == req.ExamId);

            if (accountExam == null || accountExam.CourseSubject == null)
                return BadRequest(new { message = "Không tìm thấy thông tin môn học hoặc khóa học của sinh viên." });

            var courseSubject = accountExam.CourseSubject;

            // 4️⃣ Kiểm tra số lượng đăng ký hợp lệ
            var currentRegisterCount = await _context.Registers
                .CountAsync(r => r.ExamId == req.ExamId && r.Status);

            const int maxRegister = 30;
            if (currentRegisterCount >= maxRegister)
                return BadRequest(new { message = "Kỳ thi này đã đủ người, hẹn bạn tháng sau." });

            // 5️⃣ Kiểm tra trùng lịch với các môn đã đăng ký thành công
            var existingRegisters = await _context.Registers
                .Where(r => r.StudentId == req.StudentId && r.Status)
                .Include(r => r.Exam)
                .ToListAsync();

            bool conflict = existingRegisters.Any(r =>
                r.Exam.ExamDay == exam.ExamDay &&
                r.Exam.ExamTime == exam.ExamTime
            );

            if (conflict)
            {
                return BadRequest(new
                {
                    message = "Môn bạn đăng ký bị trùng thời gian với một kỳ thi khác đã đăng ký."
                });
            }

            // 6️⃣ Chuẩn bị dữ liệu tạm
            var tempRegister = new
            {
                StudentId = student.AccountId,
                ExamId = exam.ExamId,
                CourseName = courseSubject.Course?.Name ?? "Chưa rõ khóa học",
                SubjectName = courseSubject.Subject?.Name ?? "Chưa rõ môn học",
                Email = student.Email,
                ExamName = exam.Name,
                ExamFee = exam.Fee,
                RoomName = exam.Room?.Name ?? "Chưa rõ phòng",
                PaymentStatus = "Chưa thanh toán",
                Status = false // Chưa thanh toán,
            };

            // 7️⃣ Payload QR
            var qrPayload = new
            {
                StudentId = student.AccountId,
                ExamId = exam.ExamId,
                ExamName = exam.Name ?? "Không rõ",
                Amount = exam.Fee,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            // 8️⃣ Trả dữ liệu
            return Ok(new
            {
                register = tempRegister,
                qr = qrPayload,
                currentCount = currentRegisterCount,
                maxCount = maxRegister
            });
        }







        // =========================
        // 2️⃣ Xác nhận thanh toán
        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            // 1️⃣ Lấy sinh viên
            var student = await _context.Accounts.FindAsync(request.StudentId);
            if (student == null)
                return BadRequest(new { message = "Không tìm thấy sinh viên." });

            // 2️⃣ Lấy kỳ thi và phòng thi
            var exam = await _context.Exams
                .Include(e => e.Room)
                .FirstOrDefaultAsync(e => e.ExamId == request.ExamId);

            if (exam == null)
                return BadRequest(new { message = "Không tìm thấy kỳ thi." });

            if (!request.success)
                return BadRequest(new { message = "Thanh toán thất bại." });

            // 3️⃣ Kiểm tra xem sinh viên đã đăng ký hay chưa
            var existingRegister = await _context.Registers
                .FirstOrDefaultAsync(r => r.StudentId == request.StudentId && r.ExamId == request.ExamId && r.Status);

            if (existingRegister != null)
                return Conflict(new { message = "Bạn đã đăng ký kỳ thi này rồi." });

            // 4️⃣ Lấy AccountExam và CourseSubject của sinh viên theo ExamId
            var accountExam = await _context.AccountExams
                .Include(ae => ae.CourseSubject)
                    .ThenInclude(cs => cs.Course)
                .Include(ae => ae.CourseSubject)
                    .ThenInclude(cs => cs.Subject)
                .FirstOrDefaultAsync(ae => ae.StudentId == request.StudentId && ae.ExamId == request.ExamId);

            if (accountExam == null || accountExam.CourseSubject == null)
                return BadRequest(new { message = "Không tìm thấy thông tin môn học hoặc khóa học của sinh viên." });

            var courseSubject = accountExam.CourseSubject;

            // 5️⃣ Tạo bản ghi Register mới
            var register = new Register
            {
                StudentId = student.AccountId,
                ExamId = exam.ExamId,
                CourseSubjectId = courseSubject.CourseSubjectId,
                Email = student.Email,
                Status = true,
                payment = request.method,
                CreatedAt = DateTime.UtcNow
            };

            _context.Registers.Add(register);
            await _context.SaveChangesAsync();

            // 6️⃣ Trả dữ liệu cho Flutter
            return Ok(new
            {
                register.RegisterId,
                register.StudentId,
                register.ExamId,
                register.CourseSubjectId,
                Email = register.Email,
                Payment = register.payment,
                ExamName = exam.Name,
                ExamFee = exam.Fee,
                RoomName = exam.Room?.Name ?? "Chưa rõ phòng",
                SubjectName = courseSubject.Subject?.Name ?? "Chưa rõ môn học",
                CourseName = courseSubject.Course?.Name ?? "Chưa rõ khóa học"
            });
        }





        // =========================
        // 3️⃣ Lấy lịch thi của sinh viên
        [HttpGet("student/{studentId}/exam-schedules")]
        public async Task<ActionResult<IEnumerable<object>>> GetExamSchedulesByStudent(int studentId)
        {
            var registers = await _context.Registers
                .Where(r => r.StudentId == studentId && r.Status)
                .Include(r => r.Exam)
                    .ThenInclude(e => e.Room)
                .Include(r => r.CourseSubject)
                    .ThenInclude(cs => cs.Course)
                .Include(r => r.CourseSubject)
                    .ThenInclude(cs => cs.Subject)
                .ToListAsync();

            if (!registers.Any())
                return NotFound(new { message = "Không có lịch thi nào." });

            var result = registers.Select(r => new
            {
                CourseName = r.CourseSubject?.Course?.Name ?? "Chưa xác định",
                SubjectName = r.CourseSubject?.Subject?.Name ?? "Chưa xác định",
                ExamName = r.Exam?.Name ?? "Chưa xác định",
                ExamDay = r.Exam != null ? r.Exam.ExamDay.ToString("yyyy-MM-dd") : "Chưa xác định",
                ExamTime = r.Exam != null ? r.Exam.ExamTime.ToString(@"hh\:mm") : "Chưa xác định",
                RoomName = r.Exam?.Room?.Name ?? "Chưa rõ phòng"
            });

            return Ok(result);
        }



        // =========================
        // 4️⃣ Danh sách thi lại (fail)
        [HttpGet("retake/{studentId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetRetakeExams(int studentId)
        {
            var retakes = await _context.AccountExams
                .Where(ae => ae.StudentId == studentId && !ae.IsPass)
                .Include(ae => ae.Exam)
                    .ThenInclude(e => e.Room)
                .Include(ae => ae.CourseSubject)
                    .ThenInclude(cs => cs.Subject)
                .Include(ae => ae.CourseSubject)
                    .ThenInclude(cs => cs.Course)
                .Select(ae => new
                {
                    ae.ExamId,
                    ExamName = ae.Exam.Name,
                    SubjectName = ae.CourseSubject.Subject.Name,
                    CourseName = ae.CourseSubject.Course.Name,
                    RoomName = ae.Exam.Room.Name,
                    ae.Score,
                    ae.IsPass
                })
                .ToListAsync();

            if (!retakes.Any())
                return NotFound(new { message = "Không có môn thi lại." });

            return Ok(retakes);
        }
    }
}
