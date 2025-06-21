using MeteringApi.DTOs.Responses;

namespace MeteringApi.Services.Interfaces
{
    public interface IMeteringService
    {
        Task<UploadMeterReadingResponse> SaveMeterReadingsAsync(IFormFile meterReadingsFile);
    }
}