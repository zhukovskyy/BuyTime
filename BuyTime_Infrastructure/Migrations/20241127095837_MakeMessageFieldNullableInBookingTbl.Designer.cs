﻿// <auto-generated />
using System;
using BuyTime_Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BuyTime_Infrastructure.Migrations
{
    [DbContext(typeof(BuyTimeDbContext))]
    [Migration("20241127095837_MakeMessageFieldNullableInBookingTbl")]
    partial class MakeMessageFieldNullableInBookingTbl
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BuyTime_Domain.Entities.Booking", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("TeacherId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("TimeslotId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("TeacherId");

                    b.HasIndex("TimeslotId");

                    b.HasIndex("UserId");

                    b.ToTable("Bookings");
                });

            modelBuilder.Entity("BuyTime_Domain.Entities.Feedback", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Comment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("Rating")
                        .HasColumnType("decimal(18,2)");

                    b.Property<Guid>("TeacherId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UserId1")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("TeacherId");

                    b.HasIndex("UserId");

                    b.HasIndex("UserId1");

                    b.ToTable("Feedbacks");
                });

            modelBuilder.Entity("BuyTime_Domain.Entities.Timeslot", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsAvailable")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("TeacherId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("TeacherId");

                    b.ToTable("Timeslots");
                });

            modelBuilder.Entity("BuyTime_Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("nvarchar(8)");

                    b.HasKey("Id");

                    b.ToTable("Users");

                    b.HasDiscriminator<string>("Role").HasValue("student");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("BuyTime_Domain.Entities.Teacher", b =>
                {
                    b.HasBaseType("BuyTime_Domain.Entities.User");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal?>("Rating")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Tags")
                        .HasColumnType("nvarchar(max)");

                    b.HasDiscriminator().HasValue("teacher");
                });

            modelBuilder.Entity("BuyTime_Domain.Entities.Booking", b =>
                {
                    b.HasOne("BuyTime_Domain.Entities.Teacher", "Teacher")
                        .WithMany("Bookings")
                        .HasForeignKey("TeacherId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("BuyTime_Domain.Entities.Timeslot", "TimeSlot")
                        .WithMany()
                        .HasForeignKey("TimeslotId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("BuyTime_Domain.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Teacher");

                    b.Navigation("TimeSlot");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BuyTime_Domain.Entities.Feedback", b =>
                {
                    b.HasOne("BuyTime_Domain.Entities.Teacher", "Teacher")
                        .WithMany()
                        .HasForeignKey("TeacherId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("BuyTime_Domain.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("BuyTime_Domain.Entities.User", null)
                        .WithMany("Feedbacks")
                        .HasForeignKey("UserId1");

                    b.Navigation("Teacher");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BuyTime_Domain.Entities.Timeslot", b =>
                {
                    b.HasOne("BuyTime_Domain.Entities.Teacher", "Teacher")
                        .WithMany("TimeSlots")
                        .HasForeignKey("TeacherId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("BuyTime_Domain.Entities.User", b =>
                {
                    b.Navigation("Feedbacks");
                });

            modelBuilder.Entity("BuyTime_Domain.Entities.Teacher", b =>
                {
                    b.Navigation("Bookings");

                    b.Navigation("TimeSlots");
                });
#pragma warning restore 612, 618
        }
    }
}