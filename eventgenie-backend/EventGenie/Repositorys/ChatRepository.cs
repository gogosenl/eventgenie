using EventGenie.Data;
using EventGenie.Interfaces;
using EventGenie.Models;
using Microsoft.EntityFrameworkCore;

namespace EventGenie.Repositorys
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDbContext _context;

        public ChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatMessage>> GetMessagesByRoomKeyAsync(string roomKey)
        {
            return await _context.ChatMessages
                .Where(m => m.RoomKey == roomKey)
                .OrderBy(m => m.CreatedAt)
                .Take(200) // Son 200 mesaj
                .ToListAsync();
        }

        public async Task AddMessageAsync(ChatMessage message)
        {
            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Bu odaya mesaj yazmış benzersiz kullanıcı sayısı
        /// </summary>
        public async Task<int> GetParticipantCountAsync(string roomKey)
        {
            return await _context.ChatMessages
                .Where(m => m.RoomKey == roomKey)
                .Select(m => m.UserId)
                .Distinct()
                .CountAsync();
        }

        // ===================================================================
        // 2) ChatRepository.cs'e EKLE:
        // ===================================================================

        public async Task DeleteMessageAsync(int chatMessageId)
        {
            var msg = await _context.ChatMessages.FindAsync(chatMessageId);
            if (msg != null)
            {
                _context.ChatMessages.Remove(msg);
                await _context.SaveChangesAsync();
            }
        }

    }
}