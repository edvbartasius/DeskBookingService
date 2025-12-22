namespace DeskBookingService.DatabaseSeeder;

using DeskBookingService.Models;

public static class DatabaseSeeder
{
    public static void Seed(AppDbContext dbContext)
    {
        dbContext.Database.EnsureCreated();

        // Add buildings first to get their IDs
        var buildings = GetPredefinedBuildings();
        dbContext.Buildings.AddRange(buildings);
        dbContext.SaveChanges();

        // Add operating hours for each building
        dbContext.OperatingHours.AddRange(GetOperatingHours(buildings));

        // Add users
        dbContext.Users.AddRange(GetPredefinedUsers());

        // Add reservations with time spans
        dbContext.Reservations.AddRange(GetPredefinedReservations());

        dbContext.SaveChanges();
    }

    private static List<Building> GetPredefinedBuildings()
    {
        return new List<Building>
        {
            new Building
            {
                Id = 1,
                Name = "Main Office",
                FloorPlanWidth = 15,  // 15 cells wide
                FloorPlanHeight = 10, // 10 cells tall
                Desks = new List<Desk>
                {
                    // Row 1: Regular desks along the top (window seats)
                    new Desk { Id = 101, DeskNumber = "1", Description = "Window desk", PositionX = 1, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 102, DeskNumber = "2", Description = "Window desk", PositionX = 2, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 103, DeskNumber = "3", Description = "Window desk", PositionX = 3, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 104, DeskNumber = "4", Description = "Window desk", PositionX = 4, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 105, DeskNumber = "5", Description = "Window desk 5", PositionX = 5, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 1 },

                    // Row 3: Regular desks
                    new Desk { Id = 111, DeskNumber = "6", Description = "Quiet area", PositionX = 1, PositionY = 5, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 112, DeskNumber = "7", Description = "Quiet area", PositionX = 2, PositionY = 5, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 113, DeskNumber = "8", Description = "Quiet area", PositionX = 3, PositionY = 5, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 114, DeskNumber = "9", Description = "Quiet area", PositionX = 4, PositionY = 5, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 115, DeskNumber = "10", Description = "Quiet area", PositionX = 5, PositionY = 5, Type = DeskType.RegularDesk, BuildingId = 1 },

                    // Conference rooms (larger - takes up more cells)
                    new Desk { Id = 150, DeskNumber = "20", Description = "Conference Room A", PositionX = 8, PositionY = 1, Type = DeskType.ConferenceRoom, BuildingId = 1 },
                    new Desk { Id = 151, DeskNumber = "21", Description = "Conference Room B", PositionX = 11, PositionY = 1, Type = DeskType.ConferenceRoom, BuildingId = 1 },
                    new Desk { Id = 152, DeskNumber = "22", Description = "Meeting Room", PositionX = 8, PositionY = 5, Type = DeskType.ConferenceRoom, BuildingId = 1 },
                }
            },
            new Building
            {
                Id = 2,
                Name = "Annex Building",
                FloorPlanWidth = 10,  // Smaller building
                FloorPlanHeight = 8,
                Desks = new List<Desk>
                {
                    // Small quiet office
                    new Desk { Id = 201, DeskNumber = "1", Description = "Focus desk 1", PositionX = 1, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 2 },
                    new Desk { Id = 202, DeskNumber = "2", Description = "Focus desk 2", PositionX = 2, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 2 },
                    new Desk { Id = 203, DeskNumber = "3", Description = "Focus desk 3", PositionX = 3, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 2 },
                    new Desk { Id = 204, DeskNumber = "4", Description = "Work desk 1", PositionX = 1, PositionY = 3, Type = DeskType.RegularDesk, BuildingId = 2 },
                    new Desk { Id = 205, DeskNumber = "5", Description = "Work desk 2", PositionX = 2, PositionY = 3, Type = DeskType.RegularDesk, BuildingId = 2 },

                    // Small conference room
                    new Desk { Id = 250, DeskNumber = "10",Description = "Small Meeting Room", PositionX = 6, PositionY = 2, Type = DeskType.ConferenceRoom, BuildingId = 2 },
                }
            }
        };
    }

