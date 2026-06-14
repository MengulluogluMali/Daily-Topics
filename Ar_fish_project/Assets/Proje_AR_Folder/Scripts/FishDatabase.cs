using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARFishQuiz
{
    // ====== FishInfoDB.json modelleri ======
    [Serializable]
    public class FishInfo
    {
        public string fishId;
        public string displayName;
        public string scientificName;
        public string shortDescription;
        public string habitat;
        public string diet;
        // JSON'da bulunabilen ek alanlar (opsiyonel; yoksa boş kalır)
        public string durum;
        public string tarif;
    }

    [Serializable]
    public class FishInfoCollection
    {
        public List<FishInfo> fishInfos = new List<FishInfo>();
    }

    // ====== FishQuizDB.json modelleri ======
    [Serializable]
    public class FishQuizQuestion
    {
        public int questionId;
        public string questionText;
        public string[] options;
        public int correctOptionIndex;
    }

    [Serializable]
    public class FishQuiz
    {
        public string fishId;
        public List<FishQuizQuestion> questions = new List<FishQuizQuestion>();
    }

    [Serializable]
    public class FishQuizCollection
    {
        public List<FishQuiz> fishQuizzes = new List<FishQuiz>();
    }

    /// <summary>
    /// JSON dosyalarından balık bilgilerini ve quiz sorularını yükleyen
    /// merkezi statik veritabanı sınıfı. İlk erişimde Resources veya
    /// proje klasöründen yükler ve bellekte tutar.
    /// </summary>
    public static class FishDatabase
    {
        private const string InfoResourcePath = "FishInfoDB";   // Resources/FishInfoDB.json varsa
        private const string QuizResourcePath = "FishQuizDB";

        // Proje klasöründeki konumlar (Resources içinde değilse fallback olarak okunur)
        private const string InfoFileRelative = "Proje_AR_Folder/Project_JSON/FishInfoDB.json";
        private const string QuizFileRelative = "Proje_AR_Folder/Project_JSON/FishQuizDB.json";

        private static FishInfoCollection _infoCollection;
        private static FishQuizCollection _quizCollection;

        private static bool _loaded;

        public static void EnsureLoaded()
        {
            if (_loaded) return;
            LoadInfo();
            LoadQuiz();
            _loaded = true;
        }

        public static void Reload()
        {
            _loaded = false;
            _infoCollection = null;
            _quizCollection = null;
            EnsureLoaded();
        }

        private static void LoadInfo()
        {
            string json = LoadJson(InfoResourcePath, InfoFileRelative);
            if (string.IsNullOrEmpty(json))
            {
                _infoCollection = new FishInfoCollection();
                Debug.LogWarning("[FishDatabase] FishInfoDB.json yüklenemedi.");
                return;
            }

            try
            {
                _infoCollection = JsonUtility.FromJson<FishInfoCollection>(json);
                if (_infoCollection == null) _infoCollection = new FishInfoCollection();
            }
            catch (Exception e)
            {
                Debug.LogError($"[FishDatabase] FishInfoDB parse hatası: {e.Message}");
                _infoCollection = new FishInfoCollection();
            }
        }

        private static void LoadQuiz()
        {
            string json = LoadJson(QuizResourcePath, QuizFileRelative);
            if (string.IsNullOrEmpty(json))
            {
                _quizCollection = new FishQuizCollection();
                Debug.LogWarning("[FishDatabase] FishQuizDB.json yüklenemedi.");
                return;
            }

            try
            {
                _quizCollection = JsonUtility.FromJson<FishQuizCollection>(json);
                if (_quizCollection == null) _quizCollection = new FishQuizCollection();
            }
            catch (Exception e)
            {
                Debug.LogError($"[FishDatabase] FishQuizDB parse hatası: {e.Message}");
                _quizCollection = new FishQuizCollection();
            }
        }

        private static string LoadJson(string resourceName, string relativePath)
        {
#if UNITY_EDITOR
            // Editor'da ÖNCE Proje_AR_Folder/Project_JSON klasöründeki canlı dosyayı oku.
            // Bu sayede kullanıcı oradaki JSON'u düzenlediğinde değişiklikler hemen yansır
            // (Resources kopyası eski kalsa bile).
            string editorFullPath = System.IO.Path.Combine(Application.dataPath, relativePath);
            if (System.IO.File.Exists(editorFullPath))
            {
                try
                {
                    string txt = System.IO.File.ReadAllText(editorFullPath);
                    if (!string.IsNullOrEmpty(txt)) return txt;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[FishDatabase] {editorFullPath} okunamadı: {e.Message}");
                }
            }
#endif

            // 1) Resources içinden dene (build'de ana yol)
            var ta = Resources.Load<TextAsset>(resourceName);
            if (ta != null && !string.IsNullOrEmpty(ta.text))
                return ta.text;

            // 2) Standalone / oyun içi: Assets klasöründen dosya olarak oku (varsa)
            string fullPath = System.IO.Path.Combine(Application.dataPath, relativePath);
            if (System.IO.File.Exists(fullPath))
            {
                try { return System.IO.File.ReadAllText(fullPath); }
                catch (Exception e)
                {
                    Debug.LogError($"[FishDatabase] {fullPath} okunamadı: {e.Message}");
                }
            }

            // 3) StreamingAssets fallback (build için)
            string sa = System.IO.Path.Combine(Application.streamingAssetsPath, System.IO.Path.GetFileName(relativePath));
            if (System.IO.File.Exists(sa))
            {
                try { return System.IO.File.ReadAllText(sa); }
                catch { /* yoksay */ }
            }

            return null;
        }

        public static FishInfo GetInfo(string fishId)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(fishId) || _infoCollection?.fishInfos == null) return null;
            return _infoCollection.fishInfos.Find(f => f.fishId == fishId);
        }

        public static FishQuiz GetQuiz(string fishId)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(fishId) || _quizCollection?.fishQuizzes == null) return null;
            return _quizCollection.fishQuizzes.Find(q => q.fishId == fishId);
        }

        public static List<string> GetAllFishIds()
        {
            EnsureLoaded();
            var ids = new List<string>();
            if (_infoCollection?.fishInfos != null)
            {
                foreach (var f in _infoCollection.fishInfos)
                {
                    if (!string.IsNullOrEmpty(f.fishId) && !ids.Contains(f.fishId))
                        ids.Add(f.fishId);
                }
            }
            if (_quizCollection?.fishQuizzes != null)
            {
                foreach (var q in _quizCollection.fishQuizzes)
                {
                    if (!string.IsNullOrEmpty(q.fishId) && !ids.Contains(q.fishId))
                        ids.Add(q.fishId);
                }
            }
            return ids;
        }
    }
}
