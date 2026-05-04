namespace EventGenie.Models
{
    public class Location
    {
        public int LocationId { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public bool IsAutoDetected { get; set; } // Manuel mi yoksa otomatik mi
        public int UserId { get; set; }

    }
}
