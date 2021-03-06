﻿// <auto-generated />
using System;
using AccountMicroservice.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AccountMicroservice.Data.Migrations
{
    [DbContext(typeof(MyDbContext))]
    [Migration("20190603171159_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("AccountMicroservice.Data.Models.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AvatarImageUrl");

                    b.Property<int>("CallingCountryCode");

                    b.Property<string>("CoverImageUrl");

                    b.Property<string>("DataSpaceDirName");

                    b.Property<DateTime>("DateRegistered");

                    b.Property<string>("Email");

                    b.Property<string>("Firstname");

                    b.Property<string>("Lastname");

                    b.Property<string>("PhoneNumber")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("CallingCountryCode");

                    b.HasIndex("PhoneNumber")
                        .IsUnique();

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("AccountMicroservice.Data.Models.AuthToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccountId");

                    b.Property<DateTime>("DateGenerated");

                    b.Property<string>("Ip");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.ToTable("AuthTokens");
                });

            modelBuilder.Entity("AccountMicroservice.Data.Models.CallingCode", b =>
                {
                    b.Property<int>("CallingCountryCode");

                    b.Property<string>("CountryName");

                    b.Property<string>("IsoCode");

                    b.HasKey("CallingCountryCode");

                    b.ToTable("CallingCode");
                });

            modelBuilder.Entity("AccountMicroservice.Data.Models.Contact", b =>
                {
                    b.Property<int>("AccountId");

                    b.Property<int>("ContactAccountId");

                    b.Property<string>("ContactName");

                    b.Property<DateTime>("DateAdded");

                    b.Property<bool>("IsFavorite");

                    b.HasKey("AccountId", "ContactAccountId");

                    b.HasIndex("ContactAccountId");

                    b.ToTable("Contacts");
                });

            modelBuilder.Entity("AccountMicroservice.Data.Models.Account", b =>
                {
                    b.HasOne("AccountMicroservice.Data.Models.CallingCode", "CallingCodeObj")
                        .WithMany()
                        .HasForeignKey("CallingCountryCode")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("AccountMicroservice.Data.Models.AuthToken", b =>
                {
                    b.HasOne("AccountMicroservice.Data.Models.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("AccountMicroservice.Data.Models.Contact", b =>
                {
                    b.HasOne("AccountMicroservice.Data.Models.Account", "Account")
                        .WithMany("Contacts")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("AccountMicroservice.Data.Models.Account", "ContactAccount")
                        .WithMany()
                        .HasForeignKey("ContactAccountId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
