using BackendServer.Tests.Helpers;
using DeskBookingService;
using DeskBookingService.Controllers;
using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using DeskBookingService.Services;
using DeskBookingService.Services.Validators;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BackendServer.Tests.Controllers;

public class DeskControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DeskController _controller;
    private readonly IMapper _mapper;

    public DeskControllerTests()
    {
        // Use a unique database name for each test instance
        var databaseName = $"DeskControllerTests_{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}";
        _context = TestDbContextFactory.CreateInMemoryContext(databaseName);
        TestDbContextFactory.SeedTestData(_context);

        var config = TypeAdapterConfig.GlobalSettings;
        _mapper = new Mapper(config);

        var deskAvailabilityService = new DeskAvailabilityService(_context);
        var validator = new DeskValidator(_context);
        _controller = new DeskController(_context, _mapper, deskAvailabilityService, validator);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetDesks Tests

    [Fact]
    public async Task GetDesks_ReturnsAllDesks()
    {
        // Act
        var result = await _controller.GetDesks();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var desks = Assert.IsAssignableFrom<List<DeskDto>>(okResult.Value);
        Assert.True(desks.Count >= 3); // At least 3 desks from seed data
    }

    #endregion

    #region AddDesk Tests

    [Fact]
    public async Task AddDesk_ValidDesk_ReturnsOk()
    {
        // Arrange
        var deskDto = new DeskDto
        {
            Description = "New Desk",
            BuildingId = 1,
            PositionX = 5,
            PositionY = 5
        };

        // Act
        var result = await _controller.AddDesk(deskDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var desk = Assert.IsType<Desk>(okResult.Value);
        Assert.Equal("New Desk", desk.Description);
        Assert.Equal(1, desk.BuildingId);
    }

    [Fact]
    public async Task AddDesk_InvalidBuildingId_ReturnsBadRequest()
    {
        // Arrange
        var deskDto = new DeskDto
        {
            Description = "Test Desk",
            BuildingId = 999, // Non-existent building
            PositionX = 10,
            PositionY = 10
        };

        // Act
        var result = await _controller.AddDesk(deskDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddDesk_EmptyDescription_ReturnsBadRequest()
    {
        // Arrange
        var deskDto = new DeskDto
        {
            Description = "", // Empty description
            BuildingId = 1,
            PositionX = 10,
            PositionY = 10
        };

        // Act
        var result = await _controller.AddDesk(deskDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddDesk_PositionOutsideFloorPlan_ReturnsBadRequest()
    {
        // Arrange - Building 1 has FloorPlanHeight=100, FloorPlanWidth=100
        var deskDto = new DeskDto
        {
            Description = "Test Desk",
            BuildingId = 1,
            PositionX = 150, // Outside floor plan
            PositionY = 10
        };

        // Act
        var result = await _controller.AddDesk(deskDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddDesk_NegativePosition_ReturnsBadRequest()
    {
        // Arrange
        var deskDto = new DeskDto
        {
            Description = "Test Desk",
            BuildingId = 1,
            PositionX = -5, // Negative position
            PositionY = 10
        };

        // Act
        var result = await _controller.AddDesk(deskDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region DeleteDesk Tests

    [Fact]
    public async Task DeleteDesk_ExistingDesk_ReturnsOk()
    {
        // Arrange
        var desk = new Desk
        {
            Description = "To Delete",
            BuildingId = 1,
            PositionX = 10,
            PositionY = 10
        };
        _context.Desks.Add(desk);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteDesk(desk.Id);

        // Assert
        Assert.IsType<OkResult>(result);
        var deskInDb = await _context.Desks.FindAsync(desk.Id);
        Assert.Null(deskInDb);
    }

    [Fact]
    public async Task DeleteDesk_NonExistentDesk_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _controller.DeleteDesk(nonExistentId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion

    #region UpdateDesk Tests

    [Fact]
    public async Task UpdateDesk_ExistingDesk_ReturnsOk()
    {
        // Arrange
        var desk = new Desk
        {
            Description = "Original Description",
            BuildingId = 1,
            PositionX = 5,
            PositionY = 5
        };
        _context.Desks.Add(desk);
        await _context.SaveChangesAsync();

        var deskDto = new DeskDto
        {
            Id = desk.Id,
            Description = "Updated Description",
            BuildingId = 1,
            PositionX = 8,  // Within bounds (0-14)
            PositionY = 7   // Within bounds (0-9)
        };

        // Act
        var result = await _controller.UpdateDesk(desk.Id, deskDto);

        // Assert
        Assert.IsType<OkResult>(result);  // Controller returns Ok() not OkObjectResult
        var updatedDesk = await _context.Desks.FindAsync(desk.Id);
        Assert.NotNull(updatedDesk);
        Assert.Equal("Updated Description", updatedDesk!.Description);
        Assert.Equal(8, updatedDesk.PositionX);
        Assert.Equal(7, updatedDesk.PositionY);
    }

    [Fact]
    public async Task UpdateDesk_NonExistentDesk_ReturnsNotFound()
    {
        // Arrange
        var deskDto = new DeskDto
        {
            Id = 999,
            Description = "Test",
            BuildingId = 1,
            PositionX = 10,
            PositionY = 10
        };

        // Act
        var result = await _controller.UpdateDesk(999, deskDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateDesk_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var desk = new Desk
        {
            Description = "Test Desk",
            BuildingId = 1,
            PositionX = 5,
            PositionY = 5
        };
        _context.Desks.Add(desk);
        await _context.SaveChangesAsync();

        var deskDto = new DeskDto
        {
            Id = desk.Id,
            Description = "", // Invalid empty description
            BuildingId = 1,
            PositionX = 5,
            PositionY = 5
        };

        // Act
        var result = await _controller.UpdateDesk(desk.Id, deskDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateDesk_PositionOutsideFloorPlan_ReturnsBadRequest()
    {
        // Arrange
        var desk = new Desk
        {
            Description = "Test Desk",
            BuildingId = 1,
            PositionX = 5,
            PositionY = 5
        };
        _context.Desks.Add(desk);
        await _context.SaveChangesAsync();

        var deskDto = new DeskDto
        {
            Id = desk.Id,
            Description = "Test Desk",
            BuildingId = 1,
            PositionX = 150, // Outside floor plan (max is 14)
            PositionY = 5
        };

        // Act
        var result = await _controller.UpdateDesk(desk.Id, deskDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateDesk_MismatchedIds_ReturnsNotFound()
    {
        // Arrange
        var desk = new Desk
        {
            Description = "Test Desk",
            BuildingId = 1,
            PositionX = 5,
            PositionY = 5
        };
        _context.Desks.Add(desk);
        await _context.SaveChangesAsync();

        var deskDto = new DeskDto
        {
            Id = 999, // Different from route parameter - controller uses deskDto.Id, so returns NotFound
            Description = "Test Desk",
            BuildingId = 1,
            PositionX = 5,
            PositionY = 5
        };

        // Act
        var result = await _controller.UpdateDesk(desk.Id, deskDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion
}

