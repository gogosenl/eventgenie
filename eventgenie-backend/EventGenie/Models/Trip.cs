namespace EventGenie.Models
{
    public class Trip
    {
        public int TripId { get; set; }
        public string TripName { get; set; }
        public string TripDescription { get; set; }
        public string TripUrl { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime TripDate { get; set; }
        public int UserId { get; set; }
        public int RequestGroupId { get; set; }
        public string TripComment { get; set; }

        // ✅ YENİ: Aktif rota mı yoksa geçmiş rota mı?
        public bool IsActive { get; set; } = true;

        // ✅ YENİ: Rotanın oluşturulma zamanı (geçmiş rotalarda gruplama için)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}