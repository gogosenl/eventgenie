# EventGenie

Etkinlik keþif ve rota planlama uygulamasý.

## Proje Yapýsý
eventgenie/ ??? backend/ # ASP.NET Core Web API ??? frontend/ # React.js Web Uygulamasý

## Kurulum

### Backend
```bash
cd backend
# MySQL'de eventgenie veritabaný oluþtur
# appsettings.json'u düzenle
dotnet ef database update
dotnet run
```

### Frontend
```bash
cd frontend
npm install
npm run dev
```

## Teknolojiler

**Backend:** ASP.NET Core 8, Entity Framework, MySQL, Ticketmaster API

**Frontend:** React 18, Vite, React Router, Axios

