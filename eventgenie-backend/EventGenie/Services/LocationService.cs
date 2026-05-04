using EventGenie.Interfaces;
using EventGenie.Models;

namespace EventGenie.Services
{
    public class LocationService
    {
        private readonly ILocationRepository _locationRepository;

        public LocationService(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public async Task AddManualLocationAsync(Location location)
        {
            location.IsAutoDetected = false;
            await _locationRepository.AddLocationAsync(location);
        }

        public async Task AddAutoLocation(decimal latitude, decimal longitude)
        {
            var location = new Location
            {
                Country = "DetectedCountry",
                City = "DetectedCity",
                Latitude = latitude,
                Longitude = longitude,
                IsAutoDetected = true
            };

            await _locationRepository.AddLocationAsync(location);
        }
    }
}