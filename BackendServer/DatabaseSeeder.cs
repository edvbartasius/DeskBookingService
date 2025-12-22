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
                Name = "Main Office - Downtown",
                FloorPlanWidth = 15,  // 15 cells wide
                FloorPlanHeight = 10, // 10 cells tall
                Desks = new List<Desk>
                {
                    // Row 1: Regular desks along the top (window seats with city view)
                    new Desk { Id = 101, DeskNumber = "A1", Description = "Window desk - City view", PositionX = 1, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 102, DeskNumber = "A2", Description = "Window desk - City view", PositionX = 2, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 103, DeskNumber = "A3", Description = "Window desk - City view", PositionX = 3, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 104, DeskNumber = "A4", Description = "Window desk - City view", PositionX = 4, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 105, DeskNumber = "A5", Description = "Standing desk - Window seat", PositionX = 5, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 1 },

                    // Row 2: Central collaboration area
                    new Desk { Id = 106, DeskNumber = "B1", Description = "Near kitchen - Collaborative zone", PositionX = 1, PositionY = 3, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 107, DeskNumber = "B2", Description = "Dual monitor setup available", PositionX = 2, PositionY = 3, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 108, DeskNumber = "B3", Description = "Ergonomic chair - Height adjustable", PositionX = 3, PositionY = 3, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 109, DeskNumber = "B4", Description = "Standing desk - Central location", PositionX = 4, PositionY = 3, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 110, DeskNumber = "B5", Description = "Desk with whiteboard nearby", PositionX = 5, PositionY = 3, Type = DeskType.RegularDesk, BuildingId = 1 },

                    // Row 3: Quiet zone desks
                    new Desk { Id = 111, DeskNumber = "C1", Description = "Quiet zone - Library rules", PositionX = 1, PositionY = 5, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 112, DeskNumber = "C2", Description = "Quiet zone - Focus area", PositionX = 2, PositionY = 5, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 113, DeskNumber = "C3", Description = "Quiet zone - Near power outlets", PositionX = 3, PositionY = 5, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 114, DeskNumber = "C4", Description = "Quiet zone - Corner desk", PositionX = 4, PositionY = 5, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 115, DeskNumber = "C5", Description = "Quiet zone - Privacy panels", PositionX = 5, PositionY = 5, Type = DeskType.RegularDesk, BuildingId = 1 },

                    // Row 4: Creative zone
                    new Desk { Id = 116, DeskNumber = "D1", Description = "Creative hub - Whiteboard access", PositionX = 1, PositionY = 7, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 117, DeskNumber = "D2", Description = "Creative hub - Brainstorm area", PositionX = 2, PositionY = 7, Type = DeskType.RegularDesk, BuildingId = 1, IsInMaintenance = true, MaintenanceReason = "Ergonomic chair replacement scheduled" },
                    new Desk { Id = 118, DeskNumber = "D3", Description = "Near coffee station", PositionX = 3, PositionY = 7, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 119, DeskNumber = "D4", Description = "Standing desk - Height adjustable", PositionX = 4, PositionY = 7, Type = DeskType.RegularDesk, BuildingId = 1, IsInMaintenance = true, MaintenanceReason = "Monitor arm installation in progress" },

                    // Conference rooms
                    new Desk { Id = 150, DeskNumber = "CR-A", Description = "Executive Conference - Seats 12", PositionX = 8, PositionY = 1, Type = DeskType.ConferenceRoom, BuildingId = 1 },
                    new Desk { Id = 151, DeskNumber = "CR-B", Description = "Board Room - Video conferencing", PositionX = 11, PositionY = 1, Type = DeskType.ConferenceRoom, BuildingId = 1, IsInMaintenance = true, MaintenanceReason = "AV system upgrade - Expected completion Friday" },
                    new Desk { Id = 152, DeskNumber = "CR-C", Description = "Team Meeting Room - Seats 6", PositionX = 8, PositionY = 5, Type = DeskType.ConferenceRoom, BuildingId = 1 },
                    new Desk { Id = 153, DeskNumber = "CR-D", Description = "Huddle Room - Seats 4", PositionX = 11, PositionY = 5, Type = DeskType.ConferenceRoom, BuildingId = 1 },
                }
            },
            new Building
            {
                Id = 2,
                Name = "Innovation Hub - Tech Campus",
                FloorPlanWidth = 10,  // Smaller building
                FloorPlanHeight = 8,
                Desks = new List<Desk>
                {
                    // Focus pods
                    new Desk { Id = 201, DeskNumber = "TH-1", Description = "Focus pod - Soundproof booth", PositionX = 1, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 2 },
                    new Desk { Id = 202, DeskNumber = "TH-2", Description = "Focus pod - Private workspace", PositionX = 2, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 2, IsInMaintenance = true, MaintenanceReason = "Soundproofing panel repair" },
                    new Desk { Id = 203, DeskNumber = "TH-3", Description = "Focus pod - Deep work station", PositionX = 3, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 2 },

                    // Open workspace
                    new Desk { Id = 204, DeskNumber = "TH-4", Description = "Open desk - Garden view", PositionX = 1, PositionY = 3, Type = DeskType.RegularDesk, BuildingId = 2 },
                    new Desk { Id = 205, DeskNumber = "TH-5", Description = "Open desk - Collaborative space", PositionX = 2, PositionY = 3, Type = DeskType.RegularDesk, BuildingId = 2 },
                    new Desk { Id = 206, DeskNumber = "TH-6", Description = "Standing desk - Adjustable height", PositionX = 3, PositionY = 3, Type = DeskType.RegularDesk, BuildingId = 2, IsInMaintenance = true, MaintenanceReason = "Height adjustment mechanism jammed - Parts ordered" },
                    new Desk { Id = 207, DeskNumber = "TH-7", Description = "Hot desk - Near lounge area", PositionX = 1, PositionY = 5, Type = DeskType.RegularDesk, BuildingId = 2 },
                    new Desk { Id = 208, DeskNumber = "TH-8", Description = "Hot desk - Window seat", PositionX = 2, PositionY = 5, Type = DeskType.RegularDesk, BuildingId = 2 },

                    // Conference rooms
                    new Desk { Id = 250, DeskNumber = "TH-CR1", Description = "Innovation Lab - Whiteboard walls", PositionX = 6, PositionY = 2, Type = DeskType.ConferenceRoom, BuildingId = 2 },
                    new Desk { Id = 251, DeskNumber = "TH-CR2", Description = "Video Call Room - Zoom ready", PositionX = 6, PositionY = 5, Type = DeskType.ConferenceRoom, BuildingId = 2 },
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
                // Monday-Friday:
                for (int day = 1; day <= 5; day++)  // 1=Monday, 5=Friday
                {
                    hours.Add(new OperatingHours
                    {
                        BuildingId = building.Id,
                        DayOfWeek = (DayOfWeek)day,
                        IsClosed = false
                    });
                }

                // Saturday:
                hours.Add(new OperatingHours
                {
                    BuildingId = building.Id,
                    DayOfWeek = DayOfWeek.Saturday,
                    IsClosed = false
                });

                // Sunday:
                hours.Add(new OperatingHours
                {
                    BuildingId = building.Id,
                    DayOfWeek = DayOfWeek.Sunday,
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
                        IsClosed = false
                    });
                }

                // Weekend: Closed
                hours.Add(new OperatingHours
                {
                    BuildingId = building.Id,
                    DayOfWeek = DayOfWeek.Saturday,
                    IsClosed = true
                });

                hours.Add(new OperatingHours
                {
                    BuildingId = building.Id,
                    DayOfWeek = DayOfWeek.Sunday,
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
            new User
            {
                Id = "U1",
                Name = "Sarah",
                Surname = "Johnson",
                Password = "sarah123",
                Email = "sarah@company.com",
                Role = UserRole.Admin,
            },
            new User
            {
                Id = "U2",
                Name = "Michael",
                Surname = "Chen",
                Password = "michael123",
                Email = "michael@company.com",
                Role = UserRole.User
            },
            new User
            {
                Id = "U3",
                Name = "Emily",
                Surname = "Rodriguez",
                Password = "EmilyR@2024",
                Email = "emily.rodriguez@company.com",
                Role = UserRole.User
            },
            new User
            {
                Id = "U4",
                Name = "James",
                Surname = "Williams",
                Password = "JamesW!2024",
                Email = "james.williams@company.com",
                Role = UserRole.User
            },
            new User
            {
                Id = "U5",
                Name = "Olivia",
                Surname = "Martinez",
                Password = "OliviaM@2024",
                Email = "olivia.martinez@company.com",
                Role = UserRole.User
            },
            new User
            {
                Id = "U6",
                Name = "David",
                Surname = "Anderson",
                Password = "DavidA#2024",
                Email = "david.anderson@company.com",
                Role = UserRole.User
            },
            new User
            {
                Id = "U7",
                Name = "Sophia",
                Surname = "Taylor",
                Password = "SophiaT@2024",
                Email = "sophia.taylor@company.com",
                Role = UserRole.User
            },
            new User
            {
                Id = "U8",
                Name = "Daniel",
                Surname = "Brown",
                Password = "DanielB!2024",
                Email = "daniel.brown@company.com",
                Role = UserRole.User
            }
        };
    }
}