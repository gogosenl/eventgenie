# EventGenie

Ticketmaster API ve yapay zeka destekli etkinlik keşif ve rota planlama uygulaması.

Kullanıcılar seyahat güzergahlarını girerek o rota üzerindeki etkinlikleri keşfedebilir, rota oluşturabilir ve Google Maps üzerinden güzergah görüntüleyebilir.

## Özellikler

- Kullanıcı kayıt, giriş ve profil yönetimi
- Şehir ve tarih bazlı etkinlik arama (Ticketmaster API)
- Güzergah üzerindeki illerdeki etkinlikleri listeleme
- Etkinlikleri rotaya ekleme ve Google Maps ile güzergah görüntüleme
- Konuma göre yakındaki etkinlikleri keşfetme
- Aktif ve geçmiş rota yönetimi
- Etkinlik bazlı chat odaları

## Teknolojiler

**Backend:** ASP.NET Core 8, Entity Framework Core, MySQL, Ticketmaster Discovery API

**Frontend:** React 18, Vite, React Router v6, Axios

## Proje Yapısı
eventgenie/
├── backend/    # ASP.NET Core Web API
└── frontend/   # React.js Web Uygulaması

## Kurulum

### Gereksinimler
- .NET 8 SDK
- Node.js 18+
- MySQL (XAMPP önerilir)
- Ticketmaster API Key (developer.ticketmaster.com)

### Backend

```bash
cd backend
# MySQL'de eventgenie veritabanı oluştur
# appsettings.json dosyasını düzenle (DB bağlantısı ve API key)
dotnet ef database update
dotnet run
```

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Uygulama `http://localhost:3000` adresinde çalışır.

## API

Backend `http://localhost:5118` adresinde çalışır. Başlıca endpoint'ler:

- `POST /api/User/Login` — Giriş
- `POST /api/User` — Kayıt
- `GET /api/Event/nearby/{userId}` — Yakındaki etkinlikler
- `GET /api/Trip/active/{userId}` — Aktif rotalar
- `GET /api/Location/by-user/{userId}` — Kullanıcı konumu
