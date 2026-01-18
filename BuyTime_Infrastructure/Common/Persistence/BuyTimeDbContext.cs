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
    public DbSet<Specialization> Specializations { get; set; }

    public DbSet<SocialMediaPlatform> SocialMediaPlatforms { get; set; }
    public DbSet<ExpertSocialLink> ExpertSocialLinks { get; set; }
    public DbSet<FavoriteExpert> FavoriteExperts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // === USER ===
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<User>()
            .Property(u => u.Rating)
            .HasColumnType("decimal(18,2)");

        // Зв'язок Many-to-Many для спеціалізацій
        modelBuilder.Entity<User>()
            .HasMany(u => u.Specializations)
            .WithMany(s => s.Experts)
            .UsingEntity(j => j.ToTable("ExpertSpecializations"));

        // === SPECIALIZATION ===
        modelBuilder.Entity<Specialization>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(s => s.Name).IsUnique();
        });

        // === SOCIAL MEDIA PLATFORM ===
        modelBuilder.Entity<SocialMediaPlatform>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(p => p.Name).IsUnique();
            entity.Property(p => p.LogoUrl).HasMaxLength(500);
        });

        // === EXPERT SOCIAL LINK (кол. SocialLink) ===
        modelBuilder.Entity<ExpertSocialLink>(entity =>
        {
            entity.ToTable("ExpertSocialLinks"); // Явна назва таблиці
            entity.HasKey(sl => sl.Id);
            entity.Property(sl => sl.UrlOrHandle).IsRequired().HasMaxLength(200);

            // Зв'язок з Юзером
            entity.HasOne(sl => sl.Expert)
                  .WithMany(u => u.SocialLinks)
                  .HasForeignKey(sl => sl.ExpertId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Зв'язок з Платформою
            entity.HasOne(sl => sl.Platform)
                  .WithMany(p => p.ExpertLinks)
                  .HasForeignKey(sl => sl.PlatformId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

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

        modelBuilder.Entity<Timeslot>()
            .Property(ts => ts.ExpertWalletAddress)
            .HasMaxLength(150)
            .IsRequired(false);

        // === LANGUAGE SKILL ===
        modelBuilder.Entity<LanguageSkill>(entity =>
        {
            entity.HasKey(ls => ls.Id);
            entity.Property(ls => ls.LanguageName).IsRequired().HasMaxLength(50);
            entity.Property(ls => ls.Level).IsRequired().HasMaxLength(50);

            entity.HasOne(ls => ls.User)
                  .WithMany(u => u.LanguageSkills)
                  .HasForeignKey(ls => ls.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // === FEEDBACK ===
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(f => f.Id);

            entity.Property(f => f.Rating)
                  .HasColumnType("decimal(18,2)");

            entity.Property(f => f.Comment)
                  .IsRequired(false)
                  .HasMaxLength(1000);

            entity.HasOne(f => f.Expert)
                  .WithMany(u => u.ReceivedFeedbacks)
                  .HasForeignKey(f => f.ExpertId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.Student)
                  .WithMany()
                  .HasForeignKey(f => f.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // === FAVORITE EXPERT ===
        modelBuilder.Entity<FavoriteExpert>(entity =>
        {
            entity.ToTable("FavoriteExperts");

            entity.HasKey(fe => new { fe.StudentId, fe.ExpertId });

            // Зв'язок зі Студентом (у нього є колекція FavoriteExperts)
            entity.HasOne(fe => fe.Student)
                  .WithMany(u => u.FavoriteExperts)
                  .HasForeignKey(fe => fe.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Зв'язок з Експертом (у нього НЕМАЄ колекції, тому .WithMany() пустий)
            entity.HasOne(fe => fe.Expert)
                  .WithMany()
                  .HasForeignKey(fe => fe.ExpertId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // === BOOKING ===
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(b => b.Id);

            entity.Property(b => b.ContractHash)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(b => b.MeetingLink)
                  .IsRequired(false);

            entity.Property(b => b.StudentWalletAddress)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.HasOne(b => b.Cancellation)
                  .WithOne(bc => bc.Booking)
                  .HasForeignKey<BookingCancellation>(bc => bc.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.Student)
                  .WithMany(u => u.Bookings)
                  .HasForeignKey(b => b.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.TimeSlot)
                  .WithOne(ts => ts.Booking)
                  .HasForeignKey<Booking>(b => b.TimeslotId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // === BOOKING CANCELLATION ===
        modelBuilder.Entity<BookingCancellation>()
            .HasKey(bc => bc.BookingId);

        // === WALLET ===
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Network).IsRequired().HasMaxLength(20);
            entity.Property(w => w.Address).IsRequired().HasMaxLength(150);

            entity.HasOne(w => w.User)
                  .WithMany(u => u.Wallets)
                  .HasForeignKey(w => w.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // один гаманець на одну мережу
            entity.HasIndex(w => new { w.UserId, w.Network }).IsUnique();
        });
    }
}