# EventGenie Web — React.js Frontend

## Kurulum

```bash
cd eventgenie-web
npm install
npm run dev
```

Uygulama → http://localhost:3000

## Proje Yapısı

```
src/
├── context/
│   ├── AuthContext.jsx      # Login/logout/register state
│   └── ToastContext.jsx     # Bildirim sistemi
├── services/
│   └── api.js               # Tüm API endpoint'leri (axios)
├── components/
│   └── layout/
│       ├── Sidebar.jsx      # Sol menü (daraltılabilir)
│       └── AppLayout.jsx    # Auth guard + layout wrapper
├── pages/
│   ├── LoginPage.jsx        # Giriş
│   ├── RegisterPage.jsx     # 2 adımlı kayıt
│   ├── DashboardPage.jsx    # Ana panel
│   ├── EventsPage.jsx       # Etkinlik getir + GPT öneri
│   ├── TripsPage.jsx        # Aktif/geçmiş rotalar
│   ├── NearbyPage.jsx       # Yakındaki etkinlikler
│   ├── ChatPage.jsx         # Etkinlik bazlı chat
│   └── ProfilePage.jsx      # Profil, konum, şifre
├── index.css                # Global design system
└── main.jsx                 # Router kurulumu
```

## Backend Bağlantısı

`vite.config.js` dosyasında proxy ayarı:
```js
proxy: {
  '/api': { target: 'http://localhost:5000' }
}
```

Backend farklı portta çalışıyorsa bu satırı değiştirin.

## Özellikler

- **Login / Kayıt** — 2 adımlı kayıt, tercih seçimi
- **Dashboard** — Özet istatistikler, aktif rota önizleme
- **Etkinlikler** — Tarih aralığı seç → Ticketmaster + GPT öneri
- **Rotalarım** — Aktif/geçmiş, tarihli gruplama, silme
- **Yakınımda** — GPS veya profil konumuyla etkinlik bul
- **Chat** — Etkinlik odası bazlı gerçek zamanlı chat (polling)
- **Profil** — Tercihler, konum, şifre değiştirme
