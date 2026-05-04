namespace EventGenie.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public string EventUrl { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime EventDate { get; set; }
        public int UserId { get; set; }
        public int RequestGroupId { get; set; }

    }

}
