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
			var student = await _context.Accounts.FindAsync(studentId);
			if (student == null)
				return NotFound(new { message = "Không tìm thấy sinh viên." });

			if (student.Role != 2)
				return BadRequest(new { message = "Chỉ sinh viên mới có danh sách thi lại." });

			var failedSubjects = await _context.AccountExams
				.Include(ae => ae.Exam)
					.ThenInclude(e => e.CourseSubject)
						.ThenInclude(cs => cs.Subject)
				.Include(ae => ae.Exam.CourseSubject)
					.ThenInclude(cs => cs.Course)
				.Include(ae => ae.Exam.Room)
				.Where(ae => ae.StudentId == studentId && !ae.IsPass && ae.Status)
				.Select(ae => new
				{
					SubjectName = ae.Exam.CourseSubject != null && ae.Exam.CourseSubject.Subject != null
						? ae.Exam.CourseSubject.Subject.Name
						: "Không rõ môn",

					CourseName = ae.Exam.CourseSubject != null && ae.Exam.CourseSubject.Course != null
						? ae.Exam.CourseSubject.Course.Name
						: "Không rõ khóa học",

					ExamId = ae.Exam.ExamId,
					ExamName = ae.Exam.Name ?? "Không rõ tên",
					ExamDay = ae.Exam.ExamDay,
					ExamTime = ae.Exam.ExamTime != null
	? ae.Exam.ExamTime.ToString(@"hh\:mm")
	: "Không rõ giờ",

					RoomName = ae.Exam.Room != null ? ae.Exam.Room.Name : "Không rõ phòng",
					Fee = ae.Exam.Fee,
					Score = ae.Score,
					ae.IsPass
				})
				.ToListAsync();

			if (!failedSubjects.Any())
				return NotFound(new { message = "Sinh viên không có môn nào cần thi lại." });

			return Ok(failedSubjects);
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
