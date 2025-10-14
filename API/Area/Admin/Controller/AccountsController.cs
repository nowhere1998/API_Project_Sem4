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
        public async Task<ActionResult<IEnumerable<object>>> GetAccounts(
    [FromQuery] string? name,
    [FromQuery] int? role,
    [FromQuery] string? status,
    [FromQuery] int? accountId)  // filter theo accountId
        {
            var query = _context.Accounts.AsQueryable();

            // Lọc theo accountId nếu có
            if (accountId.HasValue)
            {
                query = query.Where(acc => acc.AccountId == accountId.Value);
            }

            // Lọc theo name
            if (!string.IsNullOrWhiteSpace(name))
            {
                string lowerName = name.ToLower().Trim();
                query = query.Where(acc =>
                    acc.Name.ToLower().Contains(lowerName) ||
                    acc.Email.ToLower().Contains(lowerName) ||
                    acc.FullName.ToLower().Contains(lowerName)
                );
            }

            // Lọc theo role
            if (role.HasValue)
            {
                query = query.Where(acc => acc.Role == role.Value);
            }

            // Lọc theo status
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (bool.TryParse(status.Trim(), out bool statusBool))
                {
                    query = query.Where(acc => acc.Status == statusBool);
                }
            }

            // Nếu filter theo accountId, trả kèm Room info
            if (accountId.HasValue)
            {
                var result = await query
                    .Include(a => a.Room)
                    .Select(a => new
                    {
                        a.AccountId,
                        a.Name,
                        a.FullName,
                        a.Email,
                        a.Status,
                        Room = a.Room == null ? null : new
                        {
                            a.Room.RoomId,
                            a.Room.Name,
                            a.Room.Status,
                            Students = a.Room.Accounts
                                .Where(s => s.Role == 0 && s.Status) // chỉ lấy sinh viên active
                                .Select(s => new
                                {
                                    s.AccountId,
                                    s.Name,
                                    s.FullName,
                                    s.Email,
                                    s.Status
                                })
                                .ToList()
                        }
                    })
                    .FirstOrDefaultAsync();

                if (result == null)
                    return NotFound();

                return Ok(result);
            }

            // Nếu không filter theo accountId, trả danh sách account bình thường
            var accounts = await query
                .Select(a => new
                {
                    a.AccountId,
                    a.Name,
                    a.FullName,
                    a.Email,
                    a.Status
                })
                .ToListAsync();

            return Ok(accounts);
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
        public async Task<IActionResult> PutAccount(int id, Account account)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != account.AccountId)
            {
                return BadRequest();
            }

            _context.Entry(account).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Accounts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Account>> PostAccount(Account account)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAccount", new { id = account.AccountId }, account);
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
                acc.Password == loginRequest.Password
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
