## 15. Containerization & Deployment (Docker)

Sistem, tüm bileşenlerin izole ve kolay deploy edilebilir olması amacıyla Docker container’ları üzerinde çalışacaktır.

### Kullanılan Teknolojiler

* Docker
* Docker Compose

---

### Container Mimarisi

Sistem aşağıdaki container’lardan oluşacaktır:

#### 1. Backend Container

* Node.js tabanlı servis
* Prompt işleme ve scan orchestration görevini üstlenir
* Snyk CLI komutlarını tetikler

#### 2. Snyk CLI Container

* İzole güvenlik taraması için kullanılır
* Backend tarafından tetiklenir
* Scan işlemleri burada çalıştırılır

#### 3. Database Container

* PostgreSQL kullanılır
* Scan sonuçlarını persist eder

#### 4. BI / Dashboard Container

* Metabase
* Veritabanına bağlanarak dashboard sağlar

---

### Docker Compose Yapısı

Tüm servisler tek bir `docker-compose.yml` dosyası ile yönetilecektir:

* Servisler aynı network üzerinde çalışır
* Backend → DB → Metabase veri akışı sağlanır
* Snyk container shared volume üzerinden kodu okur

---

### Volume Yapısı

* `/scans` → scan edilen kodlar ve çıktılar
* `/db_data` → PostgreSQL persistent data

---

### Environment Variables

* `SNYK_TOKEN` → Snyk authentication için zorunlu
* `DB_HOST`, `DB_USER`, `DB_PASS` → database bağlantısı
* `PORT` → backend servis portu

---

### Network Yapısı

Docker Compose default network kullanılacaktır:

* Backend → Snyk container’a erişebilir
* Backend → PostgreSQL’e bağlanır
* Metabase → PostgreSQL’e bağlanır

---

### Deployment Senaryosu

1. Kullanıcı sistemi klonlar
2. `.env` dosyasına gerekli token ve config girilir
3. Aşağıdaki komut çalıştırılır:

```bash
docker-compose up -d
```

4. Servisler ayağa kalkar:

   * Backend → localhost:3000
   * Metabase → localhost:3001

---

### Avantajlar

* Ortam bağımsız çalışma
* Kolay kurulum (tek komut)
* İzole güvenlik taraması
* Ölçeklenebilir mimari

---

### Riskler

* Container içi Snyk çalıştırma performansı
* Volume permission problemleri
* Token yönetimi

---
