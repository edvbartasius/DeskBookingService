using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MapsterMapper;
using System.Data.Common;
using DeskBookingService.Services;
using FluentValidation;


namespace DeskBookingService.Controllers
{
    [ApiController]
    [Route("api/desks")]
    public class DeskController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<Desk> _validator;

        public DeskController(AppDbContext context, IMapper mapper, IValidator<Desk> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }

        /// <summary>
        /// Retrieves all desks from the database
        /// </summary>
        /// <returns>List of desk DTOs</returns>
        [HttpGet("get-desks")]
        public async Task<IActionResult> GetDesks()
        {
            try
            {
                var desks = await _context.Desks.ToListAsync();
                var deskDtos = _mapper.Map<List<DeskDto>>(desks);
                return Ok(deskDtos);
            }
            catch (DbException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }

        }

        /// <summary>
        /// Adds a new desk to the database
        /// </summary>
        /// <param name="deskDto">Desk data transfer object</param>
        /// <returns>Created desk object or error message</returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddDesk([FromBody] DeskDto deskDto)
        {
            try
            {
                var desk = _mapper.Map<Desk>(deskDto);

                // Validate using FluentValidation
                var validationResult = await _validator.ValidateAsync(desk);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { error = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)) });
                }

                _context.Desks.Add(desk);
                await _context.SaveChangesAsync();

                return Ok(desk);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        /// <summary>
        /// Deletes a desk from the database by ID
        /// </summary>
        /// <param name="id">Desk ID to delete</param>
        /// <returns>Success or error message</returns>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteDesk(int id)
        {
            try
            {
                var entry = await _context.Desks.FindAsync(id);
                if (entry == null)
                {
                    return NotFound($"Desk with ID: {id}, not found");
                }
                _context.Desks.Remove(entry);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing desk's information
        /// </summary>
        /// <param name="id">Desk ID to update (from route)</param>
        /// <param name="deskDto">Updated desk data</param>
        /// <returns>Success or error message</returns>
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateDesk(int id, [FromBody] DeskDto deskDto)
        {
            try
            {
                var desk = await _context.Desks.FindAsync(deskDto.Id);
                if (desk == null)
                {
                    return NotFound();
                }

                // map new properties to entity
                _mapper.Map(deskDto, desk);

                // Validate using FluentValidation
                var validationResult = await _validator.ValidateAsync(desk);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { error = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)) });
                }

                _context.Desks.Update(desk);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "An error occurred while processing your request: " + ex.Message);
            }
        }
    }
}