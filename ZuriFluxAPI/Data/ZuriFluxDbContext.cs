using Microsoft.EntityFrameworkCore;
using ZuriFluxAPI.Models;

namespace ZuriFluxAPI.Data;
public class ZuriFluxDbContext : DbContext
{
    public ZuriFluxDbContext(DbContextOptions<ZuriFluxDbContext> options)
        : base(options) { }

    // Each DbSet = one table in your database
    public DbSet<Bin> Bins { get; set; }
    public DbSet<SensorReading> SensorReadings { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<WasteCollection> WasteCollections { get; set; }
    public DbSet<CreditTransaction> CreditTransactions { get; set; }
    public DbSet<CollectionSchedule> CollectionSchedules { get; set; }
    public DbSet<DeviceToken> DeviceTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Existing relationships
        modelBuilder.Entity<SensorReading>()
            .HasOne(s => s.Bin)
            .WithMany(b => b.SensorReadings)
            .HasForeignKey(s => s.BinId);

        modelBuilder.Entity<WasteCollection>()
            .HasOne(w => w.Bin)
            .WithMany()
            .HasForeignKey(w => w.BinId);

        // Fix for self-referencing User table
        modelBuilder.Entity<User>()
            .HasOne(u => u.ReferredBy)
            .WithMany()
            .HasForeignKey(u => u.ReferredByUserId)
            .OnDelete(DeleteBehavior.NoAction);  // ← this fixes the error
                                                 // Collection Schedule relationships
        modelBuilder.Entity<CollectionSchedule>()
            .HasOne(cs => cs.Bin)
            .WithMany()
            .HasForeignKey(cs => cs.BinId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<CollectionSchedule>()
            .HasOne(cs => cs.RequestedBy)
            .WithMany()
            .HasForeignKey(cs => cs.RequestedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<CollectionSchedule>()
            .HasOne(cs => cs.AssignedCollector)
            .WithMany()
            .HasForeignKey(cs => cs.AssignedCollectorId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<DeviceToken>()
        .HasOne(d => d.User)
        .WithMany()
        .HasForeignKey(d => d.UserId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}