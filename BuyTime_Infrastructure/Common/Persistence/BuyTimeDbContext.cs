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
    public DbSet<Teacher> Teachers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<User>()
        .HasKey(u => u.Id);

    modelBuilder.Entity<User>()
        .HasMany(u => u.Feedbacks)
        .WithOne(f => f.User)
        .HasForeignKey(f => f.UserId)
        .OnDelete(DeleteBehavior.Cascade); 

    modelBuilder.Entity<User>()
        .HasDiscriminator<string>("Role") 
        .HasValue<User>("student")
        .HasValue<Teacher>("teacher");

    modelBuilder.Entity<Teacher>()
        .HasMany(t => t.TimeSlots)
        .WithOne(ts => ts.Teacher)
        .HasForeignKey(ts => ts.TeacherId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Teacher>()
        .HasMany(t => t.Bookings)
        .WithOne(b => b.Teacher)
        .HasForeignKey(b => b.TeacherId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Timeslot>()
        .HasKey(ts => ts.Id);

    modelBuilder.Entity<Timeslot>()
        .HasOne(ts => ts.Teacher)
        .WithMany(t => t.TimeSlots)
        .HasForeignKey(ts => ts.TeacherId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Feedback>()
        .HasKey(f => f.Id);

    modelBuilder.Entity<Feedback>()
        .HasOne(f => f.Teacher)
        .WithMany()
        .HasForeignKey(f => f.TeacherId)
        .OnDelete(DeleteBehavior.Restrict); 

    modelBuilder.Entity<Feedback>()
        .HasOne(f => f.User)
        .WithMany()
        .HasForeignKey(f => f.UserId)
        .OnDelete(DeleteBehavior.Restrict); 


    modelBuilder.Entity<Booking>()
        .HasKey(b => b.Id);

    modelBuilder.Entity<Booking>()
        .HasOne(b => b.User)
        .WithMany()
        .HasForeignKey(b => b.UserId)
        .OnDelete(DeleteBehavior.Restrict); 

    modelBuilder.Entity<Booking>()
        .HasOne(b => b.Teacher)
        .WithMany(t => t.Bookings)
        .HasForeignKey(b => b.TeacherId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Booking>()
        .HasOne(b => b.TimeSlot)
        .WithMany()
        .HasForeignKey(b => b.TimeslotId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Timeslot>()
        .Property(ts => ts.IsAvailable)
        .HasDefaultValue(true);
    }
}