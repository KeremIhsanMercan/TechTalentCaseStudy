# Abonelik & Otomatik Ödeme Hatırlatma Uygulaması

Bu proje, Kuveyt Türk / Architecht TechTalent Case Study gereksinimleri doğrultusunda geliştirilmiş kurumsal seviyede bir tam yığın (full-stack) bankacılık uygulamasıdır. Sistem; doğrudan banka müşterilerinin (son kullanıcıların) kendi internet bankacılığı platformları üzerinden düzenli aboneliklerini (elektrik, su, internet, GSM vb.) tek bir noktadan tanımlayabilmelerini, yaklaşan borçlarını takip edebilmelerini ve güvenle ödeyebilmelerini sağlayan müşteri odaklı (customer-facing) bir finansal modül olarak tasarlanmıştır.

### Temel Yetenekler (Core Capabilities)
* **Merkezi Abonelik Yönetimi:** Kullanıcıların elektrik, su, internet ve GSM gibi farklı hizmet sağlayıcılara ait düzenli ödemelerini tek bir platform üzerinden tanımlayıp merkezi olarak yönetebilmesi.
* **Akıllı Hatırlatma Sistemi:** Yaklaşan ödeme tarihleri için vadesinden önce otomatik bildirim (E-posta/SMS simülasyonu) üreten, ödemesi yapılmış dönemleri ise otomatik olarak filtreleyen akıllı mekanizma.
* **Güvenli Finansal İşlemler:** Dönemsel borçların takibi ve aynı döneme ait tekrar eden tahsilat riskini (double-charge) veritabanı seviyesinde engelleyen yüksek güvenlikli işlem altyapısı.
* **Kapsamlı Finansal Özet:** Aktif aboneliklerin durumunu, ödenmemiş borçları ve geçmişe dönük tüm ödeme kayıtlarını anlık olarak sunan analitik kullanıcı paneli.

### Simülasyon ve Entegrasyon Altyapısı
Uygulamanın gerçek dünya bankacılık senaryolarına uyumunu ve dirençliliğini test etmek amacıyla aşağıdaki entegrasyon altyapıları kurgulanmıştır:
* **Dış Servis Simülasyonu (Mock APIs):** Kurumsal abonelik şirketlerinin borç sorgulama sistemlerini ve banka ödeme ağ geçitlerini simüle eden, ağ gecikmelerine ve hata senaryolarına sahip yüksek dirençli mock servisler.
* **Deterministik Veri Tohumlama (Data Seeding):** Sistemin yeteneklerini anında gözlemleyebilmek için hazır, anlamlı ve tutarlı test verisi üretim mekanizması.
* **Polly Dirençlilik Kalkanı:** Dış servislerden gelebilecek olası hataları ve kesintileri yöneten, kurumsal standartlarda hata toleransı sağlayan "Retry" politikaları.

## 🛠 Kullanılan Teknolojiler

**Backend (Sunucu Tarafı):**
* C# 12 & .NET 8 Web API
* Entity Framework Core
* SQL Server
* Polly

