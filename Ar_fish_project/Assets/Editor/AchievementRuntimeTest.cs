using UnityEditor;
using UnityEngine;
using ARFishQuiz;

namespace ARFishQuiz.EditorTools
{
    public static class AchievementRuntimeTest
    {
        [MenuItem("Tools/AR Fish/Achievements/Reset Progress (PlayerPrefs)", priority = 50)]
        public static void Reset()
        {
            string[] keys = {
                "ach_unlocked_v1", "ach_scanned_fish_v1", "ach_total_scans_v1",
                "ach_perfect_quiz_v1", "ach_drawing_count_v1", "ach_saved_draw_v1", "ach_ar_seconds_v1"
            };
            foreach (var k in keys) PlayerPrefs.DeleteKey(k);
            PlayerPrefs.Save();
            Debug.Log("[AchievementTest] Tüm ilerleme sıfırlandı.");
        }

        [MenuItem("Tools/AR Fish/Achievements/Test - Sim 3 Fish Scans", priority = 51)]
        public static void Sim3Scans()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Bu komut Play Mode'da çalışır. Ya da sahnede AchievementManager Awake olmuştur.");
                return;
            }
            var mgr = AchievementManager.Instance;
            if (mgr == null) { Debug.LogError("AchievementManager bulunamadı."); return; }
            mgr.NotifyFishScanned("zargana");
            mgr.NotifyFishScanned("mersin");
            mgr.NotifyFishScanned("balon_baligi");
        }
    }
}
