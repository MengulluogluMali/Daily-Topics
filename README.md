# Geliştirenler 

Harun ULAŞ - 215541033
Mehmet Ali MENGÜLLÜOĞLU - 225541053
Yusuf MARAL - 215541005 
Zeynep Nisanur TATAR - 225541037

---

# 🐟 AR Balık Müzesi — Mobil Artırılmış Gerçeklik Uygulaması

> **Elazığ Su Ürünleri Araştırma Enstitüsü (ELSAM)** iş birliğiyle geliştirilmiş eğitici AR mobil uygulaması.

---

## 📖 Proje Hakkında

**AR Balık Müzesi**, ELSAM bünyesindeki fiziksel balık müzesini dijital dünyaya taşıyan bir Artırılmış Gerçeklik (AR) mobil uygulamasıdır. Kullanıcılar müzedeki baskılı balık kartlarını akıllı telefon kameralarına göstererek o balığın 3D modelini gerçek dünya üzerinde görüntüleyebilir; bilgi paneli, quiz ve çizim gibi interaktif içeriklere erişebilir.

Uygulama; statik müze gezisini dinamik, eğlenceli ve öğretici bir keşif deneyimine dönüştürür. Özellikle **çocuklar ve gençler** için tasarlanmış sade ve renkli arayüzüyle su altı dünyasını keşfetmeyi kolaylaştırır.

---

## ✨ Temel Özellikler

| Özellik | Açıklama |
|---|---|
| 🔍 **AR Görüntü Tanıma** | Vuforia Image Target ile baskılı kartlar gerçek zamanlı tanınır |
| 🐠 **3D Balık Modelleri** | Her balık için ayrı .glb formatlı 3D model; parmakla döndürme ve atalet desteği |
| 📋 **Bilgi Paneli** | Bilimsel ad, yaşam alanı, beslenme, yenilebilirlik durumu ve tarif bilgileri |
| ❓ **Çoktan Seçmeli Quiz** | Her balık için JSON tabanlı 4 şıklı sorular; anlık geri bildirim ve puanlama |
| 🎨 **Akvaryum Çizim** | Her balık için ayrı dijital çizim tuvali; 8 renk + silgi + boyut ayarı |
| 🖼️ **Akvaryum Görüntüleyici** | Kaydedilen tüm çizimleri galeriden gözlemleme ve navigasyon |
| 🏆 **Başarım Sistemi** | 12 farklı başarım; tarama, quiz, çizim ve AR süresi takibi |
| 💾 **Kalıcı İlerleme** | PlayerPrefs ile başarımlar ve çizimler cihazda saklanır |

---

## 🐡 Desteklenen Balık Türleri (Mevcut Sürüm)

Mevcut sürümde aşağıdaki **12 balık türü** için AR içerikleri hazırlanmıştır:

| # | Balık Türü |
|---|---|
| 1 | Zargana Balığı |
| 2 | Mersin Balığı |
| 3 | Köpek Balığı (Mahmuzlu Camgöz) |
| 4 | Pisi Balığı |
| 5 | Fener Balığı |
| 6 | Vatoz |
| 7 | Mavi Yengeç |
| 8 | Benekli Dil Balığı |
| 9 | Kırlangıç Balığı |
| 10 | Uzun Burunlu Fare Balığı |


---

## 🛠️ Kullanılan Teknolojiler

| Teknoloji | Versiyon | Amaç |
|---|---|---|
| **Unity** | URP 17.4.0 | Oyun motoru ve uygulama geliştirme |
| **Vuforia Engine** | 11.4.4 | Artırılmış Gerçeklik (AR) altyapısı |
| **C#** | .NET | Uygulama mantığı ve script geliştirme |
| **Unity Input System** | 1.19.0 | Dokunmatik ekran ve fare girişi |
| **TextMesh Pro** | — | UI metin rendering |
| **glTF / GLB** | — | 3D balık modelleri |

