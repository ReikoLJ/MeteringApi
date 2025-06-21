using MeteringApi.DTOs.Responses;

namespace MeteringApi.Services.Interfaces
{
    public interface IMeteringService
    {
        Task<UploadResponse> SaveMeterReadingsAsync(IFormFile meterReadingsFile);
    }
}