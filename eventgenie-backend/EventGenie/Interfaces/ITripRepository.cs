using EventGenie.Models;

namespace EventGenie.Interfaces
{
    public interface ITripRepository
    {
        Task<IEnumerable<Trip>> GetAllTripsAsync();
        Task<Trip?> GetTripByIdAsync(int id);
        Task<List<Trip>> GetTripsByUserIdAsync(int userId);
        Task AddTripAsync(Trip trip);
        Task UpdateTripAsync(Trip trip);
        Task DeleteTripAsync(int id);
        Task<bool> TripExistsAsync(int id);
        Task DeleteTripsByUserIdAsync(int userId);

        // ✅ YENİ: Aktif ve arşivlenmiş trip'leri ayrı getir
        Task<List<Trip>> GetActiveTripsByUserIdAsync(int userId);
        Task<List<Trip>> GetArchivedTripsByUserIdAsync(int userId);
        Task ArchiveTripsByUserIdAsync(int userId);

        Task DeleteActiveTripsByUserIdAsync(int userId);

        Task DeleteArchivedRouteByNamesAsync(int userId, List<string> tripNames);


    }
}