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
    public DbSet<Language> Languages { get; set; }
    public DbSet<ExpertLanguage> ExpertLanguages { get; set; }
    public DbSet<Timeslot> Timeslots { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<BookingCancellation> BookingCancellations { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Specialization> Specializations { get; set; }

    public DbSet<SocialMediaPlatform> SocialMediaPlatforms { get; set; }
    public DbSet<ExpertSocialLink> ExpertSocialLinks { get; set; }
    public DbSet<FavoriteExpert> FavoriteExperts { get; set; }
    public DbSet<BlockchainData> BlockchainData { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // === USER ===
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<User>()
            .Property(u => u.Rating)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<User>()
            .Property(u => u.DiscordId)
            .HasMaxLength(100)
            .IsRequired(false);

        // Зв'язок Many-to-Many для спеціалізацій
        modelBuilder.Entity<User>()
            .HasMany(u => u.Specializations)
            .WithMany(s => s.Experts)
            .UsingEntity(j => j.ToTable("ExpertSpecializations"));


        // === USER SETTINGS ===
        modelBuilder.Entity<UserSettings>(entity =>
        {
            entity.ToTable("UserSettings");
            entity.HasKey(s => s.Id);

            // 1:1
            entity.HasOne(s => s.User)
                  .WithOne(u => u.Settings)
                  .HasForeignKey<UserSettings>(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade); 
        });

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

        // === LANGUAGE ===
        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Code).IsRequired().HasMaxLength(10);
            entity.HasIndex(l => l.Code).IsUnique(); 
        });

        // === EXPERT LANGUAGE ===
        modelBuilder.Entity<ExpertLanguage>(entity =>
        {
            entity.ToTable("ExpertLanguages");

            entity.HasKey(el => new { el.ExpertId, el.LanguageId });

            entity.Property(el => el.Level).IsRequired().HasMaxLength(50);

            entity.HasOne(el => el.Expert)
                  .WithMany(u => u.ExpertLanguages)
                  .HasForeignKey(el => el.ExpertId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(el => el.Language)
                  .WithMany(l => l.ExpertLanguages)
                  .HasForeignKey(el => el.LanguageId)
                  .OnDelete(DeleteBehavior.Restrict);
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

            entity.Property(b => b.ContractAddress)
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
                  .WithMany(ts => ts.Bookings)
                  .HasForeignKey(b => b.TimeslotId)
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

        // === BLOCKCHAIN DATA ===
        modelBuilder.Entity<BlockchainData>(entity =>
        {
            entity.ToTable("BlockchainData", t =>
            {
                // SQL перевірка: Або Адреса не NULL, або Мнемоніка не NULL
                t.HasCheckConstraint("CK_BlockchainData_AddressOrMnemonic", "[Address] IS NOT NULL OR [Mnemonic] IS NOT NULL");
            });

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique(); 

            entity.Property(e => e.Address).HasMaxLength(150).IsRequired(false);
            entity.Property(e => e.Mnemonic).HasMaxLength(500).IsRequired(false);
        });
    }
}