namespace EventGenie.Models
{
    public class ChatMessage
    {
        public int ChatMessageId { get; set; }

        /// <summary>
        /// Sohbet odası anahtarı: Etkinlik adı.
        /// Aynı etkinliğe giden tüm kullanıcılar aynı odada.
        /// </summary>
        public string RoomKey { get; set; } = string.Empty;

        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}