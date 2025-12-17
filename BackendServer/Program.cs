using DeskBookingService.DatabaseSeeder;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>();
// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed the database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DatabaseSeeder.Seed(dbContext);

    // Verify seeding via console output temporarily
    var users = dbContext.Users.ToList();
    Console.WriteLine("Seeded Users:");
    foreach (var user in users)
    {
        Console.WriteLine($"- {user.Id}: {user.Name} {user.Surname} ({user.Role})");
    }
    var buildings = dbContext.Buildings.Include(b => b.Desks).ToList();
    Console.WriteLine("Seeded Buildings and Desks:");
    foreach (var building in buildings)
    {
        Console.WriteLine($"- Building: {building.Name}");
        foreach (var desk in building.Desks)
        {
            Console.WriteLine($"  - Desk {desk.Id}: {desk.Description} ({desk.Status})");
        }
    }
    var reservations = dbContext.Reservations.ToList();
    Console.WriteLine("Seeded Reservations:");
    foreach (var reservation in reservations)
    {
        Console.WriteLine($"- Reservation {reservation.Id}: Desk {reservation.DeskId} by User {reservation.UserId} on {reservation.ReservationDate} from {reservation.StartDate} to {reservation.EndDate}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
