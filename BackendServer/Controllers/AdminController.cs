using DeskBookingService.Models;
using DeskBookingService.Models.AdminDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MapsterMapper;
using System.Text.Json;
using System.Diagnostics;


namespace DeskBookingService.Controllers
{
    [ApiController]
    [Route("api/admin")]
    // Admin controller to manage admin-specific endpoints for DB CRUD operations
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(allowIntegerValues: true) },
            RespectRequiredConstructorParameters = false
        };

        public AdminController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Generic endpoint to get data from specified table
        [HttpGet("{tableName}")]
        public async Task<IActionResult> GetTableData(string tableName)
        {
            try
            {
                switch (tableName.ToLower())
                {
                    case "users":
                        return await GetUsers();
                    case "buildings":
                        return await GetBuildings();
                    case "desks":
                        return await GetDesks();
                    case "reservations":
                        return await GetReservations();
                    default:
                        return BadRequest("Invalid table name");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        // Generic endpoint to add record to specified table
        [HttpPost("{tableName}")]
        public async Task<IActionResult> AddRecord(string tableName, [FromBody] JsonElement data)
        {
            Console.WriteLine($"AddRecord called for table: {tableName} with data: {data}");
            try
            {
                // Validate before deserialization
                var validationError = await ValidateData(tableName, data, isUpdate: false);
                if (validationError != null)
                {
                    return BadRequest(new { error = validationError });
                }

                object? entity = DeserializeEntityByTableName(tableName, data);

                if (entity == null)
                {
                    return BadRequest(new { error = "Invalid table name or data format" });
                }

                // Add to context
                _context.Add(entity);
                await _context.SaveChangesAsync();
                return Ok(entity);
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { error = "Database constraint violation: " + ex.InnerException?.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, new { error = "An error occurred while processing your request: " + ex.Message });
            }
        }

        // Generic endpoint to update record in specified table
        [HttpPut("{tableName}/{id}")]
        public async Task<IActionResult> UpdateRecord(string tableName, string id, [FromBody] JsonElement data)
        {
            Console.WriteLine($"UpdateRecord called for table: {tableName} with id: {id} and data: {data}");
            try
            {
                // Validate before updating
                var validationError = await ValidateData(tableName, data, isUpdate: true, existingId: id);
                if (validationError != null)
                {
                    return BadRequest(new { error = validationError });
                }

                // Find the existing entity in the database
                object? existingEntity = await FindEntityByIdAsync(tableName, id);

                if (existingEntity == null)
                {
                    return NotFound(new { error = "Record not found" });
                }

                // Update the entity's properties from JSON data
                UpdateEntityFromJson(existingEntity, data);

                // Save changes
                await _context.SaveChangesAsync();
                return Ok(existingEntity);
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { error = "Database constraint violation: " + ex.InnerException?.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, new { error = "An error occurred while processing your request: " + ex.Message });
            }
        }

        // Generic endpoint to delete record from specified table
        [HttpDelete("{tableName}/{id}")]
        public async Task<IActionResult> DeleteRecord(string tableName, string id)
        {
            Console.WriteLine($"DeleteRecord called for table: {tableName} with id: {id}");
            try
            {
                object? existingEntity = await FindEntityByIdAsync(tableName, id);

                if (existingEntity == null)
                {
                    return NotFound("Record not found");
                }

                _context.Remove(existingEntity);
                await _context.SaveChangesAsync();
                return Ok("Record deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        private static object? DeserializeEntityByTableName(string tableName, JsonElement data)
        {
            return tableName.ToLower() switch
            {
                "users" => data.Deserialize<User>(JsonSerializerOptions),
                "buildings" => data.Deserialize<Building>(JsonSerializerOptions),
                "desks" => data.Deserialize<Desk>(JsonSerializerOptions),
                "reservations" => data.Deserialize<Reservation>(JsonSerializerOptions),
                _ => null
            };
        }

        private async Task<object?> FindEntityByIdAsync(string tableName, string id)
        {
            return tableName.ToLower() switch
            {
                "users" => await _context.Users.FirstOrDefaultAsync(u => u.Id == id),
                "buildings" => int.TryParse(id, out int buildingId) ? await _context.Buildings.FirstOrDefaultAsync(b => b.Id == buildingId) : null,
                "desks" => int.TryParse(id, out int deskId) ? await _context.Desks.FirstOrDefaultAsync(d => d.Id == deskId) : null,
                "reservations" => int.TryParse(id, out int reservationId) ? await _context.Reservations.FirstOrDefaultAsync(r => r.Id == reservationId) : null,
                _ => null
            };
        }

        private static void UpdateEntityFromJson(object existingEntity, JsonElement data)
        {
            var entityType = existingEntity.GetType();

            // Deserialize JSON to a new entity of the same type
            var updatedEntity = data.Deserialize(entityType, JsonSerializerOptions);
            if (updatedEntity == null) return;

            // Copy all properties except Id from the deserialized entity to the existing entity
            foreach (var property in entityType.GetProperties())
            {
                if (property.Name != "Id" && property.CanWrite)
                {
                    var newValue = property.GetValue(updatedEntity);
                    property.SetValue(existingEntity, newValue);
                }
            }
        }

        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                var userDtos = _mapper.Map<List<AdminUserDto>>(users);
                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        [HttpGet("get-buildings")]
        public async Task<IActionResult> GetBuildings()
        {
            try
            {
                var buildings = await _context.Buildings.ToListAsync();
                var buildingDtos = _mapper.Map<List<AdminBuildingDto>>(buildings);
                return Ok(buildingDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        [HttpGet("get-desks")]
        public async Task<IActionResult> GetDesks()
        {
            try
            {
                var desks = await _context.Desks
                    .Include(d => d.Building)
                    .ToListAsync();
                var deskDtos = _mapper.Map<List<AdminDeskDto>>(desks);
                return Ok(deskDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }
        [HttpGet("get-reservations")]
        public async Task<IActionResult> GetReservations()
        {
            try
            {
                var reservations = await _context.Reservations.ToListAsync();
                var reservationDtos = _mapper.Map<List<AdminReservationDto>>(reservations);
                return Ok(reservationDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        // Validation Methods
        private async Task<string?> ValidateData(string tableName, JsonElement data, bool isUpdate = false, string? existingId = null)
        {
            return tableName.ToLower() switch
            {
                "reservations" => await ValidateReservation(data, isUpdate, existingId),
                "users" => await ValidateUser(data, isUpdate, existingId),
                "desks" => await ValidateDesk(data, isUpdate, existingId),
                "buildings" => ValidateBuilding(data),
                _ => null
            };
        }

        private async Task<string?> ValidateReservation(JsonElement data, bool isUpdate, string? existingId)
        {
            // Check required fields
            if (!data.TryGetProperty("userId", out var userIdProp))
                return "User ID is required";
            if (!data.TryGetProperty("deskId", out var deskIdProp))
                return "Desk ID is required";
            if (!data.TryGetProperty("reservationDate", out var reservationDateProp))
                return "Reservation date is required";
            if (!data.TryGetProperty("startDate", out var startDateProp))
                return "Start time is required";
            if (!data.TryGetProperty("endDate", out var endDateProp))
                return "End time is required";

            var userId = userIdProp.GetString();
            if (string.IsNullOrWhiteSpace(userId))
                return "User ID cannot be empty";

            int deskId;
            if (deskIdProp.ValueKind == JsonValueKind.Number)
            {
                deskId = deskIdProp.GetInt32();
            }
            else if (!int.TryParse(deskIdProp.GetString(), out deskId))
            {
                return "Desk ID must be a valid number";
            }

            var reservationDate = DateOnly.Parse(reservationDateProp.GetString()!);
            var startTime = TimeOnly.Parse(startDateProp.GetString()!);
            var endTime = TimeOnly.Parse(endDateProp.GetString()!);

            // Validate time logic
            if (endTime <= startTime)
                return "End time must be after start time";

            // Validate reservation date is not in the past
            if (reservationDate < DateOnly.FromDateTime(DateTime.Now))
                return "Reservation date cannot be in the past";

            // Check if user exists
            if (!await _context.Users.AnyAsync(u => u.Id == userId))
                return "User does not exist";

            // Check if desk exists
            if (!await _context.Desks.AnyAsync(d => d.Id == deskId))
                return "Desk does not exist";

            // TODO: check for reservation conflicts
            return null; // Valid
        }

        private async Task<string?> ValidateUser(JsonElement data, bool isUpdate, string? existingId)
        {
            // Check required fields
            if (!data.TryGetProperty("name", out var nameProp))
                return "Name is required";
            if (!data.TryGetProperty("surname", out var surnameProp))
                return "Surname is required";
            if (!data.TryGetProperty("email", out var emailProp))
                return "Email is required";
            if (!data.TryGetProperty("password", out var passwordProp))
                return "Password is required";

            var name = nameProp.GetString();
            var surname = surnameProp.GetString();
            var email = emailProp.GetString();
            var password = passwordProp.GetString();

            // Validate not empty
            if (string.IsNullOrWhiteSpace(name))
                return "Name cannot be empty";
            if (string.IsNullOrWhiteSpace(surname))
                return "Surname cannot be empty";
            if (string.IsNullOrWhiteSpace(email))
                return "Email cannot be empty";
            if (string.IsNullOrWhiteSpace(password))
                return "Password cannot be empty";

            // Validate email format
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email)
                return "Invalid email format";

            // Validate password length (minimum 6 characters)
            if (password.Length < 6)
                return "Password must be at least 6 characters long";

            // Check for duplicate email
            var emailExistsQuery = _context.Users.Where(u => u.Email == email);

            // If updating, exclude the current user from duplicate check
            if (isUpdate && existingId != null)
            {
                emailExistsQuery = emailExistsQuery.Where(u => u.Id != existingId);
            }

            if (await emailExistsQuery.AnyAsync())
                return "Email already exists";

            return null; // Valid
        }

        private async Task<string?> ValidateDesk(JsonElement data, bool isUpdate = false, string? existingId = null)
        {
            // Check required fields
            if (!data.TryGetProperty("buildingId", out var buildingIdProp))
                return "Building ID is required";

            int buildingId;
            if (buildingIdProp.ValueKind == JsonValueKind.Number)
            {
                buildingId = buildingIdProp.GetInt32();
            }
            else if (!int.TryParse(buildingIdProp.GetString(), out buildingId))
            {
                return "Building ID must be a valid number";
            }

            // Check if building exists
            if (!await _context.Buildings.AnyAsync(b => b.Id == buildingId))
                return "Building does not exist";

            // Description is optional, no validation needed

            return null; // Valid
        }

        private static string? ValidateBuilding(JsonElement data)
        {
            // Check required fields
            if (!data.TryGetProperty("name", out var nameProp))
                return "Building name is required";

            var name = nameProp.GetString();

            if (string.IsNullOrWhiteSpace(name))
                return "Building name cannot be empty";

            return null; // Valid
        }
    }
}
