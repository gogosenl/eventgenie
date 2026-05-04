using EventGenie.Models;

namespace EventGenie.Interfaces
{
    public interface ILocationRepository
    {
        Task<IEnumerable<Location>> GetAllLocationsAsync();

        Task<Location?> GetLocationByIdAsync(int id);

        Task AddLocationAsync(Location location);

        Task UpdateLocationAsync(Location location);

        Task DeleteLocationAsync(int id);
        Task<Location?> GetLocationByUserIdAsync(int userId);

    }

}
