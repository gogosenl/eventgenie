using EventGenie.Interfaces;
using EventGenie.Models;
using EventGenie.Response;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace EventGenie.Services
{
    public class TicketmasterService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocationRepository _locationRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;
        private readonly ChatgptServices _chatGPTService;
        private readonly ITripRepository _tripRepository;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        // ✅ Türkiye pazarında aktif olan genre ID haritası
        // Uygulama ismi → Ticketmaster genreId
        private static readonly Dictionary<string, string> GenreIdMap = new(StringComparer.OrdinalIgnoreCase)
        {
            // 🎵 Konser alt kategorileri (Music segment)
            { "Pop",         "KnvZfZ7vAev" },
            { "Rock",        "KnvZfZ7vAeA" },
            { "Hip-Hop/Rap", "KnvZfZ7vAv1" },
            { "HipHop",      "KnvZfZ7vAv1" },  // alternatif yazım
            { "Alternative", "KnvZfZ7vAvv" },

            // 🎭 Tiyatro alt kategorileri (Arts & Theatre segment, Comedy HARİÇ)
            { "Theatre",     "KnvZfZ7v7l1" },
            { "Dance",       "KnvZfZ7v7nI" },

            // 🎤 Stand-Up alt kategorisi (Arts & Theatre > Comedy)
            { "Comedy",      "KnvZfZ7vAe1" },
        };

        // ✅ Üst kategori → Segment ID haritası (eğer tüm segment istenirse)
        private static readonly Dictionary<string, string> SegmentIdMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Concert", "KZFzniwnSyZfZ7v7nJ" },  // Music segment
            { "Music",   "KZFzniwnSyZfZ7v7nJ" },
        };

        public TicketmasterService(
            HttpClient httpClient,
            ITripRepository tripRepository,
            ILocationRepository locationRepository,
            IEventRepository eventRepository,
            IUserRepository userRepository,
            ChatgptServices chatGPTService,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _tripRepository = tripRepository;
            _locationRepository = locationRepository;
            _eventRepository = eventRepository;
            _userRepository = userRepository;
            _chatGPTService = chatGPTService;

            _apiKey = configuration["TicketmasterAPI:ApiKey"]
                ?? throw new InvalidOperationException("TicketmasterAPI:ApiKey is not configured in appsettings.json");
            _baseUrl = configuration["TicketmasterAPI:BaseUrl"]
                ?? "https://app.ticketmaster.com/discovery/v2/events.json?";

            if (!_baseUrl.EndsWith("?"))
                _baseUrl += "?";
        }

        public async Task<string> GetEventsAsync(int userId, DateTime startDateTime, DateTime endDateTime)
        {
            try
            {
                var startDate = startDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
                var endDate = endDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return "Error: User not found";

                var tercih = user.Preferences ?? string.Empty;

                var location = await _locationRepository.GetLocationByUserIdAsync(userId);
                if (location == null)
                    return "Error: Location not found";

                // Mevcut etkinlikleri temizle
                await _eventRepository.DeleteEventsByUserIdAsync(userId);

                bool isAutoDetected = location.IsAutoDetected;

                // ✅ Yeni format: "Pop:5,Rock:3,Comedy:4" veya eski format: "Pop,Rock,Comedy"
                var tercihDizisi = tercih.Split(',', StringSplitOptions.RemoveEmptyEntries);

                int totalEventCount = 0;
                int requestGroupId = 0;

                foreach (var tercihItemRaw in tercihDizisi)
                {
                    requestGroupId++;
                    var tercihItem = tercihItemRaw.Trim();

                    // ✅ Yıldız puanını ayır (varsa): "Pop:5" → name="Pop", rating=5
                    string preferenceName;
                    int rating = 3; // varsayılan puan

                    if (tercihItem.Contains(':'))
                    {
                        var parts = tercihItem.Split(':');
                        preferenceName = parts[0].Trim();
                        int.TryParse(parts[1].Trim(), out rating);
                    }
                    else
                    {
                        preferenceName = tercihItem;
                    }

                    // ✅ Puanı 0 olan tercihleri atla (kullanıcı ilgilenmiyorsa)
                    if (rating <= 0)
                        continue;

                    string url = BuildTicketmasterUrl(preferenceName, location, isAutoDetected, startDate, endDate);

                    Console.WriteLine($"[TicketmasterService] Fetching: {preferenceName} (rating: {rating})");
                    Console.WriteLine($"[TicketmasterService] URL: {url}");

                    var response = await _httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        totalEventCount += await SaveEventsFromJsonAsync(content, userId, requestGroupId);
                    }
                    else
                    {
                        Console.WriteLine($"[TicketmasterService] API Error for '{preferenceName}': {response.StatusCode}");
                    }
                }

                var userToUpdate = await _userRepository.GetUserByIdAsync(userId);
                if (userToUpdate != null)
                {
                    userToUpdate.EventRange = totalEventCount.ToString();
                    await _userRepository.UpdateUserAsync(userToUpdate);
                }

                if (totalEventCount == 0)
                    return "Events fetched but no results found for the given preferences and location.";

                return "Events fetched and saved successfully.";
            }
            catch (Exception ex)
            {
                return $"An error occurred while fetching events: {ex.Message}";
            }
        }

        /// <summary>
        /// ✅ GÜNCELLEME: genreId tabanlı sorgu oluşturma
        /// Eski: classificationName=Concert (çalışmıyordu)
        /// Yeni: genreId=KnvZfZ7vAev (doğru genre ID ile sorgu)
        /// </summary>
        private string BuildTicketmasterUrl(string preferenceName, Location location, bool isAutoDetected, string startDate, string endDate)
        {
            // ✅ Önce genre ID haritasında ara
            string filterParam;

            if (GenreIdMap.TryGetValue(preferenceName, out string? genreId))
            {
                // Spesifik genre ID ile sorgula (en doğru sonuç)
                filterParam = $"genreId={genreId}";
            }
            else if (SegmentIdMap.TryGetValue(preferenceName, out string? segmentId))
            {
                // Üst kategori (tüm segment) ile sorgula
                filterParam = $"segmentId={segmentId}";
            }
            else
            {
                // Bilinmeyen tercih → classificationName ile dene (fallback)
                filterParam = $"classificationName={Uri.EscapeDataString(preferenceName)}";
                Console.WriteLine($"[TicketmasterService] WARNING: Unknown preference '{preferenceName}', using classificationName fallback");
            }

            // Lokasyon parametresi
            string locationParam;
            if (!isAutoDetected)
            {
                // Manuel şehir seçimi → city parametresi + countryCode
                var sehirEncoded = Uri.EscapeDataString(location.City ?? string.Empty);
                locationParam = $"city={sehirEncoded}&countryCode=TR";
            }
            else
            {
                // Otomatik konum → latlong + radius
                var latlong =
                    $"{Math.Round(location.Latitude, 6, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture)}," +
                    $"{Math.Round(location.Longitude, 6, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture)}";
                locationParam = $"latlong={latlong}&radius=50&unit=km&countryCode=TR";
            }

            // ✅ countryCode=TR eklendi (her zaman Türkiye pazarına odaklan)
            // ✅ size=50 eklendi (daha fazla sonuç al)
            return $"{_baseUrl}{filterParam}&{locationParam}&apikey={_apiKey}&startDateTime={startDate}&endDateTime={endDate}&size=50&sort=date,asc";
        }

        private async Task<int> SaveEventsFromJsonAsync(string jsonContent, int userId, int requestGroupId)
        {
            int newEventCount = 0;

            try
            {
                var jObject = JObject.Parse(jsonContent);
                var eventsToken = jObject["_embedded"]?["events"];
                if (eventsToken == null)
                    return newEventCount;

                foreach (var item in eventsToken)
                {
                    string? eventName = item["name"]?.ToString();
                    string? eventUrl = item["url"]?.ToString();
                    string? dateTimeString = item["dates"]?["start"]?["dateTime"]?.ToString();

                    DateTime eventDate;
                    if (!DateTime.TryParse(dateTimeString, out eventDate))
                    {
                        var localDateString = item["dates"]?["start"]?["localDate"]?.ToString();
                        var localTimeString = item["dates"]?["start"]?["localTime"]?.ToString();

                        if (!string.IsNullOrWhiteSpace(localDateString) &&
                            !string.IsNullOrWhiteSpace(localTimeString))
                        {
                            if (!DateTime.TryParse($"{localDateString} {localTimeString}", out eventDate))
                                continue;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    var venue = item["_embedded"]?["venues"]?.FirstOrDefault();
                    if (venue == null)
                        continue;

                    string? addressLine = venue["name"]?.ToString();
                    string? latStr = venue["location"]?["latitude"]?.ToString();
                    string? lonStr = venue["location"]?["longitude"]?.ToString();

                    if (!decimal.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal latitude))
                        latitude = 0;
                    if (!decimal.TryParse(lonStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal longitude))
                        longitude = 0;

                    var newEvent = new Event
                    {
                        EventName = eventName ?? string.Empty,
                        EventDescription = addressLine ?? string.Empty,
                        EventUrl = eventUrl ?? string.Empty,
                        Latitude = latitude,
                        Longitude = longitude,
                        EventDate = eventDate,
                        UserId = userId,
                        RequestGroupId = requestGroupId
                    };

                    await _eventRepository.AddEventAsync(newEvent);
                    newEventCount++;
                }

                return newEventCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while saving events: {ex.Message}");
                return newEventCount;
            }
        }

        // ===================================================================
        // TicketmasterService.cs'deki TransferEventsToTripsAsync METHOD'UNU
        // KOMPLE BU İLE DEĞİŞTİR (eski method'u sil, bunu yapıştır)
        // ===================================================================

        /// <summary>
        /// ✅ GÜNCELLEME: 
        /// 1) Eski trip'ler ARŞİVLENİYOR (IsActive = false)
        /// 2) Arşive gidecek rota zaten arşivde varsa → arşivlemek yerine SİLİNİYOR (duplicate engeli)
        /// </summary>
        public async Task<string> TransferEventsToTripsAsync(List<int> eventIds, List<GptEventComment>? comments = null)
        {
            var trips = new List<Trip>();
            int? firstUserId = null;

            foreach (var eventId in eventIds)
            {
                var eventItem = await _eventRepository.GetEventByIdAsync(eventId);
                if (eventItem == null)
                {
                    return $"Event with ID {eventId} not found.";
                }

                firstUserId ??= eventItem.UserId;

                var comment = comments?
                    .FirstOrDefault(c => c.EventId == eventId)?.Comment
                    ?? string.Empty;

                var newTrip = new Trip
                {
                    TripName = eventItem.EventName,
                    TripDescription = eventItem.EventDescription,
                    TripUrl = eventItem.EventUrl,
                    Latitude = eventItem.Latitude,
                    Longitude = eventItem.Longitude,
                    TripDate = eventItem.EventDate,
                    UserId = eventItem.UserId,
                    RequestGroupId = eventItem.RequestGroupId,
                    TripComment = comment,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                trips.Add(newTrip);
            }

            if (firstUserId.HasValue)
            {
                // ✅ Mevcut aktif trip'lerin arşivde duplicate olup olmadığını kontrol et
                var activeTrips = await _tripRepository.GetActiveTripsByUserIdAsync(firstUserId.Value);

                if (activeTrips.Any())
                {
                    bool activeRouteAlreadyArchived = await IsRouteAlreadyArchivedAsync(
                        firstUserId.Value, activeTrips);

                    if (activeRouteAlreadyArchived)
                    {
                        // Aynı rota zaten arşivde var → aktif olanları arşivlemek yerine sil
                        Console.WriteLine("[TicketmasterService] Aktif rota zaten arşivde mevcut, siliniyor (duplicate engeli).");
                        await _tripRepository.DeleteActiveTripsByUserIdAsync(firstUserId.Value);
                    }
                    else
                    {
                        // Farklı rota → normal şekilde arşivle
                        await _tripRepository.ArchiveTripsByUserIdAsync(firstUserId.Value);
                    }
                }
            }


            // ✅ Kullanıcının konumuna göre en yakından uzağa sırala
            if (firstUserId.HasValue)
            {
                var userLocation = await _locationRepository.GetLocationByUserIdAsync(firstUserId.Value);
                if (userLocation != null)
                {
                    trips = trips.OrderBy(t => HaversineDistance(
                        (double)userLocation.Latitude, (double)userLocation.Longitude,
                        (double)t.Latitude, (double)t.Longitude
                    )).ToList();
                }
            }

            // Yeni trip'leri aktif olarak kaydet
            foreach (var trip in trips)
            {
                await _tripRepository.AddTripAsync(trip);
            }
            

            return "Selected events have been successfully transferred to trips.";
        }

        /// <summary>
        /// Arşivde aynı etkinlik isimlerinden oluşan bir rota var mı kontrol eder.
        /// Etkinlik isimleri sıralanarak karşılaştırılır (sıra farketmez).
        /// </summary>
        private async Task<bool> IsRouteAlreadyArchivedAsync(int userId, List<Trip> currentTrips)
        {
            try
            {
                // Mevcut rotanın "parmak izi" → sıralı isimler
                var currentKey = string.Join("|",
                    currentTrips.Select(t => t.TripName.Trim().ToLowerInvariant()).OrderBy(n => n));

                var archivedTrips = await _tripRepository.GetArchivedTripsByUserIdAsync(userId);
                if (!archivedTrips.Any()) return false;

                // CreatedAt'e göre grupla (aynı anda oluşturulan trip'ler = 1 rota)
                var routeGroups = archivedTrips
                    .GroupBy(t => new DateTime(
                        t.CreatedAt.Year, t.CreatedAt.Month, t.CreatedAt.Day,
                        t.CreatedAt.Hour, t.CreatedAt.Minute, 0)) // Saniyeyi yoksay
                    .ToList();

                foreach (var group in routeGroups)
                {
                    var archivedKey = string.Join("|",
                        group.Select(t => t.TripName.Trim().ToLowerInvariant()).OrderBy(n => n));

                    if (archivedKey == currentKey)
                        return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DuplicateCheck] Error: {ex.Message}");
                return false; // Hata olursa duplicate sayma
            }
        }

        // ===================================================================
        // BU DOSYA KOMPLE YENİ DEĞİL!
        // TicketmasterService.cs'e EKLENECEK METHOD'LAR
        // Mevcut class'ın içine, son kapanan } 'den ÖNCE yapıştır.
        // ===================================================================

        /// <summary>
        /// Bildirim sistemi: Kullanıcının şehri + komşu şehirlerdeki yaklaşan etkinlikleri döner.
        /// Tercih eşleşenleri IsPreferenceMatch = true olarak işaretler.
        /// Veritabanına kaydetmez, sadece DTO listesi döner.
        /// </summary>
        public async Task<List<NearbyEventDto>> GetNearbyEventsAsync(int userId, double? overrideLat = null, double? overrideLng = null, string? cityOverride = null, DateTime? startDateTime = null, DateTime? endDateTime = null)
        {
            var result = new List<NearbyEventDto>();
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null) return result;

                var location = await _locationRepository.GetLocationByUserIdAsync(userId);
                if (location == null) return result;
                // Şehir adı direkt gönderildiyse konumu override et
                if (!string.IsNullOrWhiteSpace(cityOverride))
                {
                    location.City = cityOverride.Trim();
                    location.IsAutoDetected = false;
                }

                // ✅ GPS koordinatları gönderildiyse onları kullan
                if (overrideLat.HasValue && overrideLng.HasValue)
                {
                    // GPS koordinatından şehir tespit et
                    location.Latitude = (decimal)overrideLat.Value;
                    location.Longitude = (decimal)overrideLng.Value;
                    location.IsAutoDetected = false;

                    var detectedCity = Helpers.TurkeyGeoHelper.FindNearestCity(overrideLat.Value, overrideLng.Value);
                    if (!string.IsNullOrWhiteSpace(detectedCity))
                    {
                        location.City = detectedCity;
                        Console.WriteLine($"[Nearby] GPS detected city: {detectedCity}");
                    }
                }
                else
                {
                    // Profildeki koordinattan şehir tespit et (şehir adı yanlış olabilir)
                    var detectedCity = Helpers.TurkeyGeoHelper.FindNearestCity(
                        (double)location.Latitude, (double)location.Longitude);
                    if (!string.IsNullOrWhiteSpace(detectedCity))
                    {
                        Console.WriteLine($"[Nearby] Profile coord detected city: {detectedCity} (was: {location.City})");
                        location.City = detectedCity;
                    }
                }

                // Kullanıcının tercih listesini parse et
                var userPreferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrWhiteSpace(user.Preferences))
                {
                    foreach (var pref in user.Preferences.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var name = pref.Split(':')[0].Trim();
                        if (!string.IsNullOrWhiteSpace(name))
                            userPreferences.Add(name);
                    }
                }

                // Şehir + komşu şehirler
                var city = location.City ?? "";
                var cities = Helpers.TurkeyNeighborCities.GetCityAndNeighbors(city);




                if (!cities.Any())
                {
                    // Otomatik konum kullanılmışsa şehir adı boş olabilir
                    // Bu durumda latlong ile 100km çapında ara
                    var nearbyFromLatLong = await FetchEventsFromLatLongAsync(
    location.Latitude, location.Longitude, userPreferences, startDateTime, endDateTime); ;
                    result.AddRange(nearbyFromLatLong);
                    return SortAndLimit(result);
                }



                // Her şehir için Ticketmaster'dan etkinlik çek
                var startDate = startDateTime?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var endDate = endDateTime?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-ddTHH:mm:ssZ");

                foreach (var searchCity in cities)
                {
                    try
                    {
                        var cityEncoded = Uri.EscapeDataString(searchCity);
                        var url = $"{_baseUrl}city={cityEncoded}&countryCode=TR&apikey={_apiKey}" +
                                  $"&startDateTime={startDate}&endDateTime={endDate}" +
                                  $"&size=15&sort=date,asc";

                        Console.WriteLine($"[Nearby] Fetching events for city: {searchCity}");

                        var response = await _httpClient.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            var events = ParseNearbyEvents(content, searchCity, userPreferences);
                            result.AddRange(events);
                        }

                        // Ticketmaster rate limit: 5 req/sec → güvenli bekleme
                        await Task.Delay(250);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Nearby] Error fetching {searchCity}: {ex.Message}");
                    }
                }



                return SortAndLimit(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Nearby] General error: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Otomatik konum kullananlar için latlong bazlı arama
        /// </summary>
        private async Task<List<NearbyEventDto>> FetchEventsFromLatLongAsync(
    decimal latitude, decimal longitude, HashSet<string> userPreferences, DateTime? startDateTime = null, DateTime? endDateTime = null)
        {
            var result = new List<NearbyEventDto>();

            var latlong = $"{Math.Round(latitude, 6, MidpointRounding.AwayFromZero).ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                          $"{Math.Round(longitude, 6, MidpointRounding.AwayFromZero).ToString(System.Globalization.CultureInfo.InvariantCulture)}";

            var startDate = startDateTime?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var endDate = endDateTime?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-ddTHH:mm:ssZ"); ;

            var url = $"{_baseUrl}latlong={latlong}&radius=100&unit=km&countryCode=TR&apikey={_apiKey}" +
                      $"&startDateTime={startDate}&endDateTime={endDate}&size=30&sort=date,asc";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    result.AddRange(ParseNearbyEvents(content, "", userPreferences));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Nearby] LatLong fetch error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Ticketmaster JSON'ından NearbyEventDto listesi parse eder.
        /// Genre bilgisine bakarak tercih eşleşmelerini işaretler.
        /// </summary>
        private List<NearbyEventDto> ParseNearbyEvents(
            string jsonContent, string cityName, HashSet<string> userPreferences)
        {
            var events = new List<NearbyEventDto>();

            try
            {
                var jObject = Newtonsoft.Json.Linq.JObject.Parse(jsonContent);
                var eventsToken = jObject["_embedded"]?["events"];
                if (eventsToken == null) return events;

                foreach (var item in eventsToken)
                {
                    string? eventName = item["name"]?.ToString();
                    string? eventUrl = item["url"]?.ToString();
                    string? dateTimeString = item["dates"]?["start"]?["dateTime"]?.ToString();

                    DateTime eventDate;
                    if (!DateTime.TryParse(dateTimeString, out eventDate))
                    {
                        var localDateString = item["dates"]?["start"]?["localDate"]?.ToString();
                        var localTimeString = item["dates"]?["start"]?["localTime"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(localDateString) && !string.IsNullOrWhiteSpace(localTimeString))
                        {
                            if (!DateTime.TryParse($"{localDateString} {localTimeString}", out eventDate))
                                continue;
                        }
                        else if (!string.IsNullOrWhiteSpace(localDateString))
                        {
                            if (!DateTime.TryParse(localDateString, out eventDate))
                                continue;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    var venue = item["_embedded"]?["venues"]?.FirstOrDefault();
                    string venueName = venue?["name"]?.ToString() ?? "";
                    string venueCity = venue?["city"]?["name"]?.ToString() ?? cityName;

                    string? latStr = venue?["location"]?["latitude"]?.ToString();
                    string? lonStr = venue?["location"]?["longitude"]?.ToString();

                    decimal.TryParse(latStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal lat);
                    decimal.TryParse(lonStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal lon);

                    // Etkinlik görselini çek (ilk 3:2 oranındaki veya ilk görseli al)
                    string imageUrl = ExtractImageUrl(item);
                    string priceRange = ExtractPriceRange(item);
                    // Genre bilgisini çıkar
                    string genre = ExtractGenre(item);

                    // Tercih eşleşmesi kontrolü
                    bool isMatch = CheckPreferenceMatch(genre, item, userPreferences);

                    events.Add(new NearbyEventDto
                    {
                        EventName = eventName ?? "",
                        Venue = venueName,
                        City = venueCity,
                        EventUrl = eventUrl ?? "",
                        EventDate = eventDate,
                        Latitude = lat,
                        Longitude = lon,
                        Genre = genre,
                        IsPreferenceMatch = isMatch,
                        ImageUrl = imageUrl,
                        PriceRange = priceRange,
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Nearby] Parse error: {ex.Message}");
            }

            return events;
        }

        /// <summary>
        /// Ticketmaster event JSON'ından genre adını çıkarır.
        /// classifications → genre → name
        /// </summary>
        private string ExtractGenre(Newtonsoft.Json.Linq.JToken eventItem)
        {
            try
            {
                var classifications = eventItem["classifications"];
                if (classifications == null) return "Other";

                var first = classifications.FirstOrDefault();
                if (first == null) return "Other";

                // Önce genre'ye bak
                var genreName = first["genre"]?["name"]?.ToString();
                if (!string.IsNullOrWhiteSpace(genreName) && genreName != "Undefined")
                    return genreName;

                // Genre yoksa segment'e bak
                var segmentName = first["segment"]?["name"]?.ToString();
                if (!string.IsNullOrWhiteSpace(segmentName) && segmentName != "Undefined")
                    return segmentName;

                // SubGenre'ye bak
                var subGenreName = first["subGenre"]?["name"]?.ToString();
                if (!string.IsNullOrWhiteSpace(subGenreName) && subGenreName != "Undefined")
                    return subGenreName;

                return "Other";
            }
            catch
            {
                return "Other";
            }
        }

        /// <summary>
        /// Ticketmaster event JSON'ından en uygun görsel URL'ini çıkarır.
        /// Önce 3:2 oranındaki (mobil uyumlu) görseli arar, yoksa ilk görseli alır.
        /// </summary>
        private string ExtractImageUrl(Newtonsoft.Json.Linq.JToken eventItem)
        {
            try
            {
                var images = eventItem["images"];
                if (images == null || !images.Any()) return string.Empty;

                // Öncelik: 3:2 oranında ve genişliği 640+ olan görsel (mobilde iyi görünür)
                var preferred = images.FirstOrDefault(img =>
                    img["ratio"]?.ToString() == "3_2" &&
                    (int?)img["width"] >= 640);

                if (preferred != null)
                    return preferred["url"]?.ToString() ?? string.Empty;

                // 3:2 olan herhangi biri
                var any3x2 = images.FirstOrDefault(img =>
                    img["ratio"]?.ToString() == "3_2");

                if (any3x2 != null)
                    return any3x2["url"]?.ToString() ?? string.Empty;

                // Hiç yoksa ilk görseli al
                return images.First()["url"]?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string ExtractPriceRange(JToken eventItem)
        {
            try
            {
                var priceRanges = eventItem["priceRanges"];
                if (priceRanges == null || !priceRanges.Any()) return string.Empty;

                var first = priceRanges.First();
                var min = first["min"]?.ToString();
                var max = first["max"]?.ToString();
                var currency = first["currency"]?.ToString() ?? "TRY";

                if (!string.IsNullOrEmpty(min) && !string.IsNullOrEmpty(max))
                    return $"{min} - {max} {currency}";
                if (!string.IsNullOrEmpty(min))
                    return $"{min} {currency}'den başlayan";

                return string.Empty;
            }
            catch { return string.Empty; }
        }

        /// <summary>
        /// Kullanıcı tercihlerinin bu etkinlikle eşleşip eşleşmediğini kontrol eder.
        /// Genre adı, segment adı veya subGenre adı ile karşılaştırır.
        /// </summary>
        private bool CheckPreferenceMatch(string genre, Newtonsoft.Json.Linq.JToken eventItem, HashSet<string> userPreferences)
        {
            if (userPreferences.Count == 0) return false;

            // Doğrudan genre eşleşmesi
            if (userPreferences.Contains(genre)) return true;

            try
            {
                var classifications = eventItem["classifications"]?.FirstOrDefault();
                if (classifications == null) return false;

                // Segment bazlı eşleşme: Music → Pop, Rock, Hip-Hop/Rap, Alternative
                var segmentName = classifications["segment"]?["name"]?.ToString() ?? "";
                var genreName = classifications["genre"]?["name"]?.ToString() ?? "";
                var subGenreName = classifications["subGenre"]?["name"]?.ToString() ?? "";

                // Tüm isimleri kontrol et
                foreach (var pref in userPreferences)
                {
                    if (string.Equals(pref, genreName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(pref, subGenreName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(pref, segmentName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    // Özel eşleşmeler
                    if (pref.Equals("Comedy", StringComparison.OrdinalIgnoreCase) &&
                        genreName.Contains("Comedy", StringComparison.OrdinalIgnoreCase))
                        return true;

                    if (pref.Equals("Theatre", StringComparison.OrdinalIgnoreCase) &&
                        (segmentName.Contains("Arts", StringComparison.OrdinalIgnoreCase) ||
                         genreName.Contains("Theatre", StringComparison.OrdinalIgnoreCase)))
                        return true;
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Sonuçları sıralar: Tercih eşleşenler önce, sonra tarihe göre.
        /// Duplicate'ları kaldırır, maksimum 50 etkinlik döner.
        /// </summary>
        private List<NearbyEventDto> SortAndLimit(List<NearbyEventDto> events)
        {
            return events
                .GroupBy(e => new { e.EventName, e.EventDate })
                .Select(g => g.First())
                .OrderByDescending(e => e.IsPreferenceMatch)
                .ThenBy(e => e.EventDate)
                .Take(50)
                .ToList();
        }



        private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        }
    }

}
