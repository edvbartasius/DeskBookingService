using DeskBookingService.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Desk> Desks { get; set; }
    public DbSet<Building> Buildings { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<ReservationTimeSpan> ReservationTimeSpans { get; set;}
    public DbSet<OperatingHours> OperatingHours { get; set;}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseInMemoryDatabase("DeskBookingDb");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User.Id to be generated on add (GUID will be generated in code)
        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .ValueGeneratedOnAdd();

        // Reservation -> User
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.User)
            .WithMany()  
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);  // Don't cascade delete

        // Reservation -> Desk
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Desk)
            .WithMany(d => d.Reservations)
            .HasForeignKey(r => r.DeskId)
            .OnDelete(DeleteBehavior.Cascade);  // Delete reservations if desk deleted

        // ReservationTimeSpan -> Reservation
        modelBuilder.Entity<ReservationTimeSpan>()
            .HasOne(ts => ts.Reservation)
            .WithMany(r => r.TimeSpans)
            .HasForeignKey(ts => ts.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);  // Delete timespans if reservation deleted

        // OperatingHourse -> Building
        modelBuilder.Entity<OperatingHours>()
            .HasOne(oh => oh.Building)
            .WithMany(b => b.OperatingHours)
            .HasForeignKey(oh => oh.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);

        // One record per building per day
        modelBuilder.Entity<OperatingHours>()
            .HasIndex(oh => new { oh.BuildingId, oh.DayOfWeek })
            .IsUnique();
        
    }

    public override int SaveChanges()
    {
        GenerateIds();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        GenerateIds();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void GenerateIds()
    {
        var addedUsers = ChangeTracker.Entries<User>()
            .Where(e => e.State == EntityState.Added && string.IsNullOrEmpty(e.Entity.Id));

        foreach (var entry in addedUsers)
        {
            entry.Entity.Id = Guid.NewGuid().ToString();
        }
    }
}