# Match-iSports .NET Core Backend

Bu backend, Next.js frontend'iniz için .NET Core ile yeniden yazılmış API'dir.

## 🚀 Kurulum

### 1. Gereksinimler
- .NET 10 SDK
- PostgreSQL (Railway)

### 2. Yapılandırma

`appsettings.json` dosyasını düzenleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "BURAYA_RAILWAY_POSTGRESQL_CONNECTION_STRING"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256Algorithm",
    "Issuer": "MatchISportsAPI",
    "Audience": "MatchISportsClient",
    "ExpirationInDays": 30
  },
  "Cloudinary": {
    "CloudName": "YOUR_CLOUD_NAME",
    "ApiKey": "YOUR_API_KEY",
    "ApiSecret": "YOUR_API_SECRET"
  }
}
```

### 3. Çalıştırma

```bash
cd backend
dotnet restore
dotnet run
```

API varsayılan olarak https://localhost:7000 üzerinde çalışacak.

## 📚 API Endpoints

### Authentication
- `POST /api/auth/register` - Kayıt ol
- `POST /api/auth/login` - Giriş yap

### User
- `GET /api/profile` - Profil bilgisi
- `PUT /api/profile` - Profil güncelle
- `GET /api/users` - Kullanıcı listesi
- `GET /api/user/stats` - Kullanıcı istatistikleri
- `GET /api/activities` - Aktivite geçmişi
- `POST /api/upload-image` - Resim yükle

### Matches
- `GET /api/matches` - Maç listesi
- `POST /api/matches` - Maç oluştur
- `POST /api/matches/{matchId}/request` - Maça istek gönder
- `GET /api/matches/{matchId}/requests` - Maç istekleri

### Teams
- `GET /api/teams/search` - Takım ara
- `GET /api/teams/{id}` - Takım detayı
- `POST /api/teams/request` - Takıma istek gönder
- `GET /api/teams/incoming-requests` - Gelen istekler
- `GET /api/teams/my-requests` - Gönderilen istekler
- `PUT /api/teams/{id}/request` - İstek durumunu güncelle

### Messages & Chat
- `GET /api/messages` - Mesajları getir
- `POST /api/messages` - Mesaj gönder
- `GET /api/match-history` - Maç geçmişi
- `POST /api/match-history` - Maç geçmişi ekle

### SignalR Hub
- `/chatHub` - Real-time chat endpoint

## 🔧 Frontend Entegrasyonu

Next.js frontend'inizde API URL'sini güncellemelisiniz:

```typescript
// .env.local
NEXT_PUBLIC_API_URL=http://localhost:5000
```

API çağrılarında `/api` prefix'i kullanılmalı:
```typescript
// Örnek
const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/auth/login`, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({ email, password })
});
```

## 🔐 Authentication

JWT token kullanılıyor. Her korumalı endpoint için header'da gönderilmeli:

```typescript
headers: {
  'Authorization': `Bearer ${token}`
}
```

## 🗄️ Veritabanı

Railway PostgreSQL veritabanınız aynen kullanılıyor. Prisma migration'larınız zaten tablolarını oluşturmuş durumda, Entity Framework Core aynı tabloları kullanacak.

## 📦 Deployment (Railway)

1. Railway'de yeni bir servis oluşturun
2. .NET buildpack'i seçin
3. Environment variables ekleyin:
   - `ConnectionStrings__DefaultConnection`
   - `Jwt__Key`
   - `Cloudinary__CloudName`
   - `Cloudinary__ApiKey`
   - `Cloudinary__ApiSecret`

## 🔄 Farklar

### Node.js → .NET Core
- **NextAuth** → **JWT Bearer Authentication**
- **Prisma Client** → **Entity Framework Core**
- **Socket.IO** → **SignalR**
- **bcryptjs** → **BCrypt.Net-Next**

Tüm API endpoint'leri aynı şekilde çalışıyor!
