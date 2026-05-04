namespace EventGenie.Helpers
{
    /// <summary>
    /// Türkiye illeri komşuluk haritası.
    /// Her il için komşu illerin listesini döner.
    /// </summary>
    public static class TurkeyNeighborCities
    {
        private static readonly Dictionary<string, List<string>> NeighborMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Adana", new List<string> { "Mersin", "Osmaniye", "Kayseri", "Niğde", "Hatay" } },
            { "Adıyaman", new List<string> { "Malatya", "Kahramanmaraş", "Gaziantep", "Şanlıurfa", "Diyarbakır" } },
            { "Afyonkarahisar", new List<string> { "Eskişehir", "Kütahya", "Uşak", "Burdur", "Isparta", "Konya" } },
            { "Ağrı", new List<string> { "Kars", "Iğdır", "Van", "Bitlis", "Muş", "Erzurum" } },
            { "Aksaray", new List<string> { "Konya", "Niğde", "Nevşehir", "Kırşehir", "Ankara" } },
            { "Amasya", new List<string> { "Tokat", "Samsun", "Çorum", "Yozgat" } },
            { "Ankara", new List<string> { "Eskişehir", "Konya", "Aksaray", "Kırıkkale", "Çankırı", "Bolu", "Kırşehir" } },
            { "Antalya", new List<string> { "Burdur", "Isparta", "Konya", "Karaman", "Mersin", "Muğla" } },
            { "Artvin", new List<string> { "Trabzon", "Rize", "Erzurum", "Ardahan" } },
            { "Ardahan", new List<string> { "Kars", "Artvin", "Erzurum", "Iğdır" } },
            { "Aydın", new List<string> { "İzmir", "Manisa", "Denizli", "Muğla" } },
            { "Balıkesir", new List<string> { "Çanakkale", "Bursa", "Manisa", "İzmir", "Kütahya" } },
            { "Bartın", new List<string> { "Zonguldak", "Karabük", "Kastamonu" } },
            { "Batman", new List<string> { "Diyarbakır", "Siirt", "Bitlis", "Muş", "Mardin" } },
            { "Bayburt", new List<string> { "Erzurum", "Trabzon", "Gümüşhane", "Erzincan" } },
            { "Bilecik", new List<string> { "Eskişehir", "Bolu", "Bursa", "Kütahya", "Sakarya" } },
            { "Bingöl", new List<string> { "Erzurum", "Erzincan", "Tunceli", "Elazığ", "Diyarbakır", "Muş" } },
            { "Bitlis", new List<string> { "Van", "Muş", "Siirt", "Batman", "Ağrı" } },
            { "Bolu", new List<string> { "Düzce", "Ankara", "Eskişehir", "Bilecik", "Zonguldak", "Çankırı" } },
            { "Burdur", new List<string> { "Antalya", "Isparta", "Afyonkarahisar", "Muğla", "Denizli" } },
            { "Bursa", new List<string> { "İstanbul", "Yalova", "Kocaeli", "Sakarya", "Bilecik", "Eskişehir", "Kütahya", "Balıkesir" } },
            { "Çanakkale", new List<string> { "Balıkesir", "Tekirdağ", "Edirne" } },
            { "Çankırı", new List<string> { "Ankara", "Bolu", "Kastamonu", "Çorum", "Kırıkkale" } },
            { "Çorum", new List<string> { "Amasya", "Samsun", "Sinop", "Kastamonu", "Çankırı", "Kırıkkale", "Yozgat", "Tokat" } },
            { "Denizli", new List<string> { "Aydın", "Muğla", "Burdur", "Afyonkarahisar", "Uşak", "Manisa" } },
            { "Diyarbakır", new List<string> { "Şanlıurfa", "Adıyaman", "Malatya", "Elazığ", "Bingöl", "Muş", "Batman", "Siirt", "Mardin" } },
            { "Düzce", new List<string> { "Bolu", "Sakarya", "Zonguldak" } },
            { "Edirne", new List<string> { "Tekirdağ", "Kırklareli" } },
            { "Elazığ", new List<string> { "Malatya", "Diyarbakır", "Bingöl", "Tunceli", "Erzincan" } },
            { "Erzincan", new List<string> { "Erzurum", "Bayburt", "Gümüşhane", "Giresun", "Sivas", "Tunceli", "Bingöl" } },
            { "Erzurum", new List<string> { "Artvin", "Ardahan", "Kars", "Ağrı", "Muş", "Bingöl", "Erzincan", "Bayburt", "Rize", "Trabzon" } },
            { "Eskişehir", new List<string> { "Ankara", "Bolu", "Bilecik", "Kütahya", "Afyonkarahisar", "Konya", "Bursa" } },
            { "Gaziantep", new List<string> { "Adıyaman", "Kahramanmaraş", "Osmaniye", "Hatay", "Kilis", "Şanlıurfa" } },
            { "Giresun", new List<string> { "Trabzon", "Gümüşhane", "Erzincan", "Sivas", "Ordu" } },
            { "Gümüşhane", new List<string> { "Trabzon", "Bayburt", "Erzincan", "Giresun" } },
            { "Hakkari", new List<string> { "Van", "Şırnak", "Siirt" } },
            { "Hatay", new List<string> { "Adana", "Osmaniye", "Gaziantep" } },
            { "Iğdır", new List<string> { "Kars", "Ağrı", "Ardahan" } },
            { "Isparta", new List<string> { "Antalya", "Burdur", "Afyonkarahisar", "Konya" } },
            { "İstanbul", new List<string> { "Tekirdağ", "Kırklareli", "Kocaeli", "Yalova", "Bursa" } },
            { "İzmir", new List<string> { "Manisa", "Aydın", "Balıkesir", "Denizli" } },
            { "Kahramanmaraş", new List<string> { "Gaziantep", "Adıyaman", "Malatya", "Sivas", "Kayseri", "Adana", "Osmaniye" } },
            { "Karabük", new List<string> { "Bartın", "Zonguldak", "Bolu", "Çankırı", "Kastamonu" } },
            { "Karaman", new List<string> { "Konya", "Mersin", "Antalya", "Niğde" } },
            { "Kars", new List<string> { "Ardahan", "Iğdır", "Ağrı", "Erzurum" } },
            { "Kastamonu", new List<string> { "Bartın", "Karabük", "Çankırı", "Çorum", "Sinop" } },
            { "Kayseri", new List<string> { "Sivas", "Yozgat", "Nevşehir", "Niğde", "Adana", "Kahramanmaraş" } },
            { "Kilis", new List<string> { "Gaziantep" } },
            { "Kırıkkale", new List<string> { "Ankara", "Çankırı", "Çorum", "Yozgat", "Kırşehir", "Aksaray" } },
            { "Kırklareli", new List<string> { "Edirne", "Tekirdağ", "İstanbul" } },
            { "Kırşehir", new List<string> { "Ankara", "Kırıkkale", "Yozgat", "Nevşehir", "Aksaray" } },
            { "Kocaeli", new List<string> { "İstanbul", "Sakarya", "Yalova", "Bursa" } },
            { "Konya", new List<string> { "Ankara", "Eskişehir", "Afyonkarahisar", "Isparta", "Antalya", "Karaman", "Niğde", "Aksaray", "Mersin" } },
            { "Kütahya", new List<string> { "Eskişehir", "Bilecik", "Bursa", "Balıkesir", "Manisa", "Uşak", "Afyonkarahisar" } },
            { "Malatya", new List<string> { "Adıyaman", "Kahramanmaraş", "Sivas", "Erzincan", "Tunceli", "Elazığ", "Diyarbakır" } },
            { "Manisa", new List<string> { "İzmir", "Aydın", "Denizli", "Uşak", "Kütahya", "Balıkesir" } },
            { "Mardin", new List<string> { "Diyarbakır", "Batman", "Siirt", "Şırnak", "Şanlıurfa" } },
            { "Mersin", new List<string> { "Adana", "Antalya", "Karaman", "Konya", "Niğde" } },
            { "Muğla", new List<string> { "Aydın", "Denizli", "Burdur", "Antalya" } },
            { "Muş", new List<string> { "Erzurum", "Ağrı", "Bitlis", "Bingöl", "Diyarbakır", "Batman" } },
            { "Nevşehir", new List<string> { "Kayseri", "Kırşehir", "Aksaray", "Niğde", "Yozgat" } },
            { "Niğde", new List<string> { "Kayseri", "Nevşehir", "Aksaray", "Konya", "Karaman", "Mersin", "Adana" } },
            { "Ordu", new List<string> { "Samsun", "Giresun", "Tokat", "Sivas" } },
            { "Osmaniye", new List<string> { "Adana", "Hatay", "Gaziantep", "Kahramanmaraş" } },
            { "Rize", new List<string> { "Trabzon", "Artvin", "Erzurum" } },
            { "Sakarya", new List<string> { "Kocaeli", "Düzce", "Bolu", "Bilecik", "Bursa" } },
            { "Samsun", new List<string> { "Ordu", "Tokat", "Amasya", "Çorum", "Sinop" } },
            { "Şanlıurfa", new List<string> { "Adıyaman", "Gaziantep", "Kilis", "Diyarbakır", "Mardin" } },
            { "Siirt", new List<string> { "Batman", "Bitlis", "Şırnak", "Mardin", "Diyarbakır" } },
            { "Sinop", new List<string> { "Samsun", "Çorum", "Kastamonu" } },
            { "Sivas", new List<string> { "Tokat", "Ordu", "Giresun", "Erzincan", "Malatya", "Kahramanmaraş", "Kayseri", "Yozgat", "Amasya" } },
            { "Şırnak", new List<string> { "Mardin", "Siirt", "Hakkari", "Van" } },
            { "Tekirdağ", new List<string> { "İstanbul", "Edirne", "Kırklareli", "Çanakkale" } },
            { "Tokat", new List<string> { "Samsun", "Ordu", "Sivas", "Amasya", "Çorum", "Yozgat" } },
            { "Trabzon", new List<string> { "Rize", "Artvin", "Gümüşhane", "Bayburt", "Giresun", "Erzurum" } },
            { "Tunceli", new List<string> { "Erzincan", "Bingöl", "Elazığ", "Malatya" } },
            { "Uşak", new List<string> { "Kütahya", "Afyonkarahisar", "Denizli", "Manisa" } },
            { "Van", new List<string> { "Ağrı", "Bitlis", "Hakkari", "Şırnak", "Siirt" } },
            { "Yalova", new List<string> { "Bursa", "İstanbul", "Kocaeli" } },
            { "Yozgat", new List<string> { "Kırşehir", "Kırıkkale", "Çorum", "Amasya", "Tokat", "Sivas", "Kayseri", "Nevşehir" } },
            { "Zonguldak", new List<string> { "Bartın", "Karabük", "Bolu", "Düzce" } },
        };

        /// <summary>
        /// Verilen şehrin komşu illerini döner.
        /// Şehir bulunamazsa boş liste döner.
        /// </summary>
        public static List<string> GetNeighbors(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return new List<string>();

            // Şehir adını normalize et
            var normalizedCity = city.Trim();

            if (NeighborMap.TryGetValue(normalizedCity, out var neighbors))
                return neighbors;

            // Fuzzy match: "istanbul" → "İstanbul" gibi durumlar için
            var match = NeighborMap.Keys.FirstOrDefault(k =>
                string.Equals(k, normalizedCity, StringComparison.OrdinalIgnoreCase));

            if (match != null)
                return NeighborMap[match];

            return new List<string>();
        }

        /// <summary>
        /// Şehir + komşularını birlikte döner (toplam liste).
        /// </summary>
        public static List<string> GetCityAndNeighbors(string city)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(city))
                return result;

            result.Add(city.Trim());
            result.AddRange(GetNeighbors(city));

            return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}