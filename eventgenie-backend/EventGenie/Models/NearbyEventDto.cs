namespace EventGenie.Models
{
    /// <summary>
    /// Bildirim sistemi için yakındaki etkinlik DTO'su.
    /// Veritabanına kaydedilmez, sadece API response olarak döner.
    /// </summary>
    public class NearbyEventDto
    {
        public string EventName { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string EventUrl { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Genre { get; set; } = string.Empty;
        public string? PriceRange { get; set; }

        /// <summary>
        /// true → Kullanıcının tercihine uyuyor ("Senin İçin" etiketi)
        /// false → Genel etkinlik ("Yakınında" etiketi)
        /// </summary>
        public bool IsPreferenceMatch { get; set; }

        /// <summary>
        /// Ticketmaster'dan gelen etkinlik görseli URL'i
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;




    }
}