using EventGenie.Data;
using EventGenie.Interfaces;
using EventGenie.Models;
using Microsoft.EntityFrameworkCore;

namespace EventGenie.Repositorys
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;

        public EventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddEventAsync(Event eventEntity)
        {
            await _context.Events.AddAsync(eventEntity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllEventsAsync()
        {
            var allEvents = await _context.Events.ToListAsync();
            _context.Events.RemoveRange(allEvents);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEventAsync(int id)
        {
            var eventToDelete = await _context.Events.FindAsync(id);
            if (eventToDelete != null)
            {
                _context.Events.Remove(eventToDelete);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteEventsByUserIdAsync(int userId)
        {
            var events = await _context.Events.Where(e => e.UserId == userId).ToListAsync();
            _context.Events.RemoveRange(events);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EventExistsAsync(int id)
        {
            return await _context.Events.AnyAsync(e => e.EventId == id);
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _context.Events.ToListAsync();
        }

        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _context.Events.FindAsync(id);
        }

        public async Task<IEnumerable<Event>> GetEventsByUserIdAsync(int userId)
        {
            return await _context.Events
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }

        public async Task UpdateEventAsync(Event eventEntity)
        {
            _context.Events.Update(eventEntity);
            await _context.SaveChangesAsync();
        }
    }
}