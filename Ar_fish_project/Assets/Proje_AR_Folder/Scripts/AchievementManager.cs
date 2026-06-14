using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARFishQuiz
{
    /// <summary>
    /// Tüm başarım ilerlemesini yöneten merkez sınıf. PlayerPrefs üstünden kalıcı olarak saklar.
    /// Sahnedeki herhangi bir yerden sinyal gönderebilirsiniz:
    ///   AchievementManager.Instance.NotifyFishScanned("zargana");
    ///   AchievementManager.Instance.NotifyQuizCompleted("zargana", correct, total);
    ///   AchievementManager.Instance.NotifyDrawingCreated("zargana");
    ///   AchievementManager.Instance.NotifyDrawingSaved("zargana");
    /// AR süresi otomatik olarak Update'te artırılır.
    /// </summary>
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        // Olay: bir başarım yeni açıldığında tetiklenir (UI bunu dinler)
        public event Action<AchievementData> OnAchievementUnlocked;
        // Olay: ilerleme değiştiğinde (sayaçlar) tetiklenir (panel açıkken canlı güncelleme)
        public event Action OnProgressChanged;

        // ----- PlayerPrefs anahtarları -----
        private const string KeyUnlocked       = "ach_unlocked_v1";       // CSV achievementId
        private const string KeyScannedFish    = "ach_scanned_fish_v1";   // CSV unique fishId
        private const string KeyTotalScans     = "ach_total_scans_v1";    // int
        private const string KeyPerfectQuiz    = "ach_perfect_quiz_v1";   // int
        private const string KeyDrawingCount   = "ach_drawing_count_v1";  // int
        private const string KeySavedDrawCount = "ach_saved_draw_v1";     // int
        private const string KeyArSeconds      = "ach_ar_seconds_v1";     // float

        // ----- Hafıza durumu -----
        private HashSet<string> _unlocked      = new HashSet<string>();
        private HashSet<string> _scannedFish   = new HashSet<string>();
        private int   _totalScans              = 0;
        private int   _perfectQuizCount        = 0;
        private int   _drawingCount            = 0;
        private int   _savedDrawingCount       = 0;
        private float _arSeconds               = 0f;
        private float _arSaveTimer             = 0f;

        public IReadOnlyCollection<string> UnlockedIds => _unlocked;
        public IReadOnlyCollection<string> ScannedFishIds => _scannedFish;
        public int TotalScans => _totalScans;
        public int PerfectQuizCount => _perfectQuizCount;
        public int DrawingCount => _drawingCount;
        public int SavedDrawingCount => _savedDrawingCount;
        public float ArSeconds => _arSeconds;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            AchievementDatabase.EnsureLoaded();
            LoadFromPrefs();
        }

        private void Update()
        {
            // AR süresi sayacı (uygulama açıkken artar)
            _arSeconds += Time.unscaledDeltaTime;
            _arSaveTimer += Time.unscaledDeltaTime;
            if (_arSaveTimer >= 5f)
            {
                _arSaveTimer = 0f;
                PlayerPrefs.SetFloat(KeyArSeconds, _arSeconds);
                PlayerPrefs.Save();
                CheckAll();
            }
        }

        // =================== PUBLIC API ===================

        /// <summary> Bir balık tarandığında çağırın. </summary>
        public void NotifyFishScanned(string fishId)
        {
            if (string.IsNullOrEmpty(fishId)) return;

            _totalScans++;
            bool isNewSpecies = _scannedFish.Add(NormalizeFishId(fishId));
            SaveProgress();
            CheckAll();
            OnProgressChanged?.Invoke();
        }

        /// <summary> Quiz bittiğinde çağırın. correct == total ise mükemmel sayılır. </summary>
        public void NotifyQuizCompleted(string fishId, int correct, int total)
        {
            if (total > 0 && correct == total)
            {
                _perfectQuizCount++;
                SaveProgress();
                CheckAll();
                OnProgressChanged?.Invoke();
            }
        }

        /// <summary> Çizim paneli açılıp ilk çizgi atıldığında çağırın. </summary>
        public void NotifyDrawingCreated(string fishId)
        {
            _drawingCount++;
            SaveProgress();
            CheckAll();
            OnProgressChanged?.Invoke();
        }

        /// <summary> Çizim "Kaydet ve Çık" ile kaydedildiğinde çağırın. </summary>
        public void NotifyDrawingSaved(string fishId)
        {
            _savedDrawingCount++;
            SaveProgress();
            CheckAll();
            OnProgressChanged?.Invoke();
        }

        public bool IsUnlocked(string achievementId)
        {
            return _unlocked.Contains(achievementId);
        }

        /// <summary> Bir başarımın 0..1 ilerleme yüzdesini döner. </summary>
        public float GetProgress(AchievementData a, out string label)
        {
            label = "";
            if (a == null || a.requirement == null) return 0f;
            var r = a.requirement;
            switch (r.type)
            {
                case "scan_count":
                {
                    int cur = Mathf.Min(_totalScans, r.value);
                    label = $"{cur} / {r.value}";
                    return r.value > 0 ? (float)cur / r.value : 1f;
                }
                case "unique_fish_scan":
                {
                    int cur = Mathf.Min(_scannedFish.Count, r.value);
                    label = $"{cur} / {r.value}";
                    return r.value > 0 ? (float)cur / r.value : 1f;
                }
                case "all_fish_scan":
                {
                    var allIds = FishDatabase.GetAllFishIds();
                    int total = allIds != null ? allIds.Count : 0;
                    int cur = 0;
                    if (allIds != null)
                    {
                        foreach (var id in allIds)
                            if (_scannedFish.Contains(NormalizeFishId(id))) cur++;
                    }
                    label = total > 0 ? $"{cur} / {total}" : "?";
                    return total > 0 ? (float)cur / total : 0f;
                }
                case "scan_specific_fish":
                {
                    bool ok = !string.IsNullOrEmpty(r.fishId) && _scannedFish.Contains(NormalizeFishId(r.fishId));
                    label = ok ? "Tamamlandı" : "Henüz keşfetmedin";
                    return ok ? 1f : 0f;
                }
                case "perfect_quiz":
                {
                    int cur = Mathf.Min(_perfectQuizCount, r.value);
                    label = $"{cur} / {r.value}";
                    return r.value > 0 ? (float)cur / r.value : 1f;
                }
                case "drawing_count":
                {
                    int cur = Mathf.Min(_drawingCount, r.value);
                    label = $"{cur} / {r.value}";
                    return r.value > 0 ? (float)cur / r.value : 1f;
                }
                case "saved_drawing_count":
                {
                    int cur = Mathf.Min(_savedDrawingCount, r.value);
                    label = $"{cur} / {r.value}";
                    return r.value > 0 ? (float)cur / r.value : 1f;
                }
                case "ar_time_minutes":
                {
                    int curSec = Mathf.Min(Mathf.FloorToInt(_arSeconds), r.value * 60);
                    int totalSec = r.value * 60;
                    label = $"{curSec / 60}:{(curSec % 60):00} / {r.value}:00";
                    return totalSec > 0 ? (float)curSec / totalSec : 1f;
                }
                case "unlock_all_achievements":
                {
                    int total = 0, cur = 0;
                    foreach (var ach in AchievementDatabase.All)
                    {
                        if (ach == null) continue;
                        if (ach.achievementId == a.achievementId) continue; // kendisini sayma
                        total++;
                        if (_unlocked.Contains(ach.achievementId)) cur++;
                    }
                    label = $"{cur} / {total}";
                    return total > 0 ? (float)cur / total : 0f;
                }
            }
            return 0f;
        }

        /// <summary> Tüm gerekleri yeniden değerlendirir. Yeni açılan başarımlar için event tetikler. </summary>
        public void CheckAll()
        {
            // 1) Master ("unlock_all_achievements") hariç hepsini kontrol et
            AchievementData masterAch = null;
            foreach (var a in AchievementDatabase.All)
            {
                if (a == null) continue;
                if (a.requirement != null && a.requirement.type == "unlock_all_achievements")
                {
                    masterAch = a;
                    continue;
                }
                TryUnlock(a);
            }
            // 2) Sonra master'ı kontrol et (diğerleri açıldığı için artık doğru olabilir)
            if (masterAch != null) TryUnlock(masterAch);
        }

        private void TryUnlock(AchievementData a)
        {
            if (a == null || _unlocked.Contains(a.achievementId)) return;
            float p = GetProgress(a, out _);
            if (p >= 1f - 0.0001f)
            {
                _unlocked.Add(a.achievementId);
                SaveProgress();
                Debug.Log($"[Achievement] Unlocked: {a.achievementId} ({a.title})");
                try { OnAchievementUnlocked?.Invoke(a); } catch (Exception e) { Debug.LogError(e); }
            }
        }

        // =================== Helpers ===================

        public static string NormalizeFishId(string fishId)
        {
            if (string.IsNullOrEmpty(fishId)) return fishId;
            string id = fishId.ToLowerInvariant();
            if (id.EndsWith("_target")) id = id.Substring(0, id.Length - "_target".Length);
            // "balıgı" -> "baligi" (Türkçe karakter eşleştirmesi için basit normalize)
            id = id.Replace("ı", "i").Replace("İ", "i").Replace("ğ", "g").Replace("ü", "u")
                   .Replace("ş", "s").Replace("ö", "o").Replace("ç", "c");
            id = id.Replace("baligi", "").Replace("balig", "");
            id = id.Trim('_');
            return id;
        }

        // =================== Persist ===================

        private void LoadFromPrefs()
        {
            _unlocked.Clear();
            string s = PlayerPrefs.GetString(KeyUnlocked, "");
            if (!string.IsNullOrEmpty(s))
            {
                foreach (var t in s.Split(',')) if (!string.IsNullOrEmpty(t)) _unlocked.Add(t);
            }

            _scannedFish.Clear();
            s = PlayerPrefs.GetString(KeyScannedFish, "");
            if (!string.IsNullOrEmpty(s))
            {
                foreach (var t in s.Split(',')) if (!string.IsNullOrEmpty(t)) _scannedFish.Add(t);
            }

            _totalScans         = PlayerPrefs.GetInt(KeyTotalScans, 0);
            _perfectQuizCount   = PlayerPrefs.GetInt(KeyPerfectQuiz, 0);
            _drawingCount       = PlayerPrefs.GetInt(KeyDrawingCount, 0);
            _savedDrawingCount  = PlayerPrefs.GetInt(KeySavedDrawCount, 0);
            _arSeconds          = PlayerPrefs.GetFloat(KeyArSeconds, 0f);
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetString(KeyUnlocked, string.Join(",", _unlocked));
            PlayerPrefs.SetString(KeyScannedFish, string.Join(",", _scannedFish));
            PlayerPrefs.SetInt(KeyTotalScans, _totalScans);
            PlayerPrefs.SetInt(KeyPerfectQuiz, _perfectQuizCount);
            PlayerPrefs.SetInt(KeyDrawingCount, _drawingCount);
            PlayerPrefs.SetInt(KeySavedDrawCount, _savedDrawingCount);
            PlayerPrefs.SetFloat(KeyArSeconds, _arSeconds);
            PlayerPrefs.Save();
        }

        /// <summary> Tüm ilerlemeyi sıfırla (debug). </summary>
        public void ResetAll()
        {
            _unlocked.Clear();
            _scannedFish.Clear();
            _totalScans = 0;
            _perfectQuizCount = 0;
            _drawingCount = 0;
            _savedDrawingCount = 0;
            _arSeconds = 0f;
            SaveProgress();
            OnProgressChanged?.Invoke();
        }
    }
}
