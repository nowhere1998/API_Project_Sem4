using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Models
{
    public class FlutterContext:DbContext
    {
        public FlutterContext(DbContextOptions options) : base(options) 
        {
            
        }
        public DbSet<AccountExam> AccountExams { get; set; } = default!;
        public DbSet<Account> Accounts { get; set; } = default!;
        public DbSet<Exam> Exams { get; set; } = default!;
        public DbSet<Register> Registers { get; set; } = default!;
        public DbSet<Room> Rooms { get; set; } = default!;
        public DbSet<Subject> Subjects { get; set; } = default!;
    }
}
