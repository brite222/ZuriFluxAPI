using Microsoft.EntityFrameworkCore;
using ZuriFluxAPI.Models;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Define relationships clearly
        modelBuilder.Entity<SensorReading>()
            .HasOne(s => s.Bin)
            .WithMany(b => b.SensorReadings)
            .HasForeignKey(s => s.BinId);

        modelBuilder.Entity<WasteCollection>()
            .HasOne(w => w.Bin)
            .WithMany()
            .HasForeignKey(w => w.BinId);
    }
}