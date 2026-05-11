# Abonelik & Otomatik Ödeme Hatırlatma Uygulaması

Bu proje, Kuveyt Türk / Architecht TechTalent 2026 Case Study gereksinimleri doğrultusunda, bireysel banka müşterilerinin düzenli abonelik ödemelerini takip edebilmeleri ve yönetebilmeleri için geliştirilmiş kurumsal seviyede bir bankacılık uygulamasıdır.

## 🧠 Yapay Zeka (AI) Kullanım Ajandası

Bu projede yapay zeka araçları (AntiGravity, Copilot) birer "yardımcı kodlayıcı" olarak konumlandırılmış, tüm mimari kararlar, iş kuralları ve güvenlik denetimleri tarafımca kontrol edilmiştir.

* **Proje İskeleti ve Temel Modelleme:** Strict Clean Architecture Solution yapısı, temel Domain Entity'leri (`Customer`, `Subscription`, `Payment`) ve EF Core `DbContext` altyapısı AI desteği ile üretilmiştir.
* **İlişkisel Veri Güvenliği (Refactoring):** Finansal sistemlerde işlem loglarının (ödemeler) kaybolmaması kritik bir iş kuralıdır. AI tarafından üretilen varsayılan (Cascade Delete) silme davranışları tarafımca müdahale edilerek tüm ilişkilerde `DeleteBehavior.Restrict` olarak güncellenmiştir.
* **Idempotency & Tekrarlanan Ödeme Koruması (Refactoring):** İlk veri modellemesi gözden geçirildiğinde, ağ gecikmeleri veya kullanıcı hataları nedeniyle aynı döneme (Örn: `2026_05`) ait tekrarlanan ödeme alınma riski tespit edilmiştir. Bu finansal açığı kapatmak için `Payment` tablosuna tarafımca `(SubscriptionId, Period, IsSuccessful)` bazlı filtrelenmiş Composite Unique Index kısıtı ekletilmiştir.
* **Soft Delete Entegrasyonu (Refactoring):** Dokümanda istenen silme operasyonlarının, finansal kısıtlarla çakışıp `Foreign Key Constraint` hatası üretmesini engellemek adına tüm varlıklar `IsDeleted` bayrağı ile Soft Delete mantığına geçirilmiş ve DbContext seviyesinde `Global Query Filter` ile güvenceye alınmıştır.
* **Dış Servis Entegrasyonları ve Dirençlilik (Resilience):** Bankacılık domain standartlarına uygun olarak dış borç sorgulama ve ödeme servisleri (Mock) `HttpClient` altyapısıyla kurgulanmış, geçici ağ hatalarına karşı `Polly` ile Retry (Tekrar Deneme) ve Circuit Breaker (Devre Kesici) mekanizmaları eklenmiştir. AI tarafından tek bir ortak nesne üzerinden kurgulanan ve servislerin birbirini kilitlemesine yol açabilecek devre kesici politikaları, tarafımca her servise özel (izole) olacak şekilde ayrıştırılmış ve işlem ID üretimindeki `DateTime.UtcNow` sızıntısı `IDateTimeProvider` ile kapatılmıştır.