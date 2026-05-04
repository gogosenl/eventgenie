using EventGenie.Interfaces;
using EventGenie.Response;
using EventGenie.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EventGenie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly IEventRepository _eventRepository;
        private readonly ChatgptServices _chatGPTService;
        private readonly TicketmasterService _ticketmasterService;

        public EventController(
           IEventRepository eventRepository,
           ChatgptServices chatGPTService,
           TicketmasterService ticketmasterService)
        {
            _eventRepository = eventRepository;
            _chatGPTService = chatGPTService;
            _ticketmasterService = ticketmasterService;
        }

        [HttpGet("fetch-and-recommend/{userId}")]
        public async Task<IActionResult> FetchEventsAndRecommend(int userId, string startDateTime, string endDateTime)
        {
            try
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

                // Ek kontrol: başlangıç tarihi bitiş tarihinden önce olmalı
                if (startDate >= endDate)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Başlangıç tarihi bitiş tarihinden önce olmalıdır."
                    });
                }

                var ticketmasterResult = await _ticketmasterService.GetEventsAsync(userId, startDate, endDate);

                if (ticketmasterResult.StartsWith("Error"))
                {
                    return BadRequest(new { Success = false, Message = ticketmasterResult });
                }

                // Kullanıcının tüm etkinliklerini al
                var userEvents = (await _eventRepository.GetEventsByUserIdAsync(userId)).ToList();

                if (!userEvents.Any())
                {
                    return NotFound(new { Success = false, Message = "No events found for this user." });
                }

                var filteredEvents = userEvents.Select(e => new
                {
                    e.EventId,
                    e.EventName,
                    e.RequestGroupId
                }).ToList();

                var eventsJson = JsonConvert.SerializeObject(filteredEvents);

                // DÜZELTME: "seda yüz asla gösterme" satırı kaldırıldı (debug/test kalıntısı)
                var prompt = $@"
                    Aşağıda kullanıcıya ait tüm etkinliklerin listesi (JSON) bulunmaktadır. 
                    Her etkinliğin bir RequestGroupId değeri vardır. Etkinlikleri seçerken farklı RequestGroupId olmasına dikkat et.
                    Aynı eventname'e sahip etkinlikleri verme, farklı olsun.

                    Lütfen sadece 3 tane etkinlik seç. Eğer 3'ten az etkinlik varsa; kaç tane etkinlik olduğu fark etmez, olan etkinlikleri gene sadece bana JSON verisi ile döndür. 
                    Bu 3 etkinliğin mutlaka birbirinden farklı RequestGroupId değerleri olsun.
                    Aynı etkinlikleri gösterme.
                    Seçeceğin etkinlikler için, birer cümlelik yorum üret.

                    Kullanılacak JSON dönüş formatı HARİÇ hiçbir ek açıklama yapma.
                    Sadece şu formatta dön:
                    {{
                       ""recommendations"": [EtkinlikID1, EtkinlikID2, EtkinlikID3],
                       ""comments"": [
                         {{ ""EventId"": EtkinlikID1, ""Comment"": ""..."" }},
                         {{ ""EventId"": EtkinlikID2, ""Comment"": ""..."" }},
                         {{ ""EventId"": EtkinlikID3, ""Comment"": ""..."" }}
                       ]
                    }}

                    Etkinlikler (JSON):
                    {eventsJson}
                    ";

                var gptResponse = await _chatGPTService.GetSuggestionsAsync(prompt);

                GptResponse? parsedResponse;
                try
                {
                    parsedResponse = JsonConvert.DeserializeObject<GptResponse>(gptResponse);
                }
                catch (Exception)
                {
                    return BadRequest(new { Success = false, Message = $"GPT yanıtı parse edilemedi: {gptResponse}" });
                }

                if (parsedResponse?.Recommendations == null || !parsedResponse.Recommendations.Any())
                {
                    return BadRequest(new { Success = false, Message = "No valid recommendations found." });
                }

                var transferResult = await _ticketmasterService.TransferEventsToTripsAsync(
                    parsedResponse.Recommendations, parsedResponse.Comments);

                return Ok(new
                {
                    Success = true,
                    TicketmasterResponse = ticketmasterResult,
                    ChatGPTResponse = gptResponse,
                    TransferResult = transferResult
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        // ===================================================================
        // EventController.cs'e EKLENECEK ENDPOINT
        // Mevcut FetchEventsAndRecommend method'undan SONRA yapıştır.
        // ===================================================================

        /// <summary>
        /// Bildirim sistemi: Kullanıcının şehri + komşu şehirlerdeki yaklaşan etkinlikleri döner.
        /// Tercih eşleşenler IsPreferenceMatch = true olarak işaretlenir ve üstte gösterilir.
        /// </summary>
        [HttpGet("nearby/{userId}")]
        public async Task<IActionResult> GetNearbyEvents(int userId, [FromQuery] double? lat = null, [FromQuery] double? lng = null, [FromQuery] string? cityOverride = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var events = await _ticketmasterService.GetNearbyEventsAsync(userId, lat, lng, cityOverride, startDate, endDate);
                return Ok(new { Success = true, Count = events.Count, Data = events });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }


    }
}