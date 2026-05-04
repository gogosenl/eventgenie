# EventGenie Backend API

ASP.NET Core ile geliştirilmiş etkinlik keşif ve rota planlama uygulamasının backend API'si.

## Teknolojiler

- ASP.NET Core 8.0
- Entity Framework Core
- MySQL
- Ticketmaster Discovery API

## Kurulum

### Gereksinimler
- .NET 8 SDK
- MySQL (XAMPP önerilir)

### Adımlar

1. Repoyu klonla
```bash
git clone https://github.com/kullanici_adi/eventgenie-backend.git
```

2. Veritabanını oluştur
MySQL'de eventgenie adında veritabanı oluştur

3. `appsettings.json` dosyasını düzenle
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=eventgenie;User=root;Password=;"
  },
  "TicketmasterAPI": {
    "ApiKey": "SENIN_API_KEY"
  }
}
```

4. Migration çalıştır
```bash
dotnet ef database update
```

5. Projeyi başlat
```bash
dotnet run
```

API `http://localhost:5118` adresinde çalışır.

## API Endpoints

### User
| Method | Endpoint | Açıklama |
|--------|---------|---------|
| GET | /api/User | Tüm kullanıcılar |
| GET | /api/User/{id} | Kullanıcı getir |
| POST | /api/User | Kayıt ol |
| PUT | /api/User/{id} | Güncelle |
| DELETE | /api/User/{id} | Sil |
| POST | /api/User/Login | Giriş yap |
| PATCH | /api/User/{id}/preferences | Tercih güncelle |
| PATCH | /api/User/{id}/change-password | Şifre değiştir |

### Event
| Method | Endpoint | Açıklama |
|--------|---------|---------|
| GET | /api/Event/nearby/{userId} | Yakındaki etkinlikler |

### Trip
| Method | Endpoint | Açıklama |
|--------|---------|---------|
| GET | /api/Trip/by-user/{userId} | Kullanıcı triplerı |
| GET | /api/Trip/active/{userId} | Aktif rotalar |
| GET | /api/Trip/archived/{userId} | Geçmiş rotalar |
| POST | /api/Trip | Rota oluştur |
| PUT | /api/Trip/{id} | Güncelle |
| DELETE | /api/Trip/{id} | Sil |
| DELETE | /api/Trip/active/{userId} | Aktif rotayı temizle |

### Location
| Method | Endpoint | Açıklama |
|--------|---------|---------|
| GET | /api/Location/by-user/{userId} | Kullanıcı konumu |
| POST | /api/Location | Konum ekle |
| PUT | /api/Location/{userId} | Konum güncelle |

### Chat
| Method | Endpoint | Açıklama |
|--------|---------|---------|
| GET | /api/Chat/{roomKey} | Mesajları getir |
| POST | /api/Chat | Mesaj gönder |
| DELETE | /api/Chat/{id} | Mesaj sil |

## Önemli Notlar

- `appsettings.json` dosyasını `.gitignore`'a ekle (API key ve şifre içeriyor)
- Şifreler şu an plain text, production için bcrypt kullanılmalı

React klasöründe de README.md oluştur:
markdown# EventGenie Web

React.js ile geliştirilmiş EventGenie uygulamasının web frontend'i.

## Teknolojiler

- React 18
- Vite
- React Router v6
- Axios

## Kurulum

### Gereksinimler
- Node.js 18+
- EventGenie Backend API çalışıyor olmalı

### Adımlar

1. Repoyu klonla
```bash
git clone https://github.com/kullanici_adi/eventgenie-web.git
```

2. Bağımlılıkları yükle
```bash
npm install
```

3. Backend portunu ayarla — `vite.config.js` dosyasında:
```js
proxy: {
  '/api': {
    target: 'http://localhost:5118', // Backend portun
    changeOrigin: true,
  }
}
```

4. Başlat
```bash
npm run dev
```

Uygulama `http://localhost:3000` adresinde çalışır.

## Özellikler

- Kullanıcı kayıt ve giriş
- Konuma göre yakındaki etkinlikleri keşfet
- Şehir ve tarih bazlı etkinlik arama
- Güzergah oluşturma ve Google Maps entegrasyonu
- Etkinlik bazlı chat odaları
- Aktif ve geçmiş rota yönetimi