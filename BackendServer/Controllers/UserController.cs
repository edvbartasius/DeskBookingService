using MapsterMapper;
using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DeskBookingService.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public UserController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            var userDtos = _mapper.Map<List<UserDto>>(users);
            return Ok(userDtos);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var entry = await _context.Users.FindAsync(id);
                if (entry == null)
                {
                    return NotFound();
                }
                _context.Users.Remove(entry);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        [HttpPost("register-user")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            try
            {
                var user = _mapper.Map<User>(userDto);
                if (UserExists(user.Email))
                {
                    return Conflict("User with this email already exists.");
                }
                if (!IsValidPassword(user.Password))
                {
                    return BadRequest("Password must be at least 8 characters long and contain at least one digit.");
                }
                if (!IsValidEmail(user.Email))
                {
                    return BadRequest("Invalid email format.");
                }
                user.Id = Guid.NewGuid().ToString();
                user.Role = UserRole.User;
                // Since no authentification is needed, store plaintext password
                // In production, use proper password encryption and security measures
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(_mapper.Map<UserDto>(user));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        [HttpPost("login-user")]
        public async Task<IActionResult> LoginUser([FromBody] UserDto userDto)
        {
            try
            {
                // Check if email is valid & registered to a user
                if (!IsValidEmail(userDto.Email))
                    return BadRequest("No valid email provided");
                if (!await IsRegisteredEmail(userDto.Email))
                {
                    return BadRequest("Email is not registered");
                }
                // Get the user by credentials by checking if password matches the one with specified email
                // Since no authentification is needed as a requirement, just check plaintext password
                // In production environment ensure proper security/authentification measures
                var loggedInUser = await GetUserByCredentials(userDto.Email, userDto.Password);

                if (loggedInUser == null)
                {
                    return BadRequest("Invalid email or password");
                }
                return Ok(loggedInUser);
            } catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        /// <summary>
        /// Checks if a user with the specified email already exists.
        /// </summary>
        /// <param name="email">User email to check</param>
        /// <returns>True if user exists</returns>
        private bool UserExists(string email)
        {
            return _context.Users.Any(e => e.Email == email);
        }

        /// <summary>
        /// Checks if the password has at least 8 characters and contains at least one digit
        /// </summary>
        /// <param name="password">String password to check</param>
        /// <returns>True if password is valid</returns>
        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < 8)
                return false;
            if (!password.Any(char.IsDigit))
                return false;
            return true;
        }

        /// <summary>
        /// Validates the email format
        /// </summary>
        /// <param name="email">Email string to validate</param>
        /// <returns>True if email is valid</returns>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if a provided email adress is registered to a user
        /// </summary>
        /// <param name="email">Email string to check</param>
        /// <returns>True if email is already registered</returns>
        private async Task<bool> IsRegisteredEmail(string email)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.Email == email);
            } 
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Get user by checking email and password matching
        /// </summary>
        /// <param name="email">User email string</param>
        /// <param name="password">User password string</param>
        /// <returns>Returns UserDTO</returns>
        private async Task<UserDto?> GetUserByCredentials(string email, string password)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
                if (user == null)
                    return null;

                return _mapper.Map<UserDto>(user);
            }
            catch
            {
                return null;
            }
        }
    }
}

