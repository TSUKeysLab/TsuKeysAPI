﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using tsuKeysAPIProject.DBContext;

#nullable disable

namespace tsuKeysAPIProject.Migrations
{
    [DbContext(typeof(AppDBContext))]
    partial class AppDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("tsuKeysAPIProject.DBContext.Models.BlackToken", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Blacktoken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("BlackTokens");
                });

            modelBuilder.Entity("tsuKeysAPIProject.DBContext.Models.Key", b =>
                {
                    b.Property<string>("ClassroomNumber")
                        .HasColumnType("text");

                    b.Property<string>("Owner")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ClassroomNumber");

                    b.ToTable("Keys");
                });

            modelBuilder.Entity("tsuKeysAPIProject.DBContext.Models.KeyRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ClassroomNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("KeyOwner")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("KeyRecipient")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("KeyRequest");
                });

            modelBuilder.Entity("tsuKeysAPIProject.DBContext.Models.Request", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ClassroomNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateOnly>("DateOfBooking")
                        .HasColumnType("date");

                    b.Property<DateTime>("DateOfSent")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeOnly>("EndTime")
                        .HasColumnType("time without time zone");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uuid");

                    b.Property<string>("RequestOwner")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<TimeOnly>("StartTime")
                        .HasColumnType("time without time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<Guid>("TimeId")
                        .HasColumnType("uuid");

                    b.Property<int>("ownerRole")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Requests");
                });

            modelBuilder.Entity("tsuKeysAPIProject.DBContext.Models.TimeSlot", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<TimeOnly>("EndTime")
                        .HasColumnType("time without time zone");

                    b.Property<TimeOnly>("StartTime")
                        .HasColumnType("time without time zone");

                    b.HasKey("Id");

                    b.ToTable("TimeSlots");
                });

            modelBuilder.Entity("tsuKeysAPIProject.DBContext.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateOnly>("BirthDate")
                        .HasColumnType("date");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Fullname")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Gender")
                        .HasColumnType("integer");

                    b.Property<string>("Lastname")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Email");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
