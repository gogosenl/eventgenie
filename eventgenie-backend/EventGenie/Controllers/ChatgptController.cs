using EventGenie.Interfaces;
using EventGenie.Response;
using EventGenie.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EventGenie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatgptController : ControllerBase
    {
        private readonly IEventRepository _eventRepository;
        private readonly ChatgptServices _chatGPTService;
        private readonly TicketmasterService _ticketmasterService;

        public ChatgptController(
            IEventRepository eventRepository,
            ChatgptServices chatGPTService,
            TicketmasterService ticketmasterService)
        {
            _eventRepository = eventRepository;
            _chatGPTService = chatGPTService;
            _ticketmasterService = ticketmasterService;
        }

        [HttpGet("three-events-with-comment/{userId}")]
        public async Task<IActionResult> GetThreeEventIdsAndTransferWithComment(int userId)
        {
            try
            {
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
                    return BadRequest(new { Success = false, Message = "GPT öneri sunamadı." });
                }

                var transferResult = await _ticketmasterService.TransferEventsToTripsAsync(
                    parsedResponse.Recommendations, parsedResponse.Comments);

                return Ok(new
                {
                    Success = true,
                    ChatGPTResponse = gptResponse,
                    TransferResult = transferResult
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}