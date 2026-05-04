using EventGenie.Interfaces;
using EventGenie.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventGenie.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripController : ControllerBase
    {
        private readonly ITripRepository _tripRepository;

        public TripController(ITripRepository tripRepository)
        {
            _tripRepository = tripRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetTripsAsync()
        {
            var trips = await _tripRepository.GetAllTripsAsync();
            return Ok(trips);
        }

        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetTripsByUserIdAsync(int userId)
        {
            var trips = await _tripRepository.GetTripsByUserIdAsync(userId);
            return Ok(trips);
        }

        // ✅ YENİ: Sadece aktif trip'leri getir
        [HttpGet("active/{userId}")]
        public async Task<IActionResult> GetActiveTripsByUserIdAsync(int userId)
        {
            var trips = await _tripRepository.GetActiveTripsByUserIdAsync(userId);
            return Ok(trips);
        }

        // ✅ YENİ: Geçmiş (arşivlenmiş) trip'leri getir
        [HttpGet("archived/{userId}")]
        public async Task<IActionResult> GetArchivedTripsByUserIdAsync(int userId)
        {
            var trips = await _tripRepository.GetArchivedTripsByUserIdAsync(userId);
            return Ok(trips);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTripByIdAsync(int id)
        {
            var trip = await _tripRepository.GetTripByIdAsync(id);
            if (trip == null)
                return NotFound(new { Success = false, Message = "Trip not found." });
            return Ok(trip);
        }

        [HttpPost]
        public async Task<IActionResult> AddTripAsync([FromBody] Trip trip)
        {
            if (trip == null)
                return BadRequest(new { Success = false, Message = "Invalid trip data." });

            await _tripRepository.AddTripAsync(trip);
            return Ok(new { Success = true, Data = trip });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTripAsync(int id, [FromBody] Trip updatedTrip)
        {
            if (!await _tripRepository.TripExistsAsync(id))
                return NotFound(new { Success = false, Message = "Trip not found." });

            updatedTrip.TripId = id;
            await _tripRepository.UpdateTripAsync(updatedTrip);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTripAsync(int id)
        {
            if (!await _tripRepository.TripExistsAsync(id))
                return NotFound(new { Success = false, Message = "Trip not found." });

            await _tripRepository.DeleteTripAsync(id);
            return NoContent();
        }


        /// <summary>
        /// Belirli bir rota grubunu siler (etkinlik isimlerine göre).
        /// Geçmiş rotalardan silme işlemi için kullanılır.
        /// </summary>
        [HttpDelete("delete-route/{userId}")]
        public async Task<IActionResult> DeleteArchivedRouteAsync(int userId, [FromBody] List<string> tripNames)
        {
            if (tripNames == null || !tripNames.Any())
                return BadRequest(new { Success = false, Message = "Trip names required." });

            try
            {
                await _tripRepository.DeleteArchivedRouteByNamesAsync(userId, tripNames);
                return Ok(new { Success = true, Message = "Rota başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Hata: {ex.Message}" });
            }
        }


        /// <summary>
        /// Kullanıcının aktif rotasını siler.
        /// </summary>
        [HttpDelete("active/{userId}")]
        public async Task<IActionResult> DeleteActiveTripsByUserIdAsync(int userId)
        {
            try
            {
                await _tripRepository.DeleteActiveTripsByUserIdAsync(userId);
                return Ok(new { Success = true, Message = "Active route deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }
    }
}