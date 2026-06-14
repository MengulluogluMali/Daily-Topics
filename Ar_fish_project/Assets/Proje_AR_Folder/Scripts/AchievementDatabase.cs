using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARFishQuiz
{
    [Serializable]
    public class AchievementRequirement
    {
        public string type;       // scan_count, unique_fish_scan, all_fish_scan, scan_specific_fish, perfect_quiz, drawing_count, saved_drawing_count, ar_time_minutes, unlock_all_achievements
        public int value;         // sayısal eşik (varsa)
        public string fishId;     // belirli balık gerektiren başarımlar için
    }

    [Serializable]
    public class AchievementData
    {
        public string achievementId;
        public string title;
        public string icon;
        public string description;
        public string category;
        public string rarity;     // common, rare, epic, legendary
        public AchievementRequirement requirement;
    }

    [Serializable]
    public class AchievementCollection
    {
        public List<AchievementData> achievements = new List<AchievementData>();
    }

    /// <summary>
    /// achievements.json dosyasını yükleyen statik veritabanı.
    /// Önce Resources/achievements, ardından Project_JSON klasörü, son olarak StreamingAssets denenir.
    /// </summary>
    public static class AchievementDatabase
    {
        private const string ResourcePath = "achievements";
        private const string FileRelative = "Proje_AR_Folder/Project_JSON/achievements.json";

        private static AchievementCollection _collection;
        private static bool _loaded;

        public static IReadOnlyList<AchievementData> All
        {
            get
            {
                EnsureLoaded();
                return _collection.achievements;
            }
        }

        public static void EnsureLoaded()
        {
            if (_loaded) return;
            Load();
            _loaded = true;
        }

        public static void Reload()
        {
            _loaded = false;
            _collection = null;
            EnsureLoaded();
        }

        private static void Load()
        {
            string json = LoadJson();
            if (string.IsNullOrEmpty(json))
            {
                _collection = new AchievementCollection();
                Debug.LogWarning("[AchievementDatabase] achievements.json yüklenemedi.");
                return;
            }

            try
            {
                _collection = JsonUtility.FromJson<AchievementCollection>(json);
                if (_collection == null) _collection = new AchievementCollection();
            }
            catch (Exception e)
            {
                Debug.LogError($"[AchievementDatabase] parse hatası: {e.Message}");
                _collection = new AchievementCollection();
            }
        }

        private static string LoadJson()
        {
#if UNITY_EDITOR
            // Editor'da önce canlı dosyayı oku
            string editorPath = System.IO.Path.Combine(Application.dataPath, FileRelative);
            if (System.IO.File.Exists(editorPath))
            {
                try
                {
                    string txt = System.IO.File.ReadAllText(editorPath);
                    if (!string.IsNullOrEmpty(txt)) return txt;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[AchievementDatabase] {editorPath} okunamadı: {e.Message}");
                }
            }
#endif

            var ta = Resources.Load<TextAsset>(ResourcePath);
            if (ta != null && !string.IsNullOrEmpty(ta.text)) return ta.text;

            string fullPath = System.IO.Path.Combine(Application.dataPath, FileRelative);
            if (System.IO.File.Exists(fullPath))
            {
                try { return System.IO.File.ReadAllText(fullPath); }
                catch (Exception e) { Debug.LogError($"[AchievementDatabase] {fullPath} okunamadı: {e.Message}"); }
            }

            string sa = System.IO.Path.Combine(Application.streamingAssetsPath, "achievements.json");
            if (System.IO.File.Exists(sa))
            {
                try { return System.IO.File.ReadAllText(sa); }
                catch { /* yoksay */ }
            }

            return null;
        }

        public static AchievementData Get(string achievementId)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(achievementId)) return null;
            return _collection.achievements.Find(a => a.achievementId == achievementId);
        }
    }
}
