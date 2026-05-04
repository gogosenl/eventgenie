using EventGenie.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventGenie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketmasterController : ControllerBase
    {
        private readonly TicketmasterService _ticketmasterService;

        public TicketmasterController(TicketmasterService ticketmasterService)
        {
            _ticketmasterService = ticketmasterService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetEvents(int userId, string startDateTime, string endDateTime)
        {
            // DÜZELTME: TryParse ile güvenli tarih dönüşümü
            if (!DateTime.TryParse(startDateTime, out DateTime startDate))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Geçersiz başlangıç tarihi formatı: '{startDateTime}'. Beklenen format: yyyy-MM-ddTHH:mm:ss"
                });
            }

            if (!DateTime.TryParse(endDateTime, out DateTime endDate))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Geçersiz bitiş tarihi formatı: '{endDateTime}'. Beklenen format: yyyy-MM-ddTHH:mm:ss"
                });
            }

            if (startDate >= endDate)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Başlangıç tarihi bitiş tarihinden önce olmalıdır."
                });
            }

            var resultMessage = await _ticketmasterService.GetEventsAsync(userId, startDate, endDate);

            if (resultMessage.StartsWith("Error"))
            {
                return BadRequest(new { Success = false, Message = resultMessage });
            }

            return Ok(new { Success = true, Message = resultMessage });
        }
    }
}