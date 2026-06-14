using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ARFishQuiz.EditorTools
{
    public static class SetActiveBuildScene
    {
        private const string TargetScenePath = "Assets/Scenes/SampleScene.unity";

        [MenuItem("Tools/AR Fish/Build/Use Scenes-SampleScene For Build", priority = 100)]
        public static void Apply()
        {
            // 1) Build Settings'te SADECE Assets/Scenes/SampleScene.unity olsun
            var newList = new[]
            {
                new EditorBuildSettingsScene(TargetScenePath, true)
            };
            EditorBuildSettings.scenes = newList;
            Debug.Log("[Build] Build Settings güncellendi. Aktif sahne: " + TargetScenePath);

            // 2) Editor'de bu sahneyi açalım ki "kaydet" işlemleri buraya yansısın
            if (EditorSceneManager.GetActiveScene().path != TargetScenePath)
            {
                EditorSceneManager.SaveOpenScenes();
                EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
                Debug.Log("[Build] Aktif sahne açıldı: " + TargetScenePath);
            }

            // 3) Asset'leri yenile
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
