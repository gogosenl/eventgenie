using EventGenie.Interfaces;
using EventGenie.Models;
using EventGenie.Repositorys;
using Microsoft.AspNetCore.Mvc;

namespace EventGenie.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepository;
        private readonly IServiceScopeFactory _serviceScopeFactory;  // ✅ YENİ

        public ChatController(IChatRepository chatRepository, IServiceScopeFactory serviceScopeFactory)
        {
            _chatRepository = chatRepository;
            _serviceScopeFactory = serviceScopeFactory;  // ✅ YENİ
        }

        // ===================================================================
        // ChatController.cs'deki GetMessages METHOD'UNU BU İLE DEĞİŞTİR
        // ===================================================================

        [HttpGet("{roomKey}")]
        public async Task<IActionResult> GetMessages(string roomKey)
        {
            var decodedKey = Uri.UnescapeDataString(roomKey).Replace("+", " ");
            var messages = await _chatRepository.GetMessagesByRoomKeyAsync(decodedKey);

            // ✅ DEMO: Oda boşsa otomatik seed yap (ilk giriş)
            if (!messages.Any())
            {
                await SeedInitialConversation(decodedKey);
                messages = await _chatRepository.GetMessagesByRoomKeyAsync(decodedKey);
            }

            var participantCount = await _chatRepository.GetParticipantCountAsync(decodedKey);

            return Ok(new
            {
                Success = true,
                RoomKey = decodedKey,
                ParticipantCount = participantCount,
                Messages = messages
            });
        }

        /// <summary>
        /// DEMO: Sohbet odasına ilk girişte doğal bir konuşma oluşturur
        /// </summary>
        private async Task SeedInitialConversation(string roomKey)
        {
            var conversations = new[]
            {
                new { Id = 900, Name = "Elif Yılmaz",    Msg = $"Beyler {roomKey} etkinliğine gidecek var mı?" },
                new { Id = 901, Name = "Ahmet Kaya",     Msg = "Ben gidiyorum! Biletimi dün aldım 🎉" },
                new { Id = 902, Name = "Zeynep Demir",   Msg = "Ben de gideceğim, çok heyecanlıyım!" },
                new { Id = 900, Name = "Elif Yılmaz",    Msg = "Süper! Kaçta buluşalım orada?" },
                new { Id = 903, Name = "Burak Çelik",    Msg = "Bence 1 saat önceden gidelim, kalabalık olur" },
                new { Id = 901, Name = "Ahmet Kaya",     Msg = "Aynen katılıyorum, iyi yer kapmak lazım" },
                new { Id = 904, Name = "Ayşe Korkmaz",   Msg = "Ben de gelmek istiyorum ama bilet kaldı mı acaba?" },
                new { Id = 902, Name = "Zeynep Demir",   Msg = "Daha var, ama acele et fiyatlar artıyor!" },
                new { Id = 903, Name = "Burak Çelik",    Msg = "Etkinlik sonrası bir şeyler yiyelim mi hep birlikte?" },
                new { Id = 900, Name = "Elif Yılmaz",    Msg = "Harika fikir! Yakınlarda güzel mekanlar var 😊" },
            };

            var baseTime = DateTime.UtcNow.AddHours(-2);
            var random = new Random();

            for (int i = 0; i < conversations.Length; i++)
            {
                var c = conversations[i];
                await _chatRepository.AddMessageAsync(new ChatMessage
                {
                    RoomKey = roomKey,
                    UserId = c.Id,
                    UserName = c.Name,
                    Message = c.Msg,
                    CreatedAt = baseTime.AddMinutes(i * random.Next(3, 12))
                });
            }
        }

        // ===================================================================
        // ChatController.cs'deki SendMessage METHOD'UNU KOMPLE BU İLE DEĞİŞTİR
        // ===================================================================

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RoomKey) || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { Success = false, Message = "RoomKey ve Message zorunludur." });

            var chatMessage = new ChatMessage
            {
                RoomKey = request.RoomKey,
                UserId = request.UserId,
                UserName = request.UserName,
                Message = request.Message.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _chatRepository.AddMessageAsync(chatMessage);

            // ✅ DEMO: Otomatik yanıt oluştur (gerçek kullanıcı gibi)
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(2000); // 2 saniye bekle (gerçekçi olsun)
                    var reply = GenerateSmartReply(request.Message, request.UserName);

                    using var scope = _serviceScopeFactory.CreateScope();
                    var chatRepo = scope.ServiceProvider.GetRequiredService<IChatRepository>();

                    await chatRepo.AddMessageAsync(new ChatMessage
                    {
                        RoomKey = request.RoomKey,
                        UserId = reply.UserId,
                        UserName = reply.UserName,
                        Message = reply.Message,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AutoReply] Error: {ex.Message}");
                }
            });

            return Ok(new { Success = true, Message = "Mesaj gönderildi.", Data = chatMessage });
        }

        // ===================================================================
        // BU HELPER CLASS VE METHOD'LARI DA AYNI CONTROLLER'A EKLE
        // ===================================================================

        private static AutoReplyResult GenerateSmartReply(string userMessage, string userName)
        {
            var random = new Random();
            var msg = userMessage.ToLowerInvariant();

            var fakeUsers = new[]
            {
                new { Id = 900, Name = "Elif Yılmaz" },
                new { Id = 901, Name = "Ahmet Kaya" },
                new { Id = 902, Name = "Zeynep Demir" },
                new { Id = 903, Name = "Burak Çelik" },
                new { Id = 904, Name = "Ayşe Korkmaz" },
            };

            var user = fakeUsers[random.Next(fakeUsers.Length)];
            string reply;

            // Mesaja göre akıllı yanıt
            if (msg.Contains("merhaba") || msg.Contains("selam") || msg.Contains("hey"))
            {
                var greetings = new[]
                {
                    $"Selam {userName}! Ben de bu etkinliğe gidiyorum 🎉",
                    $"Merhaba! Hoş geldin, sen de mi bilet aldın?",
                    $"Selamm! Etkinlikte görüşürüz 😊",
                    $"Hey! Birlikte gitsek çok eğlenceli olur!"
                };
                reply = greetings[random.Next(greetings.Length)];
            }
            else if (msg.Contains("bilet") || msg.Contains("aldım") || msg.Contains("aldim"))
            {
                var ticketReplies = new[]
                {
                    "Harika! Ben de biletimi geçen hafta aldım, ön sıralar kalmıştı!",
                    "Süper! Kaçıncı bölgeden aldın?",
                    "Ben de aldım! VIP biletler çok pahalıydı ama normal alan da güzel",
                    "Güzel! Erken almak en iyisi, fiyatlar artıyor sonra"
                };
                reply = ticketReplies[random.Next(ticketReplies.Length)];
            }
            else if (msg.Contains("kaçta") || msg.Contains("saat") || msg.Contains("ne zaman"))
            {
                var timeReplies = new[]
                {
                    "Kapılar genelde 1 saat önce açılıyor, erken gitsen iyi olur!",
                    "Ben 1 saat önceden orada olacağım, iyi yer kapmak lazım 😄",
                    "Etkinlik saatinde git yeter ama kalabalık olabilir",
                    "Erken gidip önce çevreyi gezelim mi?"
                };
                reply = timeReplies[random.Next(timeReplies.Length)];
            }
            else if (msg.Contains("buluş") || msg.Contains("gidel") || msg.Contains("beraber") || msg.Contains("birlikte"))
            {
                var meetReplies = new[]
                {
                    "Olur! Giriş kapısının önünde buluşalım mı?",
                    "Harika fikir! WhatsApp grubumuz var istersen ekleyebilirim",
                    "Ben de yalnız gidecektim, beraber çok daha iyi olur!",
                    "Tabi! Ben arabayla geleceğim, yer varsa alabilirim sizi"
                };
                reply = meetReplies[random.Next(meetReplies.Length)];
            }
            else if (msg.Contains("yemek") || msg.Contains("yiyelim") || msg.Contains("restoran") || msg.Contains("kafe"))
            {
                var foodReplies = new[]
                {
                    "Yakınlarda güzel bir burger mekanı var, oraya gidelim!",
                    "Etkinlik sonrası pizza söyleyelim mi? 🍕",
                    "Ben açlıktan ölüyorum zaten, kesinlikle bir şeyler yiyelim",
                    "Mekanın içinde de yiyecek satılıyor galiba, ama dışarısı daha uygun"
                };
                reply = foodReplies[random.Next(foodReplies.Length)];
            }
            else if (msg.Contains("heyecan") || msg.Contains("sabırsız") || msg.Contains("harika") || msg.Contains("süper"))
            {
                var excitedReplies = new[]
                {
                    "Ben de çok heyecanlıyım! Bu sene en iyi etkinlik olacak!",
                    "Aynı şekilde! Geri sayım başladı 🔥",
                    "Daha önce gittin mi bu tarz etkinliklere? Müthiş oluyor!",
                    "Aynen! Arkadaşlarıma da söyledim herkes kıskanıyor 😄"
                };
                reply = excitedReplies[random.Next(excitedReplies.Length)];
            }
            else if (msg.Contains("nasıl") || msg.Contains("nası") || msg.Contains("ulaşım") || msg.Contains("arabayla") || msg.Contains("otobüs"))
            {
                var transportReplies = new[]
                {
                    "Ben arabayla geleceğim, otopark var mı bilen var mı?",
                    "Metro ile geliyorum, en yakın durak 10 dk yürüme mesafesinde",
                    "Taksi tutabiliriz beraber, daha uygun olur",
                    "Google Maps'ten baktım, 30 dk sürüyor arabayla"
                };
                reply = transportReplies[random.Next(transportReplies.Length)];
            }
            else
            {
                // Genel cevaplar
                var generalReplies = new[]
                {
                    "Kesinlikle katılıyorum! 👍",
                    "Aynen bence de! Etkinlikte görüşürüz",
                    "Çok haklısın, ben de aynısını düşünüyorum",
                    "İyi fikir! Bunu konuşalım etkinlikte",
                    "Haha evet! Bu etkinlik çok güzel geçecek 😊",
                    "Aynı düşüncedeyim! Birlikte güzel vakit geçiririz",
                    "Not aldım! Etkinlikte hatırlat bana 📝",
                    "Süper! Çok eğlenceli olacak hep beraber",
                    $"Güzel söyledin {userName}! Katılıyorum",
                    "Merak etme her şey güzel olacak, sabırsızlanıyorum!"
                };
                reply = generalReplies[random.Next(generalReplies.Length)];
            }

            return new AutoReplyResult
            {
                UserId = user.Id,
                UserName = user.Name,
                Message = reply
            };
        }

        private class AutoReplyResult
        {
            public int UserId { get; set; }
            public string UserName { get; set; } = "";
            public string Message { get; set; } = "";
        }





        // ===================================================================
        // ChatController.cs'e EKLENECEK ENDPOINT'LER
        // Mevcut endpoint'lerden sonra yapıştır.
        // ===================================================================

        /// <summary>
        /// DEMO İÇİN: Belirli bir sohbet odasına sahte kullanıcı mesajları ekler.
        /// Sunumda gerçekçi görünmesi için kullanılır.
        /// </summary>
        [HttpPost("seed/{roomKey}")]
        public async Task<IActionResult> SeedMessages(string roomKey)
        {
            var decodedKey = Uri.UnescapeDataString(roomKey).Replace("+", " ");

            var fakeUsers = new[]
            {
                new { Id = 900, Name = "Elif Yılmaz" },
                new { Id = 901, Name = "Ahmet Kaya" },
                new { Id = 902, Name = "Zeynep Demir" },
                new { Id = 903, Name = "Burak Çelik" },
                new { Id = 904, Name = "Ayşe Korkmaz" },
            };

            var fakeMessages = new[]
            {
                "Bu etkinliğe gidecek var mı? Ben biletimi aldım!",
                "Ben de gidiyorum, çok heyecanlıyım!",
                "Kaçta buluşalım orada?",
                "Etkinlik başlamadan 1 saat önce orada olacağım",
                "Benim de biletim var! Beraber gitsek mi?",
                "Hangi bölümde oturuyorsunuz?",
                "Ben ön sıralardayım, siz?",
                "Etkinlik sonrası bir şeyler yiyelim mi?",
                "Harika fikir, ben de tam onu düşünüyordum!",
                "Yakınlarda güzel bir restoran var, oraya gidelim",
                "Etkinliğe nasıl gidiyorsunuz? Arabayla mı?",
                "Ben toplu taşımayla gideceğim",
                "Birlikte taksi tutsak daha uygun olur bence",
                "Hava durumunu kontrol ettiniz mi o gün için?",
                "Güneşli görünüyor, dışarıda da vakit geçirebiliriz",
            };

            var random = new Random();
            var baseTime = DateTime.UtcNow.AddHours(-3); // 3 saat önce başlamış gibi

            var seededCount = 0;
            for (int i = 0; i < fakeMessages.Length; i++)
            {
                var user = fakeUsers[random.Next(fakeUsers.Length)];

                var msg = new ChatMessage
                {
                    RoomKey = decodedKey,
                    UserId = user.Id,
                    UserName = user.Name,
                    Message = fakeMessages[i],
                    CreatedAt = baseTime.AddMinutes(i * random.Next(5, 20))
                };

                await _chatRepository.AddMessageAsync(msg);
                seededCount++;
            }

            return Ok(new
            {
                Success = true,
                Message = $"{seededCount} sahte mesaj eklendi: {decodedKey}",
                RoomKey = decodedKey
            });
        }

        /// <summary>
        /// DEMO İÇİN: Tüm sohbet mesajlarını temizler (reset).
        /// </summary>
        [HttpDelete("clear/{roomKey}")]
        public async Task<IActionResult> ClearMessages(string roomKey)
        {
            var decodedKey = Uri.UnescapeDataString(roomKey).Replace("+", " ");
            var messages = await _chatRepository.GetMessagesByRoomKeyAsync(decodedKey);

            foreach (var msg in messages)
            {
                await _chatRepository.DeleteMessageAsync(msg.ChatMessageId);
            }

            return Ok(new { Success = true, Message = $"Tüm mesajlar silindi: {decodedKey}" });
        }
    }
}