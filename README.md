# SmartRoutine

SmartRoutine, kullanıcıların günlük ve haftalık rutinlerini kolayca oluşturup takip edebileceği, istatistik ve hatırlatıcı özellikleriyle öne çıkan modern bir alışkanlık/rutin takip platformudur. Proje hem web hem de mobil istemci sunar.

> **Uyarı:** Bu proje geliştirme aşamasındadır. Eksik özellikler ve hatalar bulunabilir. Aktif olarak güncellenmektedir; katkı ve geri bildirimlere açıktır.

---

## İçindekiler
- [Özellikler](#özellikler)
- [Mimari ve Katmanlar](#mimari-ve-katmanlar)
- [Teknolojiler](#teknolojiler)
- [Kurulum](#kurulum)
- [Kullanım](#kullanım)
- [API](#api)
- [Mobil Uygulama](#mobil-uygulama)
- [Katkı ve Lisans](#katkı-ve-lisans)

---

## Özellikler
- Kullanıcı kaydı, giriş ve JWT tabanlı kimlik doğrulama
- Günlük ve haftalık rutin oluşturma, düzenleme, silme
- Rutin tamamlama, streak ve başarı oranı takibi
- İstatistikler ve haftalık ilerleme grafikleri
- E-posta doğrulama, şifre sıfırlama
- Gerçek zamanlı bildirimler (SignalR)
- Web ve mobil istemci desteği (React Native/Expo)
- Çoklu dil desteği (TR/EN)

---

## Mimari ve Katmanlar
Proje, modern çok katmanlı mimariyle aşağıdaki katmanlara ayrılmıştır:

### 1. **Domain**
- Saf iş kuralları, entity ve value object'ler, domain event'leri
- Örnek: `User`, `Routine`, `RoutineLog`, `Email`, `RoutineTitle`

### 2. **Application**
- Use-case'ler (CQRS: Command/Query), DTO'lar, interface'ler, validasyonlar
- MediatR ile handler yönetimi
- Örnek: `CreateRoutineCommandHandler`, `RoutineDto`, `IUnitOfWork`

### 3. **Infrastructure**
- Veritabanı (EF Core/SQLite), dış servisler (e-posta, cache, token, notification)
- Repository ve UnitOfWork pattern implementasyonları
- Örnek: `AuthService`, `RoutineService`, `ApplicationDbContext`

### 4. **API**
- RESTful endpoint'ler, kimlik doğrulama, Swagger, arka plan servisleri
- Örnek endpointler: `/api/auth/register`, `/api/auth/login`, `/api/routines/today`, `/api/routines/week`

### 5. **Web**
- ASP.NET Core MVC tabanlı web arayüzü
- Rutin yönetimi, istatistikler, kullanıcı paneli, çoklu dil desteği

### 6. **Mobile**
- React Native (Expo) ile geliştirilen mobil istemci
- Rutin takibi, istatistikler, push bildirimler, modern UI

---

## Teknolojiler
- **Backend:** .NET 9, ASP.NET Core, EF Core (SQLite), MediatR, AutoMapper, FluentValidation, Serilog, StackExchange.Redis, SignalR
- **Frontend:** ASP.NET Core MVC, Bootstrap, Chart.js
- **Mobile:** React Native, Expo, React Navigation, Axios, React Native Paper
- **Diğer:** JWT, BCrypt

---

## Kurulum
### 1. Backend (API & Web)
```bash
# API için
cd SmartRoutine.API

dotnet restore
dotnet build
dotnet run
# API varsayılan olarak http://localhost:5000 adresinde çalışır

# Web için
cd SmartRoutine.Web

dotnet restore
dotnet build
dotnet run
# Web arayüzü varsayılan olarak http://localhost:5002 adresinde çalışır
```

### 2. Mobil Uygulama
```bash
cd SmartRoutine.Mobile
npm install
npm start
# veya
expo start
# Mobil uygulama, API'ye http://localhost:5000/api adresinden bağlanacak şekilde ayarlanmalıdır
```

> **Not:** Gerekirse mobil ve web istemcilerde API adresini `.env` veya konfigürasyon dosyasında güncelleyebilirsiniz.

---

## Kullanım
- **Web arayüzü:** [http://localhost:5002](http://localhost:5002)
- **API Swagger:** [http://localhost:5000/swagger](http://localhost:5000/swagger)
- **API endpointleri:** `http://localhost:5000/api/...`
- **Mobil:** API adresi olarak `http://localhost:5000/api` kullanılır (gerekirse .env veya kodda güncelleyin)

---

## API
### Temel Endpointler
- `POST   /api/auth/register`   → Kullanıcı kaydı
- `POST   /api/auth/login`      → Giriş (JWT)
- `GET    /api/auth/me`         → Profil bilgisi
- `PUT    /api/auth/profile`    → Profil güncelleme
- `POST   /api/auth/refresh`    → Token yenileme
- `POST   /api/auth/forgot-password` → Şifre sıfırlama
- `POST   /api/auth/verify-email`    → E-posta doğrulama
- `GET    /api/routines/today`  → Bugünkü rutinler
- `GET    /api/routines/week`   → Haftalık rutinler
- `POST   /api/routines`        → Rutin oluştur
- `PUT    /api/routines/{id}`   → Rutin güncelle
- `DELETE /api/routines/{id}`   → Rutin sil
- `POST   /api/routines/{id}/complete` → Rutin tamamlama

> Tüm endpointler JWT ile korunur. Swagger arayüzü ile test edilebilir.

---

## Mobil Uygulama
- Modern ve kullanıcı dostu arayüz
- Rutin ekleme, tamamlama, silme
- İstatistik ve streak takibi
- Push bildirim desteği (Expo/FCM)
- Çoklu dil desteği

---

## Katkı ve Lisans
- Katkıda bulunmak için fork'layıp PR gönderebilirsiniz.
- MIT Lisansı ile lisanslanmıştır.

---

## İletişim
Her türlü soru ve öneriniz için GitHub Issues üzerinden iletişime geçebilirsiniz. 