using EventGenie.Models;

namespace EventGenie.Interfaces
{
    public interface IChatRepository
    {
        Task<List<ChatMessage>> GetMessagesByRoomKeyAsync(string roomKey);
        Task AddMessageAsync(ChatMessage message);
        Task<int> GetParticipantCountAsync(string roomKey);

        Task DeleteMessageAsync(int chatMessageId);
    }
}