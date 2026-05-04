\# EventGenie



Etkinlik keşif ve rota planlama uygulaması.



\## Proje Yapısı

eventgenie/

├── backend/    # ASP.NET Core Web API

└── frontend/   # React.js Web Uygulaması



\## Kurulum



\### Backend

```bash

cd backend

\# MySQL'de eventgenie veritabanı oluştur

\# appsettings.json'u düzenle

dotnet ef database update

dotnet run

```



\### Frontend

```bash

cd frontend

npm install

npm run dev

```



\## Teknolojiler



\*\*Backend:\*\* ASP.NET Core 8, Entity Framework, MySQL, Ticketmaster API



\*\*Frontend:\*\* React 18, Vite, React Router, Axios

