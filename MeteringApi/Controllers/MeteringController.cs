using Asp.Versioning;
using MeteringApi.DTOs.Responses;
using MeteringApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeteringApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("")]
    [Produces("application/json")]
    public class MeteringController : ControllerBase
    {
        private readonly IMeteringService _meteringService;

        public MeteringController(IMeteringService meteringService)
        {
            _meteringService = meteringService;
        }

        /// <summary>
        /// Uploads a CSV file containing meter readings
        /// </summary>
        /// <remarks>
        /// The CSV must have the following columns:
        ///
        /// AccountId,MeterReadingDateTime,MeterReadValue
        /// 
        /// The values for these must be types :
        /// 
        /// Integer, Datetime, Integer
        /// 
        /// Example:
        /// 
        /// 1234,21/06/2025 09:24,1002  
        /// 4567,22/07/2025 12:25,0323
        /// </remarks>
        [HttpPost]
        [Route("meter-reading-uploads")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
        [ProducesResponseType(typeof(UploadResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UploadResponse>> UploadMeterReading(IFormFile file)
        {
            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return UnprocessableEntity("Only .csv files are accepted.");

            return await _meteringService.SaveMeterReadingsAsync(file);
        }
    }
}