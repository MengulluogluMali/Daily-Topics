using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using ARFishQuiz;

namespace ARFishQuiz.EditorTools
{
    public static class AchievementPreview
    {
        [MenuItem("Tools/AR Fish/Achievements/Preview Open List", priority = 11)]
        public static void OpenList()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()) return;

            var ui = Object.FindFirstObjectByType<AchievementUIManager>();
            if (ui == null)
            {
                Debug.LogError("AchievementUIManager bulunamadı.");
                return;
            }

            // Manager Awake olmadığı için DB'yi manuel yükle ve listenin görünümünü zorla
            AchievementDatabase.EnsureLoaded();

            var listPanel = ui.transform.Find("ListPanel");
            if (listPanel != null) listPanel.gameObject.SetActive(true);

            // RebuildList'i invoke etmek için method'u public yapmadan reflektif çağır
            var m = typeof(AchievementUIManager).GetMethod("RebuildList", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (m != null) m.Invoke(ui, null);

            EditorUtility.SetDirty(ui);
            Debug.Log("[AchievementPreview] Liste açıldı.");
        }

        [MenuItem("Tools/AR Fish/Achievements/Preview Close List", priority = 12)]
        public static void CloseList()
        {
            var ui = Object.FindFirstObjectByType<AchievementUIManager>();
            if (ui == null) return;
            var listPanel = ui.transform.Find("ListPanel");
            if (listPanel != null) listPanel.gameObject.SetActive(false);
        }
    }
}
