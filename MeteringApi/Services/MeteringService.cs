using System.Globalization;
using CsvHelper;
using MeteringApi.Database.Entities;
using MeteringApi.Database.Interfaces;
using MeteringApi.DomainModels;
using MeteringApi.DomainModels.ClassMaps;
using MeteringApi.DTOs.Responses;
using MeteringApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeteringApi.Services
{
    public class MeteringService : IMeteringService
    {
        private readonly IMeteringDbContext _meteringDbContext;

        public MeteringService(IMeteringDbContext meteringDbContext)
        {
            _meteringDbContext = meteringDbContext;
        }

        public async Task<UploadMeterReadingResponse> SaveMeterReadingsAsync(IFormFile meterReadingsFile)
        {
            var records = ExtractRecordsFromCSV(meterReadingsFile);

            var accountIds = _meteringDbContext.Accounts
                .AsNoTracking()
                .Select(a => a.Id).ToHashSet();

            var existingReadings = _meteringDbContext.MeterReadings
                .AsNoTracking()
                .Select(r => new ValueTuple<int, DateTime>(r.AccountId, r.ReadingDatetime))
                .ToHashSet();

            var validReadings = new List<Entity_MeterReading>();
            var processedReadings = new HashSet<(int, DateTime)>();

            var dto = new UploadMeterReadingResponse();

            foreach (var record in records)
            {
                if (!accountIds.Any(a => a == record.AccountId))
                {
                    dto.Errors.Add($"Invalid AccountId: {record.AccountId}");
                    dto.FailureCount++;
                    continue;
                }

                if (record.MeterReadValue < 0 || record.MeterReadValue > 99999)
                {
                    dto.Errors.Add($"Invalid meter read value: {record.MeterReadValue} for AccountId {record.AccountId}");
                    dto.FailureCount++;
                    continue;
                }

                var recordKey = (record.AccountId, record.MeterReadingDateTime);

                if (existingReadings.Contains(recordKey) || !processedReadings.Add(recordKey))
                {
                    dto.Errors.Add($"Duplicate entry: Account {record.AccountId} at {record.MeterReadingDateTime}");
                    dto.FailureCount++;
                    continue;
                }

                validReadings.Add(new Entity_MeterReading
                {
                    AccountId = record.AccountId,
                    ReadingDatetime = record.MeterReadingDateTime,
                    ReadValue = record.MeterReadValue
                });
                dto.SuccessCount++;

            }

            if (validReadings.Any())
            {
                _meteringDbContext.MeterReadings.AddRange(validReadings);

                await _meteringDbContext.SaveChangesAsync();
            }

            return dto;
        }

        private static List<MeterReading> ExtractRecordsFromCSV(IFormFile meterReadingsFile)
        {
            using var stream = meterReadingsFile.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<MeterReadingMap>();

            var records = new List<MeterReading>();

            try
            {
                records = csv.GetRecords<MeterReading>().ToList();
            }
            catch(Exception ex)
            {
                throw new InvalidDataException("CSV could not be parsed, please check required format in documentation", ex);
            }

            return records;
        }
    }
}