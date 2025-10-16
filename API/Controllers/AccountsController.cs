using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountsController : ControllerBase
	{
		private readonly FlutterContext _context;

		public AccountsController(FlutterContext context)
		{
			_context = context;
		}
        [HttpGet("retake/{studentId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetRetakeExams(int studentId)
        {
            // Lấy sinh viên
            var student = await _context.Accounts.FindAsync(studentId);
            if (student == null)
                return NotFound(new { message = "Không tìm thấy sinh viên." });

            if (student.Role != 2) // 0 = sinh viên
                return BadRequest(new { message = "Chỉ sinh viên mới có danh sách thi lại." });

            // Lấy danh sách môn thi lại (IsPass = false)
            var failedExams = await _context.AccountExams
                .Where(ae => ae.StudentId == studentId && !ae.IsPass && ae.Status)
                .Include(ae => ae.Exam)
                    .ThenInclude(e => e.Room)
                .Include(ae => ae.CourseSubject)
                    .ThenInclude(cs => cs.Course)
                .Include(ae => ae.CourseSubject)
                    .ThenInclude(cs => cs.Subject)
                .Select(ae => new
                {
                    // Thông tin môn học
                    SubjectName = ae.CourseSubject != null && ae.CourseSubject.Subject != null
                        ? ae.CourseSubject.Subject.Name
                        : "Không rõ môn",
                    CourseName = ae.CourseSubject != null && ae.CourseSubject.Course != null
                        ? ae.CourseSubject.Course.Name
                        : "Không rõ khóa",

                    // Thông tin kỳ thi
                    ExamId = ae.Exam != null ? ae.Exam.ExamId : 0,
                    ExamName = ae.Exam != null ? ae.Exam.Name : "Không rõ tên",
                    ExamDay = ae.Exam != null ? ae.Exam.ExamDay : (DateTime?)null,
                    ExamTime = ae.Exam != null ? ae.Exam.ExamTime.ToString(@"hh\:mm") : "Không rõ giờ",
                    RoomName = ae.Exam != null && ae.Exam.Room != null
                        ? ae.Exam.Room.Name
                        : "Không rõ phòng",

                    // Thông tin khác
                    Score = (float)ae.Score, // Ép kiểu từ double sang float
                    Fee = ae.Exam != null ? (float)ae.Exam.Fee : 0f, // Ép kiểu từ double sang float
                    ae.IsPass
                })
                .ToListAsync();

            if (!failedExams.Any())
                return NotFound(new { message = "Sinh viên không có môn nào cần thi lại." });

            return Ok(failedExams);
        }







        [HttpGet("studentsByAccount/{accountId}")]
		public async Task<ActionResult<object>> GetStudentsByAccount(int accountId)
		{
			// Lấy account để biết RoomId
			var account = await _context.Accounts
				.FirstOrDefaultAsync(a => a.AccountId == accountId);

			if (account == null)
				return NotFound($"AccountId={accountId} không tồn tại.");

			// Lấy tất cả account trong Room
			var allAccountsInRoom = await _context.Accounts
				.Where(a => a.RoomId == account.RoomId)
				.ToListAsync();

			// Filter sinh viên active
			var students = allAccountsInRoom
				.Where(a => a.Role == 2 &&
							(a.Status == true || a.Status == true)) // nếu Status kiểu int hoặc bool
				.Select(a => new
				{
					a.AccountId,
					a.Name,
					a.FullName,
					a.Email,
					a.Status
				})
				.ToList();

			// Lấy thông tin Room
			var room = await _context.Rooms
				.Where(r => r.RoomId == account.RoomId)
				.Select(r => new { r.RoomId, r.Name })
				.FirstOrDefaultAsync();

			return new
			{
				Room = room,
				Students = students
			};
		}
	}
}
