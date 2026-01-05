using BuyTime_Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuyTime_Infrastructure.Common.Persistence;

public class BuyTimeDbContext : DbContext
{
    public BuyTimeDbContext() : base()
    {
    }

    public BuyTimeDbContext(DbContextOptions<BuyTimeDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Timeslot> Timeslots { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    public DbSet<BookingCancellation> BookingCancellations { get; set; }

    public DbSet<Wallet> Wallets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // === USER ===
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Feedbacks)
            .WithOne(f => f.User)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // === TIMESLOT ===
        modelBuilder.Entity<Timeslot>()
            .HasKey(ts => ts.Id);

        modelBuilder.Entity<Timeslot>()
            .HasOne(ts => ts.Expert)
            .WithMany(u => u.TimeSlots)
            .HasForeignKey(ts => ts.ExpertId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Timeslot>()
            .Property(ts => ts.IsAvailable)
            .HasDefaultValue(true);

        modelBuilder.Entity<Timeslot>()
            .Property(ts => ts.Price)
            .HasColumnType("decimal(18,2)");

        // === FEEDBACK ===
        modelBuilder.Entity<Feedback>()
            .HasKey(f => f.Id);

        modelBuilder.Entity<Feedback>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // === BOOKING ===
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(b => b.Id);

            entity.Property(b => b.ContractHash)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(b => b.MeetingLink) // винести це в таблицю confirmedBookings
                  .IsRequired(false);

            // Booking -> Cancellation (1:0..1)
            entity.HasOne(b => b.Cancellation)
                  .WithOne(bc => bc.Booking)
                  .HasForeignKey<BookingCancellation>(bc => bc.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.Student)
                  .WithMany(u => u.Bookings)
                  .HasForeignKey(b => b.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.TimeSlot)
                  .WithMany()
                  .HasForeignKey(b => b.TimeslotId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // === BOOKING CANCELLATION ===
        modelBuilder.Entity<BookingCancellation>()
            .HasKey(bc => bc.BookingId);

        // === WALLET ===
        modelBuilder.Entity<Wallet>()
            .HasKey(w => w.Id);

        modelBuilder.Entity<Wallet>()
            .HasOne(w => w.User)
            .WithMany(u => u.Wallets)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}