using EventGenie.Models;

namespace EventGenie.Interfaces
{
    public interface IEventRepository
    {
        Task<IEnumerable<Event>> GetAllEventsAsync();

        Task<Event?> GetEventByIdAsync(int id);

        Task AddEventAsync(Event eventEntity);

        Task UpdateEventAsync(Event eventEntity);

        Task DeleteEventAsync(int id);

        Task<bool> EventExistsAsync(int id);

        Task<IEnumerable<Event>> GetEventsByUserIdAsync(int userId);

        Task DeleteAllEventsAsync();

        Task DeleteEventsByUserIdAsync(int userId);

    }
}