    private static List<OperatingHours> GetOperatingHours(List<Building> buildings)
    {
        var hours = new List<OperatingHours>();

        foreach (var building in buildings)
        {
            if (building.Id == 1) // Main Office
            {
                // Monday-Friday: 7 AM - 7 PM
                for (int day = 1; day <= 5; day++)  // 1=Monday, 5=Friday
                {
                    hours.Add(new OperatingHours
                    {
                        BuildingId = building.Id,
                        DayOfWeek = (DayOfWeek)day,
                        OpeningTime = new TimeOnly(7, 0),
                        ClosingTime = new TimeOnly(19, 0),
                        IsClosed = false
                    });
                }

                // Saturday: 8 AM - 2 PM (half day)
                hours.Add(new OperatingHours
                {
                    BuildingId = building.Id,
                    DayOfWeek = DayOfWeek.Saturday,
                    OpeningTime = new TimeOnly(8, 0),
                    ClosingTime = new TimeOnly(14, 0),
                    IsClosed = false
                });

                // Sunday: Closed
                hours.Add(new OperatingHours
                {
                    BuildingId = building.Id,
                    DayOfWeek = DayOfWeek.Sunday,
                    OpeningTime = new TimeOnly(0, 0),
                    ClosingTime = new TimeOnly(0, 0),
                    IsClosed = true
                });
            }
            else if (building.Id == 2) // Annex Building
            {
                // Monday-Friday: 8 AM - 6 PM (shorter hours)
                for (int day = 1; day <= 5; day++)
                {
                    hours.Add(new OperatingHours
                    {
                        BuildingId = building.Id,
                        DayOfWeek = (DayOfWeek)day,
                        OpeningTime = new TimeOnly(8, 0),
                        ClosingTime = new TimeOnly(18, 0),
                        IsClosed = false
                    });
                }

                // Weekend: Closed
                hours.Add(new OperatingHours
                {
                    BuildingId = building.Id,
                    DayOfWeek = DayOfWeek.Saturday,
                    OpeningTime = new TimeOnly(0, 0),
                    ClosingTime = new TimeOnly(0, 0),
                    IsClosed = true
                });

                hours.Add(new OperatingHours
                {
                    BuildingId = building.Id,
                    DayOfWeek = DayOfWeek.Sunday,
                    OpeningTime = new TimeOnly(0, 0),
                    ClosingTime = new TimeOnly(0, 0),
                    IsClosed = true
                });
            }
        }

        return hours;
    }

    private static List<Reservation> GetPredefinedReservations()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var tomorrow = today.AddDays(1);
        var dayAfterTomorrow = today.AddDays(2);
        var threeDaysFromNow = today.AddDays(3);
        var fourDaysFromNow = today.AddDays(4);

        // Past dates for finished reservations
        var yesterday = today.AddDays(-1);
        var twoDaysAgo = today.AddDays(-2);
        var threeDaysAgo = today.AddDays(-3);
        var fiveDaysAgo = today.AddDays(-5);
        var sevenDaysAgo = today.AddDays(-7);
        var tenDaysAgo = today.AddDays(-10);

        // Reservation group IDs
        var adminFinishedGroupId = Guid.NewGuid(); // For admin's finished group
        var adminCancelledGroupId = Guid.NewGuid(); // For admin's cancelled group
        var adminFutureGroupId1 = Guid.NewGuid(); // For admin's upcoming multi-day booking
        var adminFutureGroupId2 = Guid.NewGuid(); // For admin's future conference room
        var user2FutureGroupId = Guid.NewGuid(); // For User 2's future multi-day booking
        var user2PastGroupId = Guid.NewGuid(); // For User 2's past booking
        var user3FutureGroupId = Guid.NewGuid(); // For User 3's future booking
        var user3PastGroupId = Guid.NewGuid(); // For User 3's past cancelled booking
        var user4FutureGroupId = Guid.NewGuid(); // For User 4's future consecutive booking
        var user4PastGroupId = Guid.NewGuid(); // For User 4's past booking
        var user5FutureGroupId = Guid.NewGuid(); // For User 5's future booking
        var user5PastGroupId = Guid.NewGuid(); // For User 5's past booking

