using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CourseSubjectsController : ControllerBase
	{
		private readonly FlutterContext _context;

		public CourseSubjectsController(FlutterContext context)
		{
			_context = context;
		}
		[HttpGet("byCourse/{courseId}")]
		public async Task<ActionResult<IEnumerable<CourseSubject>>> GetCourseSubjectsByCourse(int courseId)
		{
			var courseSubjects = await _context.CourseSubject
				.Where(cs => cs.CourseId == courseId && cs.Status == true)
				.ToListAsync();

			if (courseSubjects == null || !courseSubjects.Any())
			{
				return NotFound();
			}

			return courseSubjects;
		}
	}
}
