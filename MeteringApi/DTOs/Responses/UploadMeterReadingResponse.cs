namespace MeteringApi.DTOs.Responses
{
    public class UploadMeterReadingResponse
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}