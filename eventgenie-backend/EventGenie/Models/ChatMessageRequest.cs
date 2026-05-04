namespace EventGenie.Models
{
    /// <summary>
    /// Mesaj gönderme request modeli
    /// </summary>
    public class ChatMessageRequest
    {
        public string RoomKey { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
