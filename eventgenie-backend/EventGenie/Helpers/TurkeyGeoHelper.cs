namespace EventGenie.Helpers
{
    /// <summary>
    /// GPS koordinatından en yakın Türkiye ilini bulur.
    /// Harici API gerektirmez — 81 ilin merkez koordinatlarını kullanır.
    /// </summary>
    public static class TurkeyGeoHelper
    {
        private static readonly List<(string City, double Lat, double Lng)> CityCoordinates = new()
        {
            ("Adana", 37.0, 35.32), ("Adıyaman", 37.76, 38.28), ("Afyonkarahisar", 38.74, 30.54),
            ("Ağrı", 39.72, 43.05), ("Aksaray", 38.37, 34.03), ("Amasya", 40.65, 35.83),
            ("Ankara", 39.93, 32.85), ("Antalya", 36.89, 30.71), ("Artvin", 41.18, 41.82),
            ("Ardahan", 41.11, 42.70), ("Aydın", 37.85, 27.85), ("Balıkesir", 39.65, 27.89),
            ("Bartın", 41.64, 32.34), ("Batman", 37.88, 41.13), ("Bayburt", 40.26, 40.23),
            ("Bilecik", 40.05, 30.00), ("Bingöl", 38.88, 40.50), ("Bitlis", 38.40, 42.11),
            ("Bolu", 40.73, 31.61), ("Burdur", 37.72, 30.29), ("Bursa", 40.19, 29.06),
            ("Çanakkale", 40.15, 26.41), ("Çankırı", 40.60, 33.62), ("Çorum", 40.55, 34.96),
            ("Denizli", 37.77, 29.09), ("Diyarbakır", 37.91, 40.24), ("Düzce", 40.84, 31.16),
            ("Edirne", 41.68, 26.56), ("Elazığ", 38.67, 39.22), ("Erzincan", 39.75, 39.49),
            ("Erzurum", 39.90, 41.27), ("Eskişehir", 39.78, 30.52), ("Gaziantep", 37.07, 37.38),
            ("Giresun", 40.91, 38.39), ("Gümüşhane", 40.46, 39.48), ("Hakkari", 37.57, 43.74),
            ("Hatay", 36.40, 36.35), ("Iğdır", 39.92, 44.05), ("Isparta", 37.76, 30.55),
            ("İstanbul", 41.01, 28.98), ("İzmir", 38.42, 27.14), ("Kahramanmaraş", 37.59, 36.93),
            ("Karabük", 41.20, 32.63), ("Karaman", 37.18, 33.23), ("Kars", 40.60, 43.10),
            ("Kastamonu", 41.39, 33.78), ("Kayseri", 38.73, 35.49), ("Kilis", 36.72, 37.12),
            ("Kırıkkale", 39.85, 33.51), ("Kırklareli", 41.74, 27.23), ("Kırşehir", 39.15, 34.17),
            ("Kocaeli", 40.77, 29.92), ("Konya", 37.87, 32.48), ("Kütahya", 39.42, 29.98),
            ("Malatya", 38.35, 38.31), ("Manisa", 38.61, 27.43), ("Mardin", 37.31, 40.73),
            ("Mersin", 36.80, 34.63), ("Muğla", 37.22, 28.36), ("Muş", 38.95, 41.75),
            ("Nevşehir", 38.63, 34.71), ("Niğde", 37.97, 34.68), ("Ordu", 40.98, 37.88),
            ("Osmaniye", 37.07, 36.25), ("Rize", 41.02, 40.52), ("Sakarya", 40.69, 30.40),
            ("Samsun", 41.29, 36.33), ("Şanlıurfa", 37.16, 38.79), ("Siirt", 37.93, 41.94),
            ("Sinop", 42.03, 35.15), ("Sivas", 39.75, 37.02), ("Şırnak", 37.52, 42.46),
            ("Tekirdağ", 41.00, 27.52), ("Tokat", 40.31, 36.55), ("Trabzon", 41.00, 39.72),
            ("Tunceli", 39.11, 39.55), ("Uşak", 38.67, 29.41), ("Van", 38.49, 43.38),
            ("Yalova", 40.66, 29.27), ("Yozgat", 39.82, 34.81), ("Zonguldak", 41.46, 31.79)
        };

        /// <summary>
        /// GPS koordinatından en yakın Türkiye ilini döner.
        /// </summary>
        public static string? FindNearestCity(double lat, double lng)
        {
            string? nearest = null;
            double minDistance = double.MaxValue;

            foreach (var city in CityCoordinates)
            {
                var dist = Haversine(lat, lng, city.Lat, city.Lng);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = city.City;
                }
            }

            return nearest;
        }

        private static double Haversine(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371;
            var dLat = ToRad(lat2 - lat1);
            var dLng = ToRad(lng2 - lng1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static double ToRad(double deg) => deg * Math.PI / 180;
    }
}