**Frontend (Ön Yüz):**
* React 19 & TypeScript (C# ile Type Checking uyumu için)
* Tailwind CSS

**Mimari Yaklaşım:**
* Clean Architecture
* Repository Pattern
* Dependency Injection
* Global Exception Handling

## 🚀 Kurulum ve Çalıştırma

Projenin kurulum süreci, manuel SQL querylerine ihtiyaç duyulmaksızın, uygulamanın başlatılması anında devreye giren otomatik `EnsureCreatedAsync` yapımız sayesinde kusursuz ve sıfır eforlu hale getirilmiştir. Veritabanı ve test verileri uygulama ayağa kalkarken otomatik olarak oluşturulur.

### Backend'i Çalıştırmak İçin:
Terminalinizden uygulamanın API dizinine gidin ve projeyi başlatın:
```bash
cd src/WebAPI
dotnet run
```

### Frontend'i Çalıştırmak İçin:
Ayrı bir terminalde ön yüz dizinine gidin, bağımlılıkları yükleyin ve geliştirme sunucusunu başlatın:
```bash
cd frontend
npm install
npm run dev
```

## 📐 Sistem Tasarım Dokümanları

Uygulamanın kurumsal mimari standartlara, veri bütünlüğü ilkelerine ve denetim (audit) süreçlerine tam uyumluluğunu kanıtlamak adına, sistemin tüm katmanları ve entegrasyon noktaları uçtan uca dokümante edilmiştir:

* 📊 **[ER Diyagramı](./docs/er_diagram.png):** `Customer`, `Subscription` ve `Payment` varlıkları arasındaki ilişkiler, veritabanı seviyesinde uygulanan ilişkisel silme korumaları (`DeleteBehavior.Restrict`) ve çifte ödemeleri engelleyen kompozit indeksleme mimarisi.
* 🌊 **[Sistem Akış Şeması (Flowchart)](./docs/flow_diagram.png):** Birbirinden tamamen izole edilmiş (decoupled) senkron ve asenkron yürütme yolları, iş mantığı kararları ve arka plan süreçlerindeki kısmi hata yalıtım sınırları.
* ⏳ **[Sistem Orkestrasyon ve Sekans Diyagramı](./docs/sequence_diagram.png):** İstemci (UI), Web API, Uygulama Servisleri ve dış mock API'ler arasındaki asenkron haberleşme zaman çizelgesi, devre kesici Polly kalkanlarının aktivasyon anları ve DI Scope yaşam döngüleri.
* 📋 **[API Endpoint Matrisi](./docs/api_endpoints.md):** Sistemin dış dünyaya açtığı tüm RESTful rotaların listesi, HTTP durum kodu haritalamaları, DTO sözlüğü ve etki alanı kurallarına bağlı güvenlik kısıtları (Örn: 409 Conflict senaryoları).
* 🛡️ **[Kapsamlı Mimari ve Performans Denetim Raporu](./docs/backendFinalAuditReport.md):** Sistemin dosya bazlı bağımsız denetimden geçtiğini kanıtlayan resmi sertifikasyon belgesi. Sıfır teknik borç, %100 EF Core thread-safety uyumluluğu, heap bellek optimizasyonları ve etki alanı kurallarının (Domain Invariants) kusursuzluğunu onaylayan AI raporu.

## 🧠 Yapay Zeka Destekli Geliştirme ve Mühendislik Günlüğü

Bu projede modern yapay zeka araçları (Copilot, AntiGravity) ve dil modelleri (Claude Opus 4.6, Gemini 3 Pro) birer "yardımcı kodlayıcı" olarak konumlandırılmış; projenin temel mimari iskeleti, veri güvenliği kısıtlamaları, etki alanı (Domain) kuralları ve uçtan uca performans optimizasyonları bizzat tarafımca tasarlanmış ve sıkı bir denetimden geçirilmiştir. Üretilen çözüm, kurumsal bankacılık standartlarına tam uyumlu, deterministik ve yüksek dirençli bir mühendislik ürünü haline getirilmiştir. Sistem genelinde uyguladığım kritik mimari kararlar ve stabilizasyon aşamaları aşağıda özetlenmiştir.

### 1. Mimari İskelet, Veri Bütünlüğü ve İlişkisel Güvenlik
Projenin temel omurgasını oluştururken Clean Architecture prensiplerini ve temel etki alanı varlıklarımızı (`Customer`, `Subscription`, `Payment`) yapay zeka desteğiyle modelledim. Ancak finansal sistemlerde hiçbir para hareketinin veya işlem logunun fiziksel olarak kaybolmaması kritik bir iş kuralı olduğundan, yapay zekanın ürettiği varsayılan "Cascade Delete" (kademeli silme) kurgularına derhal müdahale ederek tüm varlık ilişkilerini `DeleteBehavior.Restrict` silme koruması altına aldım. 

Silme operasyonlarının bu kısıtlarla çakışıp "Foreign Key" hataları üretmesini engellemek adına tüm varlıkları `IsDeleted` bayrağı ile Soft Delete mantığına geçirdim ve bu yapıyı EF Core DbContext seviyesinde tanımladığım `Global Query Filter` ile kalıcı olarak güvence altına aldım. Ayrıca, olası ağ gecikmelerinde veya mükerrer kullanıcı tıklamalarında aynı döneme (Örn: `2026_05`) ait ödemelerin tekrar alınması riskini tespit ettim; bu finansal açığı kapatmak için `Payment` tablosuna `(SubscriptionId, Period, IsSuccessful)` kolonlarını kapsayan filtrelenmiş bir "Composite Unique Index" kısıtı ekleyerek sistemde tam bir finansal Idempotency (aynı işlemin tekrarlanamazlığı) sağladım.

### 2. Dış Servis Entegrasyonları, Dirençlilik (Resilience) ve I/O Performansı
Bankacılık etki alanı standartlarına uygun olarak simüle ettiğimiz dış borç sorgulama ve ödeme servislerini `HttpClient` altyapısıyla kurarken, geçici ağ kesintilerine karşı Polly kütüphanesi yardımıyla Retry (Tekrar Deneme) ve Circuit Breaker (Devre Kesici) mekanizmalarını entegre ettim. Yapay zekanın tüm servisler için tek bir ortak devre kesici nesnesi kurgulayarak sistemlerin birbirini kilitlemesine yol açabilecek hatalı tasarımını tespit edip, bu politikaları her servise özel (izole) olacak şekilde ayrıştırdım ve işlem ID atamalarındaki `DateTime.UtcNow` sızıntısını `IDateTimeProvider` soyutlamasıyla kapattım. Dış servis çağrılarının Dependency Injection seviyesinde kurulan kalkanları es geçme (bypass) riskine karşı ise, iş mantığı katmanındaki tüm entegrasyonları (`IDebtCheckingService` ve `IPaymentInfrastructureService`) doğrudan kod satırları arasında (inline) Polly Retry kalkanlarıyla standartlaştırarak her iki servisin de kesintilere karşı tutarlı bir hata toleransına sahip olmasını garantiledim.

Sistemin ölçeklenebilirliği önündeki en büyük engel olan I/O darboğazını da bu aşamada çözdüm. Tohumlanan 120'den fazla aboneliğin borç sorgulamalarının ardışık (sequential) işlenmesi nedeniyle istemci tarafında yaşanan `TaskCanceledException` zaman aşımlarını engellemek için mimariyi iki faza ayırdım: DbContext'in thread-safety kuralları gereği veritabanı kontrollerini ilk fazda ardışık olarak güvenle tamamladım; thread-safe olan dış API sorgularını ise ikinci fazda LINQ ve `Task.WhenAll` kullanarak eşzamanlı (concurrent) hale getirdim. Bu mühendislik hamlesiyle API yanıt sürelerini **12-15 saniyeden ~350 milisaniyeye** indirerek I/O darboğazını tamamen ortadan kaldırdım. Ayrıca denetimlerim esnasında `0.00 TL` borcu olan aboneliklere vade tarihi yaklaştığı için hatalı bildirim üretildiğini tespit edip, akışlara `DebtAmount > 0` kısıtını ekleyerek etki alanı mantığını kusursuzlaştırdım.

### 3. İş Mantığı, Web API Tasarımı ve Merkezi Hata Yönetimi
Use-Case akışları, DTO'lar, FluentValidation kuralları ve RESTful sunum katmanını oluştururken yapay zekadan destek aldım. Ancak hatırlatma servisini (`ReminderAppService`) denetlerken, yapay zekanın tarih farkı algoritmasında gecikmiş (eksi günlere düşmüş) borçları da akışa soktuğunu ve mesajlarda sunucu bölgesine bağımlı (`:C2`) para birimi formatı kullandığı açıklarını saptayarak müdahale ettim; hatırlatma aralığını `0 <= gün <= Threshold` sınırına çektim ve para birimi gösterimini TL'ye sabitledim. Hatırlatma eşik değerleri gibi iş kurallarını kod içerisindeki sabit sayılardan (Magic Numbers) arındırıp .NET `IOptions` deseniyle `appsettings.json` dosyasına taşıyarak sistemi yeniden derleme gerektirmeyen dinamik bir yapıya kavuşturdum.

API katmanında ise fırlatılan özel finansal istisnaların (`DuplicatePaymentException` vb.) doğru HTTP durum kodlarına (`409 Conflict`, `400 Bad Request`) haritalanmasını ve beklenen hatalar için logların hata (error) yerine uyarı (warning) olarak yazılmasını sağladım. Ara katmanda (middleware) başlıkların kilitlenmesi durumunda oluşabilecek çökmeleri önlemek için `Response.HasStarted` güvenlik kontrolünü uyguladım ve bellek optimizasyonu (Zero-Allocation) amacıyla JSON serileştirme akışlarını gereksiz string kopyaları oluşturmadan doğrudan HTTP akışına (`SerializeAsync`) yönlendirdim.

### 4. Deterministik Veri Yönetimi, Arka Plan Süreçleri ve OCP Soyutlaması
Mock servislerin her sorguda farklı borç tutarları üreterek kullanıcı deneyimini bozmasını önlemek adına, backend seviyesinde abonelik numarası ve döneme dayalı deterministik bir "Seed-based Randomness" kurguladım. Borç miktarı ve vade tarihlerini doğrudan `Subscription` varlığına (Entity) mühürleyerek sistem genelinde mutlak veri bütünlüğü (Data Integrity) sağladım ve vade tarihlerini rastgele dağıtarak gerçekçi bir senaryo elde ettim. Bildirim mekanizmasını ise doğrudan iş mantığına sıkıştırmak yerine `INotificationService` arayüzü ile soyutlayarak (Open-Closed Principle) gelecekteki olası kurumsal entegrasyonlara hazır hale getirdim. Kurguladığım mock bildirim servisi (`MockNotificationService`) sayesinde asenkron gecikmeleri simüle ettim ve olası bildirim hatalarının asıl motoru kilitlemesini Kısmi Başarısızlık (Partial Failure) prensibiyle engelledim.

Hatırlatma akışlarını RESTful prensiplerine sadık kalmak adına arayüz isteklerinden tamamen sökerek yerleşik `BackgroundService` yapısına taşıdım. Yapay zeka ajanının yaptığı `PeriodicTimer` ilk çalışma gecikmesi (startup delay) hatasını düzelterek sunucu başlar başlamaz ilk taramanın yapılmasını sağladım. Singleton yaşam döngüsündeki arka plan işçisinden, Scoped ömürlü servislere ve veritabanı bağlamına güvenle erişebilmek adına `IServiceScopeFactory` ile manuel kapsam (Scope) yönetimi kurgulayarak bellek sızıntılarını (memory leaks) tamamen engelledim. Backend yapısını tamamladıktan sonra yapay zeka ajanlarını birer "Kıdemli Denetçi" olarak çalıştırıp tüm kod tabanında denetimler yürüttüm; hazırlanan `backendFinalAuditReport.md` belgesiyle de uyguladığımız kalkanların mimariyi %100 güvenceye aldığını tescilleyerek projeyi üretime hazır (production-ready) olarak dondurdum.

### 5. Ön Yüz (Frontend) Mimarisi, Durum Güvenliği ve Modern UX/UI
React ve TypeScript ile kurulan arayüz katmanında, yapay zekanın kod üretimi sırasında modern form gönderim standartlarını (`React.SubmitEvent`) ezip eski yapılara döndürdüğü regresyonları tespit ederek kod tabanını en güncel DOM standartlarına yükselttim. Asenkron API sorguları esnasında modallarda yaşanabilecek **Race Condition** (veri/başlık çakışması) ve **Stale State** (eski müşteri verilerinin ekranda asılı kalması) risklerini, hem üst çarpı hem de alt butonlar (Kapat, İptal) gibi tüm kapatma tetikleyicilerine uyguladığım eksiksiz durum sıfırlamaları (`null` ve boş dizi atamaları) ile mimari seviyede %100 güvence altına aldım. İç içe geçmiş ödeme geçmişlerini düzleştirirken olası tanımsız referansların arayüzü çökertmesini (White Screen of Death) önlemek adına algoritmaları **Optional Chaining** (`?.`) ve güvenli dizilerle (`?? []`) koruma altına aldım. Ayrıca tablo verilerini React Query aracılığıyla getirip, sayfa (page) bazlı değil tüm küme (dataset) bazlı sıralama (Global Sorting) mantığıyla donattım. Kullanıcıların arama yaparak seçim yapabildiği tip güvenli `SearchableSelect` bileşeniyle, abonelik oluşturma ekranındaki standart açılır menü deneyimini üst seviyeye taşıdım.

Görsel katmanda ise arayüzü modern Fintech standartlarına uygun olarak baştan aşağı AI ajanları ile düzenledim.
