using Microsoft.EntityFrameworkCore;
using NagerHolidays.Models;

namespace NagerHolidays.Data;

public class HolidayDbContext:DbContext
{
    public HolidayDbContext(DbContextOptions<HolidayDbContext> options) : base(options)
    {
        
    }

    public DbSet<Holiday> Holidays { get; set; } = null;
    public DbSet<Country> Countries {get; set;} = null;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Country>(b =>
        {
            b.HasIndex(c => c.CountryCode).IsUnique();
            
            b.Property(c => c.CountryCode).HasMaxLength(8).IsRequired();
            b.Property(c => c.Name).HasMaxLength(128).IsRequired();
        });
        modelBuilder.Entity<Holiday>(b =>
        {
            //most common filter pattern identified, maybe wrong??
            b.HasIndex(h => new { h.CountryCode, h.Year });
            b.HasIndex(h => h.Date);
            
            b.Property(h => h.Name).HasMaxLength(256).IsRequired();
            b.Property(h => h.LocalName).HasMaxLength(256).IsRequired();
            b.Property(h => h.CountryCode).HasMaxLength(8).IsRequired();
            b.Property(h => h.Counties).HasMaxLength(256).IsRequired(false);
            b.Property(h => h.Types).HasMaxLength(256).IsRequired();
            
            b.Property(h => h.Date).HasColumnType("date"); //tech no need for time component
            b.HasOne(h => h.Country)
                .WithMany(c => c.Holidays)
                .HasForeignKey(h => h.CountryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}