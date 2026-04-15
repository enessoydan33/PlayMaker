# PlayMaker

PlayMaker, futbol verilerini tek bir platformda toplayan ASP.NET Core MVC tabanli bir web uygulamasidir.  
Amac, kullanicinin canli skor, lig puani, gol kralligi, transfer market oyuncu verileri, haberler ve yorum gibi modullere tek noktadan erismesini saglamaktir.

## Uygulamanin Amaci

- Futbol odakli verileri daginik kaynaklardan toplayip tek bir arayuzde sunmak
- Kullaniciya guncel lig/gol/oyuncu/haber akisi saglamak
- Topluluk etkilesimini yorum, like/dislike ve anket ozellikleri ile arttirmak
- Wonderkids (AI Scout) modulu ile veri destekli oyuncu degerlendirmesi sunmak

## Temel Ozellikler

- **Lig ekrani:** Lig puan durumu ve gol kralligi verileri
- **Live Score:** Canli mac listesi ve skor takibi
- **Market:** Oyuncu arama, profil ve market value goruntuleme
- **Transfer/Haber:** Futbol haber akislarinin listelenmesi
- **Wonderkids / AI Scout:** Genc oyuncu filtreleme ve skorlama
- **Kullanici sistemi:** Kayit, giris, profil duzenleme
- **Yorum sistemi:** Lig / takim / oyuncu bazli yorumlar
- **Etkilesim:** Like-dislike ve anket yapisi

## Teknoloji Yigini

- **Backend:** ASP.NET Core MVC (.NET 8)
- **ORM:** Entity Framework Core
- **Kimlik dogrulama:** ASP.NET Identity
- **UI:** Razor Views + Bootstrap
- **Harici API kaynaklari:**
  - RapidAPI (Market, LiveScore, SofaScore, Football News)
  - CollectAPI (League ve GoalKings)

## Proje Yapisi

```text
PlayMaker/
  Api/                 -> Harici API servis siniflari
  Controllers/         -> MVC controller'lar
  Data/                -> DbContext ve repository katmani
  Entity/              -> Veritabani entity modelleri
  Models/              -> Uygulama ve API model siniflari
  ViewComponents/      -> Tekrar kullanilan server-side UI bloklari
  Views/               -> Razor gorunum dosyalari
  wwwroot/             -> Statik dosyalar (css, js, image)
  Program.cs           -> DI, middleware, routing ayarlari
```

## Kurulum

### 1) Gereksinimler

- .NET SDK 8
- Bir PostgreSQL veritabani (veya projedeki baglanti yapisina uyumlu ortam)

### 2) Projeyi acma

```bash
git clone <repo-url>
cd PlayMaker
```

### 3) Konfigurasyon

`PlayMaker/appsettings.Example.json` dosyasini referans alip kendi local ayarlarini olusturun.

> Guvenlik icin gercek key/connection string degerlerini repoya commit etmeyin.

Ornek alanlar:

- `TransfermarktApi:Host`
- `TransfermarktApi:Key`
- `LiveScoreApi:Host`
- `LiveScoreApi:Key`
- `SofaScoreApi:Host`
- `SofaScoreApi:Key`
- `FootballNewsApi:Host`
- `FootballNewsApi:Key`
- `CollectApi:Key` (`apikey ...` formatinda)
- `ConnectionStrings:DefaultConnection`

### 4) Veritabani migration

```bash
dotnet ef database update
```

### 5) Uygulamayi calistirma

```bash
dotnet run --project PlayMaker/PlayMaker.csproj
```

## API ve Secret Yonetimi

- `appsettings.json` ve `appsettings.Development.json` dosyalari local kullanim icindir.
- Production ortami icin environment variable veya secret manager kullanin.
- API limitlerinden dolayi (ozellikle RapidAPI) Market ve diger modullerde rate limit (429) alinabilir.

## Bilinen Operasyonel Notlar

- Market modulu dis API rate limitine duyarli oldugu icin cache/throttle davranisi kullanir.
- CollectAPI endpointleri `league` query parametresi ile kullanilir.
- API tarafinda donen JSON yapisi degistiginde model map alanlari guncellenmelidir.

## Hata Ayiklama Ipuclari

- `dotnet build` ile derleme kontrolu yapin
- Console loglarinda HTTP status/body ciktilarini takip edin
- 401/403: key veya abonelik problemi
- 404: endpoint path degisikligi
- 429: rate limit/quota

## Gelistirme Kurallari

- Secret degerleri koda hard-code etmeyin
- Yeni API entegrasyonlarinda:
  - timeout/retry/caching stratejisi ekleyin
  - response null ve parse hatalarini ele alin
  - UI tarafinda kullaniciya anlamli fallback gosterin

## Katki

1. Feature branch acin
2. Degisiklikleri test edin (`dotnet build`, temel akislari manuel dogrulama)
3. Pull request acin

## Lisans

Bu proje egitim/gelistirme amacli kullanima yoneliktir. Lisans bilgisi gerekiyorsa depoya `LICENSE` dosyasi ekleyin.
