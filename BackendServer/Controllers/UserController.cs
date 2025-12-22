using MapsterMapper;
using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace DeskBookingService.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<User> _validator;

        public UserController(AppDbContext context, IMapper mapper, IValidator<User> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }

        /// <summary>
        /// Retrieves all users from the database
        /// </summary>
        /// <returns>List of user DTOs</returns>
        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                var userDtos = _mapper.Map<List<UserDto>>(users);
                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        /// <summary>
        /// Adds a new user to the database
        /// </summary>
        /// <param name="userDto">User data transfer object containing user information</param>
        /// <returns>Created user object or error message</returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddUser([FromBody] UserDto userDto)
        {
            try
            {
                var user = _mapper.Map<User>(userDto);

                // Validate using FluentValidation
                var validationResult = await _validator.ValidateAsync(user);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { error = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)) });
                }

                // Check if user already exists
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    return BadRequest(new { error = "User with this email already exists." });
                }

                // Generate ID if not provided
                if (string.IsNullOrEmpty(user.Id))
                {
                    user.Id = Guid.NewGuid().ToString();
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(user);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        /// <summary>
        /// Deletes a user from the database by ID
        /// </summary>
        /// <param name="id">User ID to delete</param>
        /// <returns>Success or error message</returns>
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

        /// <summary>
        /// Updates an existing user's information
        /// </summary>
        /// <param name="id">User ID to update</param>
        /// <param name="userDto">Updated user data</param>
        /// <returns>Success or error message</returns>
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserDto userDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound($"User with ID: {id}, not found");
                }

                // Manually update properties (don't update Id as it's a key)
                user.Name = userDto.Name;
                user.Surname = userDto.Surname;
                user.Email = userDto.Email;
                user.Password = userDto.Password;
                user.Role = userDto.Role;

                // Validate using FluentValidation
                var validationResult = await _validator.ValidateAsync(user);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { error = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)) });
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        /// <summary>
        /// Registers a new user account
        /// </summary>
        /// <param name="userDto">User registration data</param>
        /// <returns>Created user DTO or error message</returns>
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

        /// <summary>
        /// Authenticates a user and returns their information
        /// </summary>
        /// <param name="loginDto">Login credentials (email and password)</param>
        /// <returns>User DTO if successful, error message otherwise</returns>
        [HttpPost("login-user")]
        public async Task<IActionResult> LoginUser([FromBody] LoginDto loginDto)
        {
            try
            {
                // Check if email is valid & registered to a user
                if (!IsValidEmail(loginDto.Email))
                    return BadRequest("No valid email provided");
                if (!await IsRegisteredEmail(loginDto.Email))
                {
                    return BadRequest("Email is not registered");
                }
                // Get the user by credentials by checking if password matches the one with specified email
                // Since no authentification is needed as a requirement, just check plaintext password
                // In production environment ensure proper security/authentification measures
                var loggedInUser = await GetUserByCredentials(loginDto.Email, loginDto.Password);

                if (loggedInUser == null)
                {
                    return BadRequest("Invalid email or password");
                }
                return Ok(loggedInUser);
            }
            catch (Exception ex)
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

