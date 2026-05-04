using EventGenie.Interfaces;
using EventGenie.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventGenie.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationRepository _locationRepository;

        public LocationController(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLocationsAsync()
        {
            try
            {
                var locations = await _locationRepository.GetAllLocationsAsync();
                return Ok(new Response<IEnumerable<Location>> { Success = true, Data = locations });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response<IEnumerable<Location>> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var loc = await _locationRepository.GetLocationByUserIdAsync(userId);
            if (loc == null)
                return NotFound(new Response<Location> { Success = false, Message = "Location not found for this user." });
            return Ok(new Response<Location> { Success = true, Data = loc });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocationByIdAsync(int id)
        {
            try
            {
                var location = await _locationRepository.GetLocationByIdAsync(id);
                if (location == null)
                    return NotFound(new Response<Location> { Success = false, Message = "Lokasyon bulunamadı." });

                return Ok(new Response<Location> { Success = true, Data = location });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response<Location> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddLocationAsync([FromBody] Location location)
        {
            try
            {
                if (location == null)
                    return BadRequest(new Response<Location> { Success = false, Message = "Geçersiz lokasyon değeri" });

                await _locationRepository.AddLocationAsync(location);

                return Ok(new Response<Location>
                {
                    Success = true,
                    Data = location,
                    Message = "Lokasyon başarıyla eklendi"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response<Location> { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// DÜZELTME: Eski kod yeni entity attach ediyordu → EF Core tracking çakışması (500).
        /// Yeni kod: Mevcut tracked entity'nin property'lerini güncelliyor.
        /// </summary>
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateLocationAsync(int userId, [FromBody] Location updatedLocation)
        {
            try
            {
                var existingLocation = await _locationRepository.GetLocationByUserIdAsync(userId);
                if (existingLocation == null)
                    return NotFound(new Response<Location> { Success = false, Message = "Location not found." });

                // Mevcut entity'nin alanlarını güncelle (yeni entity oluşturmak yerine)
                existingLocation.City = updatedLocation.City;
                existingLocation.Country = updatedLocation.Country;
                existingLocation.Latitude = updatedLocation.Latitude;
                existingLocation.Longitude = updatedLocation.Longitude;
                existingLocation.IsAutoDetected = updatedLocation.IsAutoDetected;

                await _locationRepository.UpdateLocationAsync(existingLocation);

                return Ok(new Response<Location>
                {
                    Success = true,
                    Data = existingLocation,
                    Message = "Lokasyon güncellendi"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response<Location> { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocationAsync(int id)
        {
            try
            {
                var existingLocation = await _locationRepository.GetLocationByIdAsync(id);
                if (existingLocation == null)
                    return NotFound(new Response<Location> { Success = false, Message = "Location not found." });

                await _locationRepository.DeleteLocationAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response<Location> { Success = false, Message = ex.Message });
            }
        }
    }
}