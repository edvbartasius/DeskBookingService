namespace DeskBookingService.DatabaseSeeder;

using DeskBookingService.Models;

public static class DatabaseSeeder
{
    public static void Seed(AppDbContext dbContext)
    {
        dbContext.Database.EnsureCreated();

        dbContext.Buildings.AddRange(
            GetPredefinedBuildings()
        );

        dbContext.Users.AddRange(
            GetPredefinedUsers()
        );
    
        dbContext.Reservations.AddRange(
            GetPredefinedReservations()
        );

        dbContext.SaveChanges();
    }
    private static List<Building> GetPredefinedBuildings()
    {
        return new List<Building>
        {
            new Building
            {
                Name = "Main Office",
                Desks = new List<Desk>
                {
                    new Desk { Id = 101, Description = "Near window", Status = DeskStatus.Available },
                    new Desk { Id = 102, Description = "Corner desk", Status = DeskStatus.Booked },
                    new Desk { Id = 103, Description = "Quiet area", Status = DeskStatus.Unavailable },
                }
            },
            new Building
            {
                Name = "Annex Building",
                Desks = new List<Desk>
                {
                    new Desk { Id = 104, Description = "Quiet area", Status = DeskStatus.Unavailable },
                    new Desk { Id = 105, Description = "Next to kitchen", Status = DeskStatus.Available }
                }
            }
        };
    }
    private static List<Reservation> GetPredefinedReservations()
    {
        return new List<Reservation>
        {
            new Reservation
            {
                Id = 1,
                DeskId = 102,
                UserId = "U1",
                ReservationDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)), // Tomorrow
                StartDate = new TimeOnly(10,0),
                EndDate = new TimeOnly(15,0)
            },
            new Reservation
            {
                Id = 2,
                DeskId = 103,
                UserId = "U2",
                ReservationDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(2)), // Day after tomorrow
                StartDate = new TimeOnly(9,0),
                EndDate = new TimeOnly(17,0)
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