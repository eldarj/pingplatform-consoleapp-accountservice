using AccountMicroservice.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace AccountMicroservice.Data
{
    public class MyDbContext : DbContext
    {
        //public MyDbContext(DbContextOptions<MyDbContext> dbContextOptions) : base(dbContextOptions) { }

        #region DbSets
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AuthToken> AuthTokens { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<CallingCode> CallingCode { get; set; }
        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // TODO: Add this in appsettings or ENV (dev, prod) vars
            optionsBuilder.UseMySql("server=localhost;database=PingAccountServiceDb;user=root;password=",
                a => a.MigrationsAssembly("AccountMicroservice.Data"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>()
                .HasIndex(a => a.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<Contact>()
                .HasKey(c => new { c.AccountId, c.ContactAccountId });
        }
    }
}
