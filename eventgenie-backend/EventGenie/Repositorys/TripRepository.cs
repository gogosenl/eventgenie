using EventGenie.Data;
using EventGenie.Interfaces;
using EventGenie.Models;
using Microsoft.EntityFrameworkCore;

namespace EventGenie.Repositorys
{
    public class TripRepository : ITripRepository
    {
        private readonly AppDbContext _context;

        public TripRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddTripAsync(Trip trip)
        {
            await _context.Trips.AddAsync(trip);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTripAsync(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip != null)
            {
                _context.Trips.Remove(trip);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteTripsByUserIdAsync(int userId)
        {
            var trips = await _context.Trips.Where(t => t.UserId == userId).ToListAsync();
            _context.Trips.RemoveRange(trips);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Trip>> GetAllTripsAsync()
        {
            return await _context.Trips.ToListAsync();
        }

        public async Task<Trip?> GetTripByIdAsync(int id)
        {
            return await _context.Trips.FindAsync(id);
        }

        public async Task<List<Trip>> GetTripsByUserIdAsync(int userId)
        {
            return await _context.Trips
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> TripExistsAsync(int id)
        {
            return await _context.Trips.AnyAsync(t => t.TripId == id);
        }

        public async Task UpdateTripAsync(Trip trip)
        {
            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();
        }

        // ✅ YENİ: Sadece aktif trip'leri getir
        public async Task<List<Trip>> GetActiveTripsByUserIdAsync(int userId)
        {
            return await _context.Trips
                .Where(t => t.UserId == userId && t.IsActive)
                .OrderBy(t => t.TripDate)
                .ToListAsync();
        }

        // ✅ YENİ: Sadece arşivlenmiş (eski) trip'leri getir — en yeniden en eskiye
        public async Task<List<Trip>> GetArchivedTripsByUserIdAsync(int userId)
        {
            return await _context.Trips
                .Where(t => t.UserId == userId && !t.IsActive)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        // ✅ YENİ: Kullanıcının aktif trip'lerini arşivle (silmek yerine)
        public async Task ArchiveTripsByUserIdAsync(int userId)
        {
            var activeTrips = await _context.Trips
                .Where(t => t.UserId == userId && t.IsActive)
                .ToListAsync();

            foreach (var trip in activeTrips)
            {
                trip.IsActive = false;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Kullanıcının aktif trip'lerini siler (arşivlemeden).
        /// Duplicate rota kontrolünde kullanılır.
        /// </summary>
        public async Task DeleteActiveTripsByUserIdAsync(int userId)
        {
            var activeTrips = await _context.Trips
                .Where(t => t.UserId == userId && t.IsActive)
                .ToListAsync();

            _context.Trips.RemoveRange(activeTrips);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Arşivlenmiş trip'lerden belirli isimlere sahip olanları siler.
        /// Aynı isimlere sahip birden fazla rota varsa hepsini siler.
        /// </summary>
        public async Task DeleteArchivedRouteByNamesAsync(int userId, List<string> tripNames)
        {
            // İsimleri normalize et
            var normalizedNames = tripNames.Select(n => n.Trim().ToLowerInvariant()).ToList();

            var tripsToDelete = await _context.Trips
                .Where(t => t.UserId == userId &&
                       !t.IsActive &&
                       normalizedNames.Contains(t.TripName.Trim().ToLower()))
                .ToListAsync();

            if (tripsToDelete.Any())
            {
                _context.Trips.RemoveRange(tripsToDelete);
                await _context.SaveChangesAsync();
            }
        }

    }
}