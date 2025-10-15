using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountExamsController : ControllerBase
	{
		private readonly FlutterContext _context;

		public AccountExamsController(FlutterContext context)
		{
			_context = context;
		}
		[HttpGet("byStudent/{studentId}")]
		public async Task<ActionResult<IEnumerable<AccountExamDto>>> GetAccountExamsByStudent(int studentId)
		{
			var exams = await _context.AccountExams
				.Where(ae => ae.StudentId == studentId)
				.Include(ae => ae.Student)
					.ThenInclude(s => s.Room) // lấy Room của Student
				.Include(ae => ae.Exam)
				.Select(ae => new AccountExamDto
				{
					AccountExamId = ae.AccountExamId,
					ExamId = ae.ExamId,
					ExamName = ae.Exam.Name,
					StudentId = ae.StudentId,
					StudentName = ae.Student.Name,
					Score = ae.Score,
					IsPass = ae.IsPass,
					Record = ae.Record,
					RoomName = ae.Student.Room.Name // điền tên lớp
				})
				.ToListAsync();

			if (!exams.Any())
				return NotFound($"Không tìm thấy điểm thi cho sinh viên ID: {studentId}");

			return exams;
		}



	}
}