        return new List<Reservation>
        {
            // ===== FINISHED RESERVATIONS FOR ADMIN (U1) =====

            // Finished Group 1: Admin had a 3-day booking last week (completed)
            new Reservation
            {
                Id = 100,
                DeskId = 102, // Window desk 2
                UserId = "U1",
                ReservationDate = tenDaysAgo,
                Status = ReservationStatus.Active, // Will show as Completed in history (backend logic)
                CreatedAt = DateTime.UtcNow.AddDays(-12),
                ReservationGroupId = adminFinishedGroupId
            },
            new Reservation
            {
                Id = 101,
                DeskId = 102,
                UserId = "U1",
                ReservationDate = tenDaysAgo.AddDays(1),
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-12),
                ReservationGroupId = adminFinishedGroupId
            },
            new Reservation
            {
                Id = 102,
                DeskId = 102,
                UserId = "U1",
                ReservationDate = tenDaysAgo.AddDays(2),
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-12),
                ReservationGroupId = adminFinishedGroupId
            },

            // Finished Group 2: Admin cancelled a booking (cancelled status)
            new Reservation
            {
                Id = 103,
                DeskId = 105, // Standing desk 1
                UserId = "U1",
                ReservationDate = fiveDaysAgo,
                Status = ReservationStatus.Cancelled,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                CanceledAt = DateTime.UtcNow.AddDays(-6),
                ReservationGroupId = adminCancelledGroupId
            },
            new Reservation
            {
                Id = 104,
                DeskId = 105,
                UserId = "U1",
                ReservationDate = fiveDaysAgo.AddDays(1),
                Status = ReservationStatus.Cancelled,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                CanceledAt = DateTime.UtcNow.AddDays(-6),
                ReservationGroupId = adminCancelledGroupId
            },

            // Individual finished reservation: Admin used conference room
            new Reservation
            {
                Id = 105,
                DeskId = 150, // Large conference room
                UserId = "U1",
                ReservationDate = threeDaysAgo,
                Status = ReservationStatus.Active, // Will show as Completed
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                ReservationGroupId = Guid.NewGuid()
            },

            // Another individual: Admin's desk from yesterday
            new Reservation
            {
                Id = 106,
                DeskId = 103, // Window desk 3
                UserId = "U1",
                ReservationDate = yesterday,
                Status = ReservationStatus.Active, // Will show as Completed
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                ReservationGroupId = Guid.NewGuid()
            },

            // ===== FUTURE RESERVATIONS FOR ADMIN (U1) =====

            // Future Group 1: Admin's upcoming 4-day booking (non-consecutive dates)
            new Reservation
            {
                Id = 107,
                DeskId = 104, // Window desk 4
                UserId = "U1",
                ReservationDate = tomorrow,
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow,
                ReservationGroupId = adminFutureGroupId1
            },
            new Reservation
            {
                Id = 108,
                DeskId = 104,
                UserId = "U1",
                ReservationDate = dayAfterTomorrow,
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow,
                ReservationGroupId = adminFutureGroupId1
            },
            new Reservation
            {
                Id = 109,
                DeskId = 104,
                UserId = "U1",
                ReservationDate = threeDaysFromNow,
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow,
                ReservationGroupId = adminFutureGroupId1
            },
            new Reservation
            {
                Id = 110,
                DeskId = 104,
                UserId = "U1",
                ReservationDate = fourDaysFromNow,
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow,
                ReservationGroupId = adminFutureGroupId1
            },

            // Future Group 2: Admin's conference room booking next week
            new Reservation
            {
                Id = 111,
                DeskId = 150, // Large conference room
                UserId = "U1",
                ReservationDate = today.AddDays(7),
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow,
                ReservationGroupId = adminFutureGroupId2
            },

            // Individual future: Admin's standing desk booking
            new Reservation
            {
                Id = 112,
                DeskId = 105, // Standing desk 1
                UserId = "U1",
                ReservationDate = today.AddDays(10),
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow,
                ReservationGroupId = Guid.NewGuid()
            },

            // ===== FUTURE RESERVATIONS FOR OTHER USERS =====

            // User 2: Multi-day booking (same group)
            new Reservation
            {
                Id = 1,
                DeskId = 101,
                UserId = "U2",
                ReservationDate = tomorrow,
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow,
                ReservationGroupId = user2FutureGroupId
            },
            new Reservation
            {
                Id = 2,
                DeskId = 101,
                UserId = "U2",
                ReservationDate = dayAfterTomorrow,
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow,
                ReservationGroupId = user2FutureGroupId // Same group
            },

            // User 3: Conference room booking
            new Reservation
            {
                Id = 3,
                DeskId = 150,
                UserId = "U3",
                ReservationDate = tomorrow,
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow,
                ReservationGroupId = user3FutureGroupId
            },

            // User 4: Consecutive booking (same group)
            new Reservation
            {
                Id = 4,
                DeskId = 106,
                UserId = "U4",
                ReservationDate = threeDaysFromNow,
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow,
                ReservationGroupId = user4FutureGroupId
            },
            new Reservation
            {
                Id = 5,
                DeskId = 106,
                UserId = "U4",
                ReservationDate = fourDaysFromNow,
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow,
                ReservationGroupId = user4FutureGroupId // Same group
            },

            // User 5: Single day booking
            new Reservation
            {
                Id = 6,
                DeskId = 107,
                UserId = "U5",
                ReservationDate = tomorrow,
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow,
                ReservationGroupId = user5FutureGroupId
            },

            // ===== PAST RESERVATIONS FOR OTHER USERS =====

            // User 2: Past completed booking
            new Reservation
            {
                Id = 200,
                DeskId = 101,
                UserId = "U2",
                ReservationDate = fiveDaysAgo,
                Status = ReservationStatus.Active, // Will show as Completed
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                ReservationGroupId = user2PastGroupId
            },
            new Reservation
            {
                Id = 201,
                DeskId = 101,
                UserId = "U2",
                ReservationDate = fiveDaysAgo.AddDays(1),
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                ReservationGroupId = user2PastGroupId
            },

            // User 3: Past cancelled booking
            new Reservation
            {
                Id = 202,
                DeskId = 150,
                UserId = "U3",
                ReservationDate = threeDaysAgo,
                Status = ReservationStatus.Cancelled,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                CanceledAt = DateTime.UtcNow.AddDays(-4),
                ReservationGroupId = user3PastGroupId
            },

            // User 4: Past completed booking
            new Reservation
            {
                Id = 203,
                DeskId = 106,
                UserId = "U4",
                ReservationDate = sevenDaysAgo,
                Status = ReservationStatus.Active, // Will show as Completed
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                ReservationGroupId = user4PastGroupId
            },
            new Reservation
            {
                Id = 204,
                DeskId = 106,
                UserId = "U4",
                ReservationDate = sevenDaysAgo.AddDays(1),
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                ReservationGroupId = user4PastGroupId
            },
            new Reservation
            {
                Id = 205,
                DeskId = 106,
                UserId = "U4",
                ReservationDate = sevenDaysAgo.AddDays(2),
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                ReservationGroupId = user4PastGroupId
            },

            // User 5: Past completed booking
            new Reservation
            {
                Id = 206,
                DeskId = 107,
                UserId = "U5",
                ReservationDate = yesterday,
                Status = ReservationStatus.Active, // Will show as Completed
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ReservationGroupId = user5PastGroupId
            }
        };
    }

    private static List<User> GetPredefinedUsers()
    {
        return new List<User>
        {
            new User { Id = "U1",
             Name = "Admin",
             Surname = "Manager",
             Password = "SecurePassword123!",
             Email = "admin@company.com",
             Role = UserRole.Admin,
            },
            new User { Id = "U2",
             Name = "Sancho",
             Surname = "Eaglesham",
             Password = "seaglesham0",
             Email = "seaglesham0@furl.net",
             Role = UserRole.User
            },
            new User { Id = "U3",
             Name = "Bengt",
             Surname = "Servant",
             Password = "bservant1",
             Email = "bservant1@quantcast.com",
             Role = UserRole.User
            },
            new User { Id = "U4",
             Name = "Jayson",
             Surname = "Tonnesen",
             Password = "jtonnesen9",
             Email = "jtonnesen9@shutterfly.com",
             Role = UserRole.User
            },
            new User { Id = "U5",
             Name = "Berta",
             Surname = "Jerger",
             Password = "bjerger7",
             Email = "bjerger7@mozilla.org",
             Role = UserRole.User
            },
        };
    }
}