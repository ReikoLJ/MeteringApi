using System.Diagnostics;
using MeteringApi.Database.Entities;
using MeteringApi.Database.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeteringApi.Database
{
    public partial class MeteringDbContext : DbContext, IMeteringDbContext
    {
        private readonly string _connectionString;

        public DbSet<Entity_Account> Accounts { get; set; }
        public DbSet<Entity_MeterReading> MeterReadings { get; set; }

        public MeteringDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity_Account>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("account_pk");

                entity.ToTable("account", "metering");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");
                entity.Property(e => e.FirstName)
                    .HasColumnType("character varying")
                    .HasColumnName("first_name");
                entity.Property(e => e.LastName)
                    .HasColumnType("character varying")
                    .HasColumnName("last_name");
            });


            modelBuilder.Entity<Entity_MeterReading>(entity =>
            {
                entity.HasKey(e => new { e.AccountId, e.ReadingDatetime }).HasName("meter_reading_pk");

                entity.ToTable("meter_reading", "metering");

                entity.Property(e => e.AccountId).HasColumnName("account_id");
                entity.Property(e => e.ReadingDatetime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("reading_datetime");
                entity.Property(e => e.ReadValue).HasColumnName("read_value");

                entity.HasOne(d => d.Account).WithMany(p => p.MeterReadings)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("meter_reading_account_fk");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_connectionString);
            }

#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.LogTo(Console.WriteLine);
            optionsBuilder.LogTo(message => Debug.WriteLine(message));
#endif
        }

        public async Task<int> SaveChangesAsync()
        {
            var result = await base.SaveChangesAsync();

            return result;
        }
    }
}