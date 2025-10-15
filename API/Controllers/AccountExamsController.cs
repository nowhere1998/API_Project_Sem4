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
            // Lấy tất cả AccountExam của sinh viên, bao gồm Exam, Student và Room
            var accountExams = await _context.AccountExams
                .Where(ae => ae.StudentId == studentId && ae.Status)
                .Include(ae => ae.Student)
                    .ThenInclude(s => s.Room)
                .Include(ae => ae.Exam)
                .Include(ae => ae.CourseSubject)
                    .ThenInclude(cs => cs.Course)
                .Include(ae => ae.CourseSubject)
                    .ThenInclude(cs => cs.Subject)
                .ToListAsync();

            if (!accountExams.Any())
                return NotFound($"Không tìm thấy điểm thi cho sinh viên ID: {studentId}");

            // Map sang DTO
            var result = accountExams.Select(ae => new AccountExamDto
            {
                AccountExamId = ae.AccountExamId,
                ExamId = ae.ExamId,
                ExamName = ae.Exam?.Name ?? "",
                StudentId = ae.StudentId,
                StudentName = ae.Student?.Name ?? "",
                Score = ae.Score,
                Record = ae.Record,
                IsPass = ae.IsPass,
                Status = ae.Status,
                RoomName = ae.Student?.Room?.Name ?? "",
            }).ToList();

            return Ok(result);
        }



    }
}
