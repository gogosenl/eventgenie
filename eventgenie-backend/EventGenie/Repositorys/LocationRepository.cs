using EventGenie.Data;
using EventGenie.Interfaces;
using EventGenie.Models;
using Microsoft.EntityFrameworkCore;

namespace EventGenie.Repositorys
{
    public class LocationRepository : ILocationRepository
    {
        private readonly AppDbContext _context;

        public LocationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddLocationAsync(Location location)
        {
            await _context.Locations.AddAsync(location);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLocationAsync(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location != null)
            {
                _context.Locations.Remove(location);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            return await _context.Locations.ToListAsync();
        }

        public async Task<Location?> GetLocationByIdAsync(int id)
        {
            return await _context.Locations.FindAsync(id);
        }

        public async Task UpdateLocationAsync(Location location)
        {
            // Entity tracking hatası önlemek için: GetLocationByUserIdAsync ile yüklenen entity'ye
            // değerleri mapletiyoruz, yeni entity attach etmiyoruz.
            var existingLocation = await _context.Locations.FindAsync(location.LocationId);
            if (existingLocation != null)
            {
                existingLocation.City = location.City;
                existingLocation.Country = location.Country;
                existingLocation.Latitude = location.Latitude;
                existingLocation.Longitude = location.Longitude;
                existingLocation.IsAutoDetected = location.IsAutoDetected;
                existingLocation.UserId = location.UserId;
                
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Location?> GetLocationByUserIdAsync(int userId)
        {
            return await _context.Locations
                .FirstOrDefaultAsync(l => l.UserId == userId);
        }
    }
}