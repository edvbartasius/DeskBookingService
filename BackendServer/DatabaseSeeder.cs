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
                    new Desk { Id = 101, Description = "Window desk 1", PositionX = 1, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 102, Description = "Window desk 2", PositionX = 2, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 103, Description = "Window desk 3", PositionX = 3, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 104, Description = "Window desk 4", PositionX = 4, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 105, Description = "Window desk 5", PositionX = 5, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 1 },

                    // Row 2: Regular desks
                    new Desk { Id = 106, PositionX = 1, PositionY = 3, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 107, PositionX = 2, PositionY = 3, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 108, PositionX = 3, PositionY = 3, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 109, PositionX = 4, PositionY = 3, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 110, PositionX = 5, PositionY = 3, Type = DeskType.RegularDesk, BuildingId = 1 },

                    // Row 3: Regular desks
                    new Desk { Id = 111, Description = "Quiet area", PositionX = 1, PositionY = 5, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 112, Description = "Quiet area", PositionX = 2, PositionY = 5, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 113, Description = "Quiet area", PositionX = 3, PositionY = 5, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 114, Description = "Quiet area", PositionX = 4, PositionY = 5, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 115, Description = "Quiet area", PositionX = 5, PositionY = 5, Type = DeskType.RegularDesk, BuildingId = 1 },

                    // Bottom row desks
                    new Desk { Id = 116, PositionX = 1, PositionY = 8, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 117, PositionX = 2, PositionY = 8, Type = DeskType.RegularDesk, BuildingId = 1 },
                    new Desk { Id = 118, PositionX = 3, PositionY = 8, Type = DeskType.RegularDesk, BuildingId = 1 },

                    // Conference rooms (larger - takes up more cells)
                    new Desk { Id = 150, Description = "Conference Room A", PositionX = 8, PositionY = 1, Type = DeskType.ConferenceRoom, BuildingId = 1 },
                    new Desk { Id = 151, Description = "Conference Room B", PositionX = 11, PositionY = 1, Type = DeskType.ConferenceRoom, BuildingId = 1 },
                    new Desk { Id = 152, Description = "Meeting Room", PositionX = 8, PositionY = 5, Type = DeskType.ConferenceRoom, BuildingId = 1 },
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
                    new Desk { Id = 201, Description = "Focus desk 1", PositionX = 1, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 2 },
                    new Desk { Id = 202, Description = "Focus desk 2", PositionX = 2, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 2 },
                    new Desk { Id = 203, Description = "Focus desk 3", PositionX = 3, PositionY = 1, Type = DeskType.RegularDesk, BuildingId = 2 },
                    new Desk { Id = 204, Description = "Work desk 1", PositionX = 1, PositionY = 3, Type = DeskType.RegularDesk, BuildingId = 2 },
                    new Desk { Id = 205, Description = "Work desk 2", PositionX = 2, PositionY = 3, Type = DeskType.RegularDesk, BuildingId = 2 },

                    // Small conference room
                    new Desk { Id = 250, Description = "Small Meeting Room", PositionX = 6, PositionY = 2, Type = DeskType.ConferenceRoom, BuildingId = 2 },
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

        return new List<Reservation>
        {
            // Example 1: User 2 books desk 101 for tomorrow
            new Reservation
            {
                Id = 1,
                DeskId = 101,
                UserId = "U2",
                ReservationDate = tomorrow,
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow
            },

            // Example 2: User 2 books desk 101 for day after tomorrow (multi-day booking)
            new Reservation
            {
                Id = 2,
                DeskId = 101,
                UserId = "U2",
                ReservationDate = dayAfterTomorrow,
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow
            },

            // Example 3: User 3 books conference room for tomorrow
            new Reservation
            {
                Id = 3,
                DeskId = 150,
                UserId = "U3",
                ReservationDate = tomorrow,
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow
            },

            // Example 4: User 4 books desk 106 for three days from now
            new Reservation
            {
                Id = 4,
                DeskId = 106,
                UserId = "U4",
                ReservationDate = threeDaysFromNow,
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow
            },

            // Example 5: User 4 books desk 106 for four days from now (consecutive booking)
            new Reservation
            {
                Id = 5,
                DeskId = 106,
                UserId = "U4",
                ReservationDate = fourDaysFromNow,
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow
            },

            // Example 6: User 5 books desk 107 for tomorrow
            new Reservation
            {
                Id = 6,
                DeskId = 107,
                UserId = "U5",
                ReservationDate = tomorrow,
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow
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