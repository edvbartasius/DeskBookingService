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