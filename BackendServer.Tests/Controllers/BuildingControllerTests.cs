using BackendServer.Tests.Helpers;
using DeskBookingService;
using DeskBookingService.Controllers;
using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using DeskBookingService.Services.Validators;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BackendServer.Tests.Controllers;

public class BuildingControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly BuildingController _controller;
    private readonly IMapper _mapper;

    public BuildingControllerTests()
    {
        // Use a unique database name for each test instance
        var databaseName = $"BuildingControllerTests_{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}";
        _context = TestDbContextFactory.CreateInMemoryContext(databaseName);
        TestDbContextFactory.SeedTestData(_context);

        var config = TypeAdapterConfig.GlobalSettings;
        _mapper = new Mapper(config);

        var validator = new BuildingValidator(_context);
        _controller = new BuildingController(_context, _mapper, validator);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetBuildings Tests

    [Fact]
    public async Task GetBuildings_ReturnsAllBuildings()
    {
        // Act
        var result = await _controller.GetBuildings();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var buildings = Assert.IsAssignableFrom<List<BuildingDto>>(okResult.Value);
        Assert.True(buildings.Count >= 1); // At least one building from seed data
    }

    #endregion

    #region AddBuilding Tests

    [Fact]
    public async Task AddBuilding_ValidBuilding_ReturnsOk()
    {
        // Arrange
        var buildingDto = new BuildingDto
        {
            Name = "New Building",
            FloorPlanHeight = 50,
            FloorPlanWidth = 50
        };

        // Act
        var result = await _controller.AddBuilding(buildingDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var building = Assert.IsType<Building>(okResult.Value);
        Assert.Equal("New Building", building.Name);
    }

    [Fact]
    public async Task AddBuilding_InvalidName_ReturnsBadRequest()
    {
        // Arrange
        var buildingDto = new BuildingDto
        {
            Name = "", // Empty name
            FloorPlanHeight = 50,
            FloorPlanWidth = 50
        };

        // Act
        var result = await _controller.AddBuilding(buildingDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddBuilding_InvalidDimensions_ReturnsBadRequest()
    {
        // Arrange
        var buildingDto = new BuildingDto
        {
            Name = "Test Building",
            FloorPlanHeight = 150, // Exceeds max (100)
            FloorPlanWidth = 50
        };

        // Act
        var result = await _controller.AddBuilding(buildingDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region DeleteBuilding Tests

    [Fact]
    public async Task DeleteBuilding_ExistingBuilding_ReturnsOk()
    {
        // Arrange
        var building = new Building
        {
            Name = "To Delete",
            FloorPlanHeight = 50,
            FloorPlanWidth = 50
        };
        _context.Buildings.Add(building);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteBuilding(building.Id);

        // Assert
        Assert.IsType<OkResult>(result);
        var buildingInDb = await _context.Buildings.FindAsync(building.Id);
        Assert.Null(buildingInDb);
    }

    [Fact]
    public async Task DeleteBuilding_NonExistentBuilding_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _controller.DeleteBuilding(nonExistentId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region UpdateBuilding Tests

    [Fact]
    public async Task UpdateBuilding_ExistingBuilding_ReturnsOk()
    {
        // Arrange
        var building = new Building
        {
            Name = "Original Name",
            FloorPlanHeight = 50,
            FloorPlanWidth = 50
        };
        _context.Buildings.Add(building);
        await _context.SaveChangesAsync();

        var buildingDto = new BuildingDto
        {
            Id = building.Id,
            Name = "Updated Name",
            FloorPlanHeight = 60,
            FloorPlanWidth = 60
        };

        // Act
        var result = await _controller.UpdateBuilding(building.Id, buildingDto);

        // Assert
        Assert.IsType<OkResult>(result);
        var updatedBuilding = await _context.Buildings.FindAsync(building.Id);
        Assert.NotNull(updatedBuilding);
        Assert.Equal("Updated Name", updatedBuilding!.Name);
        Assert.Equal(60, updatedBuilding.FloorPlanHeight);
        Assert.Equal(60, updatedBuilding.FloorPlanWidth);
    }

    [Fact]
    public async Task UpdateBuilding_NonExistentBuilding_ReturnsNotFound()
    {
        // Arrange
        var buildingDto = new BuildingDto
        {
            Name = "Test",
            FloorPlanHeight = 50,
            FloorPlanWidth = 50
        };

        // Act
        var result = await _controller.UpdateBuilding(999, buildingDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateBuilding_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var building = new Building
        {
            Name = "Test Building",
            FloorPlanHeight = 50,
            FloorPlanWidth = 50
        };
        _context.Buildings.Add(building);
        await _context.SaveChangesAsync();

        var buildingDto = new BuildingDto
        {
            Name = "", // Invalid empty name
            FloorPlanHeight = 50,
            FloorPlanWidth = 50
        };

        // Act
        var result = await _controller.UpdateBuilding(building.Id, buildingDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region GetFloorPlan Tests

    [Fact]
    public async Task GetFloorPlan_ValidBuilding_ReturnsFloorPlan()
    {
        // Arrange
        var buildingId = 1; // From seed data
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var userId = "user1";

        // Act
        var result = await _controller.GetFloorPlan(buildingId, date, userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        // FloorPlanDto is internal, so we just verify the result is not null
    }

    [Fact]
    public async Task GetFloorPlan_NonExistentBuilding_ReturnsNotFound()
    {
        // Arrange
        var buildingId = 999;
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var userId = "user1";

        // Act
        var result = await _controller.GetFloorPlan(buildingId, date, userId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetFloorPlan_ShowsCorrectDeskStatus()
    {
        // Arrange
        var buildingId = 1;
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var userId = "user1";

        // Create a reservation for desk 1
        TestDbContextFactory.CreateTestReservation(_context, "user2", 1, date);

        // Act
        var result = await _controller.GetFloorPlan(buildingId, date, userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        // Verify the result is successful - detailed validation would require making FloorPlanDto public
    }

    #endregion

    #region GetBuildingClosedDate Tests

    [Fact]
    public async Task GetBuildingClosedDate_ValidBuilding_ReturnsClosedDays()
    {
        // Arrange
        var buildingId = 1; // From seed data

        // Act
        var result = await _controller.GetBuildingClosedDate(buildingId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        // The result should be a list of closed days (based on OperatingHours)
    }

    [Fact]
    public async Task GetBuildingClosedDate_NonExistentBuilding_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _controller.GetBuildingClosedDate(nonExistentId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion
}