---

## 📁 Proje Yapısı

```

├── Ar_fish_project/              # Unity projesi
│   ├── Assets/
│   │   ├── Proje_AR_Folder/
│   │   │   ├── Scripts/          # Tüm C# scriptleri
│   │   │   │   ├── FishDatabase.cs
│   │   │   │   ├── AchievementManager.cs
│   │   │   │   ├── AquariumDrawingManager.cs
│   │   │   │   ├── AquariumViewerManager.cs
│   │   │   │   ├── QuizManager.cs
│   │   │   │   ├── FishRotator.cs
│   │   │   │   ├── InfoButton3D.cs
│   │   │   │   ├── QuizButton3D.cs
│   │   │   │   └── AquariumButton3D.cs
│   │   │   ├── Project_JSON/     # Veri dosyaları (FishInfoDB, FishQuizDB, achievements)
│   │   │   ├── Project_Assest/   # 3D modeller (.glb)
│   │   │   └── Project_İmage/    # Referans görseller
│   │   └── Scenes/               # Unity sahneleri
│   └── Packages/
│       └── manifest.json         # Paket bağımlılıkları

---

## 🚀 Kurulum ve Çalıştırma

### Ön Koşullar

- **Unity Hub** (Unity 6 / 2023.x LTS önerilir, URP destekli)
- **Android Build Support** modülü yüklü olmalıdır
- **Vuforia Developer Lisansı** (developer.vuforia.com)
- **Android SDK & NDK** (Unity Hub üzerinden kurulabilir)

### Adımlar

1. Bu repo'yu klonlayın:
   ```bash
   git clone [<repo-url>](https://github.com/MengulluogluMali/Daily-Topics/)
   ```

2. Unity Hub'da `Ar_fish_project` klasörünü proje olarak açın.

3. `Packages/manifest.json` içindeki Vuforia paketini indirin:
   ```
   com.ptc.vuforia.engine-11.4.4.tgz
   ```

4. **Project Settings → Vuforia Configuration** bölümüne geçerli lisans anahtarınızı girin.

5. JSON veri dosyalarını `Assets/Proje_AR_Folder/Project_JSON/` klasörüne yerleştirin:
   - `FishInfoDB.json`
   - `FishQuizDB.json`
   - `achievements.json`

6. **Build Settings → Android** platformunu seçin ve **Switch Platform** yapın.
   Minimum Android Sürümü : Android 10.0 ve üstü

8. `SampleScene` sahnesini açarak **Play** modunda editörde test edebilir ya da Android cihaza build alabilirsiniz.

---

## 📊 Başarım Sistemi

Uygulama içinde toplam **12 başarım** bulunmaktadır:

| Başarım | Koşul | Nadirlik |
|---|---|---|
| İlk Temas | İlk balığı tara | Common |
| Denizle Tanışma | 3 farklı balık keşfet | Common |
| İlk Eskiz | İlk çizimini oluştur | Common |
| Kaşif | 5 farklı tür keşfet | Rare |
| Kusursuz | Bir balığın tüm quiz sorularını doğru cevapla | Rare |
| Deniz Ressamı | 5 çizim oluştur | Rare |
| Derin Sular | 10 dakika AR modunda kal | Rare |
| Dokunma Buna | Balon balığını keşfet | Epic |
| Yaşayan Fosil | Mersin balığını keşfet | Epic |
| Akvaryum Sanatçısı | 10 çizim kaydet | Epic |
| Deniz Ansiklopedisi | Tüm balıkları keşfet | Legendary |
| Deniz Efsanesi | Tüm başarımları aç | Legendary |

---

## 📋 Gereksinimler Belgesi

Projenin teknik ve fonksiyonel gereksinimleri için bkz. → **[Requirements.pdf](./Requirements.pdf)**


## 📄 Lisans

Bu proje ELSAM iş birliğiyle eğitim amaçlı geliştirilmiştir.
