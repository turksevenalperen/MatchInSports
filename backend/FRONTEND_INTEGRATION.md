# Frontend'i .NET Core Backend'e Bağlama Rehberi

## 1. Environment Variables (.env.local)

Next.js projenizde `.env.local` dosyası oluşturun:

```env
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=your-secret-key-here
```

## 2. API Client Örneği

```typescript
// lib/api.ts
const API_URL = process.env.NEXT_PUBLIC_API_URL;

export async function apiFetch(endpoint: string, options: RequestInit = {}) {
  const token = localStorage.getItem('token'); // veya cookie'den alın
  
  const headers = {
    'Content-Type': 'application/json',
    ...(token && { 'Authorization': `Bearer ${token}` }),
    ...options.headers,
  };

  const response = await fetch(`${API_URL}${endpoint}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    throw new Error(`API Error: ${response.statusText}`);
  }

  return response.json();
}

// Login example
export async function login(email: string, password: string) {
  return apiFetch('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  });
}

// Register example
export async function register(data: RegisterDto) {
  return apiFetch('/api/auth/register', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

// Get matches
export async function getMatches(filters?: { sport?: string; city?: string }) {
  const params = new URLSearchParams(filters as any);
  return apiFetch(`/api/matches?${params}`);
}
```

## 3. SignalR Entegrasyonu

Socket.IO yerine SignalR kullanacaksınız:

```bash
npm install @microsoft/signalr
```

```typescript
// lib/signalr.ts
import * as signalR from "@microsoft/signalr";

export function createChatConnection(token: string) {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${process.env.NEXT_PUBLIC_API_URL}/chatHub`, {
      accessTokenFactory: () => token
    })
    .withAutomaticReconnect()
    .build();

  return connection;
}

// Kullanım örneği
const connection = createChatConnection(token);

await connection.start();

// Mesaj gönder
connection.invoke("SendMessage", receiverId, message);

// Mesaj al
connection.on("ReceiveMessage", (data) => {
  console.log("New message:", data);
});
```

## 4. Authentication Context

```typescript
// contexts/AuthContext.tsx
import { createContext, useContext, useState, useEffect } from 'react';
import { login as apiLogin, register as apiRegister } from '@/lib/api';

interface AuthContextType {
  user: User | null;
  token: string | null;
  login: (email: string, password: string) => Promise<void>;
  register: (data: RegisterDto) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    // Load from localStorage
    const savedToken = localStorage.getItem('token');
    const savedUser = localStorage.getItem('user');
    
    if (savedToken && savedUser) {
      setToken(savedToken);
      setUser(JSON.parse(savedUser));
    }
  }, []);

  const login = async (email: string, password: string) => {
    const response = await apiLogin(email, password);
    setToken(response.token);
    setUser(response.user);
    localStorage.setItem('token', response.token);
    localStorage.setItem('user', JSON.stringify(response.user));
  };

  const register = async (data: RegisterDto) => {
    const response = await apiRegister(data);
    setToken(response.token);
    setUser(response.user);
    localStorage.setItem('token', response.token);
    localStorage.setItem('user', JSON.stringify(response.user));
  };

  const logout = () => {
    setToken(null);
    setUser(null);
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  };

  return (
    <AuthContext.Provider value={{ user, token, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
};
```

## 5. API Endpoint Karşılaştırması

### Eski (Next.js API Routes) → Yeni (.NET Core)

| Özellik | Next.js | .NET Core |
|---------|---------|-----------|
| Register | `/api/auth/register` | `/api/auth/register` ✓ |
| Login | `/api/auth/register` | `/api/auth/login` ✓ |
| Profile | `/api/profile` | `/api/profile` ✓ |
| Users List | `/api/users` | `/api/users` ✓ |
| Matches | `/api/matches` | `/api/matches` ✓ |
| Create Match | `/api/matches` | `/api/matches` ✓ |
| Team Search | `/api/teams/search` | `/api/teams/search` ✓ |
| Messages | `/api/messages` | `/api/messages` ✓ |
| Activities | `/api/activities` | `/api/activities` ✓ |
| Upload Image | `/api/upload-image` | `/api/upload-image` ✓ |

**Tüm endpoint'ler aynı!** Sadece base URL'i değiştirmeniz yeterli.

## 6. Projeyi Çalıştırma

### Backend:
```bash
cd backend
dotnet run
```

API: https://localhost:7000 veya http://localhost:5000

### Frontend:
```bash
npm run dev
```

Frontend: http://localhost:3000

## 7. Railway Deployment

### Backend (.NET Core):
1. Railway'de yeni bir servis oluşturun
2. GitHub repo'nuzu bağlayın
3. Build Command: `dotnet build --configuration Release`
4. Start Command: `dotnet run --configuration Release`
5. Environment variables ekleyin (yukarıda listelenmiş)

### Frontend (Next.js):
- Şu anki Next.js projeniz aynen çalışmaya devam edebilir
- Sadece `NEXT_PUBLIC_API_URL` environment variable'ını .NET Core backend'inizin Railway URL'sine güncelleyin

## 8. Notlar

- ✅ PostgreSQL veritabanınız değişmiyor
- ✅ Tüm tablolar aynı kalıyor
- ✅ API endpoint'leri aynı
- ⚠️ Socket.IO → SignalR değişikliği gerekiyor
- ⚠️ NextAuth → JWT Bearer token kullanımı
