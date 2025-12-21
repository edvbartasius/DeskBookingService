using DeskBookingService.DatabaseSeeder;
using DeskBookingService.Configurations;
using DeskBookingService.Services;
using Mapster;
using FluentValidation;

// Load .env file
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure Mapster
MapsterConfiguration.Configure();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddDbContext<AppDbContext>();

// Register application services
builder.Services.AddScoped<DeskAvailabilityService>();
builder.Services.AddScoped<ReservationValidationService>();

// Register FluentValidation validators
builder.Services.AddScoped<IValidator<DeskBookingService.Models.User>, UserValidator>();
builder.Services.AddScoped<IValidator<DeskBookingService.Models.Building>, BuildingValidator>();
builder.Services.AddScoped<IValidator<DeskBookingService.Models.Desk>, DeskValidator>();
builder.Services.AddScoped<IValidator<DeskBookingService.Models.Reservation>, ReservationValidator>();

builder.Services.AddMapster();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS policy
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:3000";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(frontendUrl)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Seed the database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DatabaseSeeder.Seed(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();

app.Run();
