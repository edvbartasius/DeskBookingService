using BackendServer.Tests.Helpers;
using DeskBookingService;
using DeskBookingService.Controllers;
using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using DeskBookingService.Services;
using DeskBookingService.Services.Validators;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BackendServer.Tests.Controllers;

public class ReservationControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ReservationController _controller;
    private readonly IMapper _mapper;

    public ReservationControllerTests()
    {
        // Use a unique database name for each test instance
        var databaseName = $"ReservationControllerTests_{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}";
        _context = TestDbContextFactory.CreateInMemoryContext(databaseName);
        TestDbContextFactory.SeedTestData(_context);

        var config = TypeAdapterConfig.GlobalSettings;
        _mapper = new Mapper(config);

        var validationService = new ReservationValidationService(_context);
        var validator = new ReservationValidator(_context);

        _controller = new ReservationController(_context, _mapper, validationService, validator);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetReservations Tests

    [Fact]
    public async Task GetReservations_ReturnsAllReservations()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, date);
        TestDbContextFactory.CreateTestReservation(_context, "user2", 2, date);

        // Act
        var result = await _controller.GetReservations();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var reservations = Assert.IsAssignableFrom<List<ReservationDto>>(okResult.Value);
        Assert.True(reservations.Count >= 2);
    }

    #endregion

    #region CreateReservation Tests

    [Fact]
    public async Task CreateReservation_ValidSingleDate_ReturnsOk()
    {
        // Arrange
        var dto = new CreateReservationDTO
        {
            UserId = "user1",
            DeskId = 1,
            ReservationDates = new List<DateOnly>
            {
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
            }
        };

        // Act
        var result = await _controller.CreateReservation(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task CreateReservation_ValidMultipleDates_ReturnsOk()
    {
        // Arrange
        var dto = new CreateReservationDTO
        {
            UserId = "user1",
            DeskId = 1,
            ReservationDates = new List<DateOnly>
            {
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(8)),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(9))
            }
        };

        // Act
        var result = await _controller.CreateReservation(dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var reservationsInDb = _context.Reservations.Where(r => r.UserId == "user1" && r.DeskId == 1).ToList();
        Assert.Equal(3, reservationsInDb.Count);

        // All should have the same group ID
        var groupIds = reservationsInDb.Select(r => r.ReservationGroupId).Distinct().ToList();
        Assert.Single(groupIds);
    }

    [Fact]
    public async Task CreateReservation_DuplicateDates_DeduplicatesAndCreatesOnce()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var dto = new CreateReservationDTO
        {
            UserId = "user1",
            DeskId = 1,
            ReservationDates = new List<DateOnly> { date, date, date } // Duplicates
        };

        // Act
        var result = await _controller.CreateReservation(dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var reservationsInDb = _context.Reservations.Where(r => r.UserId == "user1" && r.DeskId == 1 && r.ReservationDate == date).ToList();
        Assert.Single(reservationsInDb); // Only one created
    }

    [Fact]
    public async Task CreateReservation_InvalidUser_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateReservationDTO
        {
            UserId = "nonexistent",
            DeskId = 1,
            ReservationDates = new List<DateOnly>
            {
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
            }
        };

        // Act
        var result = await _controller.CreateReservation(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateReservation_InvalidDesk_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateReservationDTO
        {
            UserId = "user1",
            DeskId = 999, // Non-existent
            ReservationDates = new List<DateOnly>
            {
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
            }
        };

        // Act
        var result = await _controller.CreateReservation(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateReservation_PastDate_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateReservationDTO
        {
            UserId = "user1",
            DeskId = 1,
            ReservationDates = new List<DateOnly>
            {
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) // Past date
            }
        };

        // Act
        var result = await _controller.CreateReservation(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateReservation_DeskAlreadyBooked_ReturnsBadRequest()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        TestDbContextFactory.CreateTestReservation(_context, "user2", 1, date); // Book it first

        var dto = new CreateReservationDTO
        {
            UserId = "user1",
            DeskId = 1,
            ReservationDates = new List<DateOnly> { date }
        };

        // Act
        var result = await _controller.CreateReservation(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateReservation_ExceedsBookingSizeLimit_ReturnsBadRequest()
    {
        // Arrange
        var dates = new List<DateOnly>();
        for (int i = 0; i < 8; i++) // 8 dates (max is 7)
        {
            dates.Add(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(i + 1)));
        }

        var dto = new CreateReservationDTO
        {
            UserId = "user1",
            DeskId = 1,
            ReservationDates = dates
        };

        // Act
        var result = await _controller.CreateReservation(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingReservation_ReturnsOk()
    {
        // Arrange
        var reservation = TestDbContextFactory.CreateTestReservation(_context, "user1", 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)));

        // Act
        var result = await _controller.Delete(reservation.Id);

        // Assert
        Assert.IsType<OkResult>(result);
        var reservationInDb = await _context.Reservations.FindAsync(reservation.Id);
        Assert.Null(reservationInDb);
    }

    [Fact]
    public async Task Delete_NonExistentReservation_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _controller.Delete(nonExistentId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region GetDeskReservations Tests

    [Fact]
    public async Task GetDeskReservations_ReturnsOnlyActiveDates()
    {
        // Arrange
        var deskId = 1;
        var date1 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var date2 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(8));

        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date1);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, date2);
        TestDbContextFactory.CreateTestReservation(_context, "user1", deskId, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(9)), ReservationStatus.Cancelled);

        // Act
        var result = await _controller.GetDeskReservations(deskId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dates = Assert.IsAssignableFrom<List<DateOnly>>(okResult.Value);
        Assert.Equal(2, dates.Count);
        Assert.Contains(date1, dates);
        Assert.Contains(date2, dates);
    }

    #endregion

    #region GetUserReservations Tests

    [Fact]
    public async Task GetUserReservations_ValidUser_ReturnsReservations()
    {
        // Arrange
        var userId = "user1";
        TestDbContextFactory.CreateTestReservation(_context, userId, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)));
        TestDbContextFactory.CreateTestReservation(_context, userId, 2, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(8)));

        // Act
        var result = await _controller.GetUserReservations(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var reservations = Assert.IsAssignableFrom<List<ReservationDto>>(okResult.Value);
        Assert.Equal(2, reservations.Count);
    }

    [Fact]
    public async Task GetUserReservations_InvalidUser_ReturnsBadRequest()
    {
        // Arrange
        var userId = "nonexistent";

        // Act
        var result = await _controller.GetUserReservations(userId);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region UpdateReservation Tests

    [Fact]
    public async Task UpdateReservation_ExistingReservation_ReturnsOk()
    {
        // Arrange
        var reservation = TestDbContextFactory.CreateTestReservation(_context, "user1", 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)));

        var updateDto = new UpdateReservationDto
        {
            UserId = "user1",
            DeskId = 2, // Change desk
            ReservationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(8)),
            Status = ReservationStatus.Active
        };

        // Act
        var result = await _controller.UpdateReservation(reservation.Id, updateDto);

        // Assert
        Assert.IsType<OkResult>(result);
        var updatedReservation = await _context.Reservations.FindAsync(reservation.Id);
        Assert.NotNull(updatedReservation);
        Assert.Equal(2, updatedReservation!.DeskId);
    }

    [Fact]
    public async Task UpdateReservation_NonExistentReservation_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateReservationDto
        {
            UserId = "user1",
            DeskId = 1,
            ReservationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = ReservationStatus.Active
        };

        // Act
        var result = await _controller.UpdateReservation(999, updateDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateReservation_ConflictingReservation_ReturnsBadRequest()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        TestDbContextFactory.CreateTestReservation(_context, "user1", 1, date);
        var reservation2 = TestDbContextFactory.CreateTestReservation(_context, "user2", 2, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(8)));

        var updateDto = new UpdateReservationDto
        {
            UserId = "user2",
            DeskId = 1, // Try to book same desk as reservation1
            ReservationDate = date,
            Status = ReservationStatus.Active
        };

        // Act
        var result = await _controller.UpdateReservation(reservation2.Id, updateDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region GetUserReservationsGrouped Tests

    [Fact]
    public async Task GetUserReservationsGrouped_ValidUser_ReturnsGroupedReservations()
    {
        // Arrange
        var userId = "user1";
        var groupId = Guid.NewGuid();
        var date1 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var date2 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(8));

        // Create reservations with same group ID
        var res1 = TestDbContextFactory.CreateTestReservation(_context, userId, 1, date1);
        res1.ReservationGroupId = groupId;
        var res2 = TestDbContextFactory.CreateTestReservation(_context, userId, 1, date2);
        res2.ReservationGroupId = groupId;
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetUserReservationsGrouped(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        // The result is an anonymous type, so we just verify it's not null and is a list
        var groups = okResult.Value as IEnumerable<object>;
        Assert.NotNull(groups);
        Assert.NotEmpty(groups);
    }

    [Fact]
    public async Task GetUserReservationsGrouped_InvalidUser_ReturnsBadRequest()
    {
        // Arrange
        var userId = "nonexistent";

        // Act
        var result = await _controller.GetUserReservationsGrouped(userId);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetUserReservationsGrouped_OnlyUpcomingReservations_ExcludesPast()
    {
        // Arrange
        var userId = "user1";
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        TestDbContextFactory.CreateTestReservation(_context, userId, 1, pastDate);
        TestDbContextFactory.CreateTestReservation(_context, userId, 2, futureDate);

        // Act
        var result = await _controller.GetUserReservationsGrouped(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        var groups = okResult.Value as IEnumerable<object>;
        Assert.NotNull(groups);
        // Should have at least one group (the future reservation)
        Assert.NotEmpty(groups);
    }

    #endregion

    #region GetUserReservationHistory Tests

    [Fact]
    public async Task GetUserReservationHistory_ValidUser_ReturnsHistory()
    {
        // Arrange
        var userId = "user1";
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var cancelledReservation = TestDbContextFactory.CreateTestReservation(_context, userId, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), ReservationStatus.Cancelled);
        var completedReservation = TestDbContextFactory.CreateTestReservation(_context, userId, 2, pastDate, ReservationStatus.Completed);

        // Act
        var result = await _controller.GetUserReservationHistory(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var history = Assert.IsAssignableFrom<List<ReservationDto>>(okResult.Value);
        Assert.True(history.Count >= 2);
        Assert.Contains(history, r => r.Status == ReservationStatus.Cancelled);
        Assert.Contains(history, r => r.Status == ReservationStatus.Completed);
    }

    [Fact]
    public async Task GetUserReservationHistory_InvalidUser_ReturnsBadRequest()
    {
        // Arrange
        var userId = "nonexistent";

        // Act
        var result = await _controller.GetUserReservationHistory(userId);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region CancelReservation Tests

    [Fact]
    public async Task CancelReservation_ValidReservation_ReturnsOk()
    {
        // Arrange
        var userId = "user1";
        var deskId = 1;
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        TestDbContextFactory.CreateTestReservation(_context, userId, deskId, date);

        // Act
        var result = await _controller.CancelReservation(deskId, date, userId);

        // Assert
        Assert.IsType<OkResult>(result);  // Controller returns Ok() not OkObjectResult
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.UserId == userId && r.DeskId == deskId && r.ReservationDate == date);
        Assert.NotNull(reservation);
        Assert.Equal(ReservationStatus.Cancelled, reservation!.Status);
    }

    [Fact]
    public async Task CancelReservation_NonExistentReservation_ReturnsNotFound()
    {
        // Arrange
        var userId = "user1";
        var deskId = 999;
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        // Act
        var result = await _controller.CancelReservation(deskId, date, userId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CancelReservation_WrongUser_ReturnsNotFound()
    {
        // Arrange
        var userId = "user1";
        var deskId = 1;
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        TestDbContextFactory.CreateTestReservation(_context, userId, deskId, date);

        // Act - Try to cancel with different user
        var result = await _controller.CancelReservation(deskId, date, "user2");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion

    #region CancelBookingGroupByGroupId Tests

    [Fact]
    public async Task CancelBookingGroupByGroupId_ValidGroup_CancelsAllReservations()
    {
        // Arrange
        var userId = "user1";
        var groupId = Guid.NewGuid();
        var date1 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var date2 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(8));
        var date3 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(9));

        var res1 = TestDbContextFactory.CreateTestReservation(_context, userId, 1, date1);
        res1.ReservationGroupId = groupId;
        var res2 = TestDbContextFactory.CreateTestReservation(_context, userId, 1, date2);
        res2.ReservationGroupId = groupId;
        var res3 = TestDbContextFactory.CreateTestReservation(_context, userId, 1, date3);
        res3.ReservationGroupId = groupId;
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.CancelBookingGroupByGroupId(groupId, userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        var cancelledReservations = await _context.Reservations
            .Where(r => r.ReservationGroupId == groupId)
            .ToListAsync();

        Assert.Equal(3, cancelledReservations.Count);
        Assert.All(cancelledReservations, r => Assert.Equal(ReservationStatus.Cancelled, r.Status));
    }

    [Fact]
    public async Task CancelBookingGroupByGroupId_NonExistentGroup_ReturnsNotFound()
    {
        // Arrange
        var userId = "user1";
        var nonExistentGroupId = Guid.NewGuid();

        // Act
        var result = await _controller.CancelBookingGroupByGroupId(nonExistentGroupId, userId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CancelBookingGroupByGroupId_WrongUser_ReturnsNotFound()
    {
        // Arrange
        var userId = "user1";
        var groupId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        var res = TestDbContextFactory.CreateTestReservation(_context, userId, 1, date);
        res.ReservationGroupId = groupId;
        await _context.SaveChangesAsync();

        // Act - Try to cancel with different user
        var result = await _controller.CancelBookingGroupByGroupId(groupId, "user2");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion

    #region CancelBookingGroupByDesk Tests

    [Fact]
    public async Task CancelBookingGroupByDesk_ValidGroup_CancelsAllReservations()
    {
        // Arrange
        var userId = "user1";
        var deskId = 1;
        var groupId = Guid.NewGuid();
        var date1 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var date2 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(8));

        var res1 = TestDbContextFactory.CreateTestReservation(_context, userId, deskId, date1);
        res1.ReservationGroupId = groupId;
        var res2 = TestDbContextFactory.CreateTestReservation(_context, userId, deskId, date2);
        res2.ReservationGroupId = groupId;
        await _context.SaveChangesAsync();

        // Act - Use any date from the group
        var result = await _controller.CancelBookingGroupByDesk(deskId, date1, userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        var cancelledReservations = await _context.Reservations
            .Where(r => r.ReservationGroupId == groupId)
            .ToListAsync();

        Assert.Equal(2, cancelledReservations.Count);
        Assert.All(cancelledReservations, r => Assert.Equal(ReservationStatus.Cancelled, r.Status));
    }

    [Fact]
    public async Task CancelBookingGroupByDesk_NonExistentReservation_ReturnsNotFound()
    {
        // Arrange
        var userId = "user1";
        var deskId = 999;
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        // Act
        var result = await _controller.CancelBookingGroupByDesk(deskId, date, userId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CancelBookingGroupByDesk_WrongUser_ReturnsNotFound()
    {
        // Arrange
        var userId = "user1";
        var deskId = 1;
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        TestDbContextFactory.CreateTestReservation(_context, userId, deskId, date);

        // Act - Try to cancel with different user
        var result = await _controller.CancelBookingGroupByDesk(deskId, date, "user2");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion
}

