using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using ARFishQuiz;

namespace ARFishQuizEditor
{
    /// <summary>
    /// Tüm QuizManager'ların InfoPanel TMP metinlerini, FishInfoDB.json'daki
    /// doğru bilgilerle baştan doldurur (build-time sahnede). Böylece JSON
    /// runtime'da yüklenemese bile her balığın paneli kendi bilgisini gösterir.
    /// Menü: Tools/Aquarium/Prebake Info Panels
    /// </summary>
    public static class PrebakeInfoPanels
    {
        [MenuItem("Tools/Aquarium/Prebake Info Panels")]
        public static void Prebake()
        {
            FishDatabase.Reload();

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            int count = 0;
            int missing = 0;

            foreach (var root in scene.GetRootGameObjects())
            {
                var qms = root.GetComponentsInChildren<QuizManager>(true);
                foreach (var qm in qms)
                {
                    var fishIdField = typeof(QuizManager).GetField("fishId", BindingFlags.NonPublic | BindingFlags.Instance);
                    string fishId = fishIdField.GetValue(qm) as string;
                    if (string.IsNullOrEmpty(fishId))
                    {
                        Debug.LogWarning($"[Prebake] QuizManager on {qm.name} has no fishId");
                        continue;
                    }

                    FishInfo info = FishDatabase.GetInfo(fishId);
                    if (info == null)
                    {
                        Debug.LogWarning($"[Prebake] FishInfo bulunamadı: '{fishId}' (GO: {qm.transform.parent?.name}/{qm.name})");
                        missing++;
                        continue;
                    }

                    SetTMP(qm, "infoTitleText", info.displayName);
                    SetTMP(qm, "infoScientificNameText", info.scientificName);
                    SetTMP(qm, "infoDescriptionText", info.shortDescription);
                    SetTMP(qm, "infoHabitatText", $"<b>Yaşam Alanı:</b> {info.habitat}");
                    SetTMP(qm, "infoDietText", $"<b>Beslenme:</b> {info.diet}");

                    // Quiz title de prebake yapalım
                    var quizTitleField = typeof(QuizManager).GetField("quizTitleText", BindingFlags.NonPublic | BindingFlags.Instance);
                    var quizTitle = quizTitleField?.GetValue(qm) as TMP_Text;
                    if (quizTitle != null)
                        quizTitle.text = $"🐠 {info.displayName} Quiz 🐠";

                    EditorUtility.SetDirty(qm);
                    count++;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[Prebake] {count} QuizManager için InfoPanel metinleri dolduruldu. Eksik: {missing}");
        }

        private static void SetTMP(QuizManager qm, string fieldName, string value)
        {
            var f = typeof(QuizManager).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (f == null) return;
            var tmp = f.GetValue(qm) as TMP_Text;
            if (tmp == null) return;
            tmp.text = value;
            EditorUtility.SetDirty(tmp);
        }
    }
}
