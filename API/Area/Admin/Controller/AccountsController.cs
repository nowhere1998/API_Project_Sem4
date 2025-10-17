using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Area.Admin.Controller
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly FlutterContext _context;

        public AccountsController(FlutterContext context)
        {
            _context = context;
        }

        // GET: api/Accounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccounts([FromQuery] string? name, int? role, string? status)
        {
            var query = _context.Accounts.AsQueryable();
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(acc =>
                    (
                        acc.Name.ToLower().Contains(name.ToLower().Trim()) ||
                        acc.Email.ToLower().Contains(name.ToLower().Trim()) ||
                        acc.FullName.ToLower().Contains(name.ToLower().Trim())
                    )
                );
            }
            if (role.HasValue)
            {
                query = query.Where(acc => acc.Role == role);
            }
            if (!string.IsNullOrWhiteSpace(status) && ( status.ToLower().Trim().Equals("true") || status.ToLower().Trim().Equals("false")))
            {
                query = query.Where(cs => cs.Status == bool.Parse(status.ToLower().Trim()));
            }
            return await query.ToListAsync();
        }



        // GET: api/Accounts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> GetAccount(int id)
        {
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return NotFound();
            }

            return account;
        }

        // PUT: api/Accounts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAccount(
        int id,
        [FromForm] Account account,      // Bind từ form-data
        IFormFile? image,                // File upload mới
        [FromForm] string? oldImage = "" // Nếu không upload file mới
)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != account.AccountId)
                return BadRequest("ID không khớp");

            try
            {
                // 1️⃣ Lấy account từ DB
                var accountFromDb = await _context.Accounts.FindAsync(id);
                if (accountFromDb == null)
                    return NotFound();

                // 2️⃣ Cập nhật các field cần thay đổi
                accountFromDb.Name = account.Name;
                accountFromDb.FullName = account.FullName;
                accountFromDb.RoomId = account.RoomId;
                accountFromDb.Email = account.Email;
                accountFromDb.Phone = account.Phone ?? "";
                accountFromDb.Address = account.Address;
                accountFromDb.DateOfBirth = account.DateOfBirth;
                accountFromDb.Role = account.Role;
                accountFromDb.Status = account.Status; // checkbox status
                accountFromDb.Password = account.Password; // hoặc hash nếu cần

                // 3️⃣ Xử lý file ảnh
                if (image != null && image.Length > 0)
                {
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "images");
                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    accountFromDb.Image = "/assets/images/" + fileName;
                }
                else
                {
                    accountFromDb.Image = oldImage; // giữ ảnh cũ
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Account updated successfully", account = accountFromDb });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }
        }




        // POST: api/Accounts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Account>> PostAccount([FromForm] Account account, IFormFile? image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // ✅ 1. Nếu có upload ảnh
                if (image != null && image.Length > 0)
                {
                    // Tạo đường dẫn thư mục "wwwroot/assets/images"
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "images");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    // Tạo tên file duy nhất
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploadDir, fileName);

                    // Lưu file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    // ✅ Lưu đường dẫn tương đối vào DB
                    account.Image = "/assets/images/" + fileName;
                }

                // ✅ 2. Lưu tài khoản vào DB
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetAccount", new { id = account.AccountId }, account);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }
        }


        // DELETE: api/Accounts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            account.Status = false;
            _context.Entry(account).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //POST: api/Accounts/login
        [HttpPost("login")]
        public async Task<ActionResult<Account>> Login([FromBody] LoginRequest loginRequest)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(acc =>
                    acc.Email == loginRequest.Email &&
                acc.Password == Cipher.GenerateMD5(loginRequest.Password)
				);

            if (account == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            return Ok(account);
        }

        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.AccountId == id);
        }
    }
}
