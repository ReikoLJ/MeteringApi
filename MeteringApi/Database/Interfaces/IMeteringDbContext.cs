using MeteringApi.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeteringApi.Database.Interfaces
{
    public interface IMeteringDbContext
    {
        DbSet<Entity_Account> Accounts { get; set; }
        DbSet<Entity_MeterReading> MeterReadings { get; set; }

        Task<int> SaveChangesAsync();
    }
}