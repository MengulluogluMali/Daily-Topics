using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.EventSystems;
using ARFishQuiz;

public static class QuizInputFix
{
    public static void Execute()
    {
        // 1. ARCamera'yı bul ve QuizButton3D'ye explicit olarak ata
        var arCamGO = GameObject.Find("ARCamera");
        Camera arCam = arCamGO != null ? arCamGO.GetComponent<Camera>() : null;
        if (arCam == null)
        {
            Debug.LogError("[QuizInputFix] ARCamera bulunamadı!");
            return;
        }

        var btnGO = GameObject.Find("ImageTarget/QuizButton");
        if (btnGO == null)
        {
            Debug.LogError("[QuizInputFix] QuizButton bulunamadı!");
            return;
        }

        var qb3d = btnGO.GetComponent<QuizButton3D>();
        var so = new SerializedObject(qb3d);
        so.FindProperty("raycastCamera").objectReferenceValue = arCam;
        so.FindProperty("debugLog").boolValue = true; // test için debug açık
        so.ApplyModifiedProperties();
        Debug.Log($"[QuizInputFix] QuizButton3D.raycastCamera = {arCam.name} (debug aktif)");

        // 2. EventSystem'deki StandaloneInputModule'u, yeni Input System modunda ise
        // InputSystemUIInputModule ile değiştir.
        var esGO = Object.FindFirstObjectByType<EventSystem>();
        if (esGO != null)
        {
            var oldModule = esGO.GetComponent<StandaloneInputModule>();
            if (oldModule != null)
            {
                // Proje yeni Input System modunda (activeInputHandler=1).
                // Bu nedenle eski StandaloneInputModule'u kaldırıp InputSystemUIInputModule ekliyoruz.
                Object.DestroyImmediate(oldModule);
                var newModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (newModuleType != null)
                {
                    esGO.gameObject.AddComponent(newModuleType);
                    Debug.Log("[QuizInputFix] EventSystem -> InputSystemUIInputModule olarak güncellendi.");
                }
                else
                {
                    Debug.LogError("[QuizInputFix] InputSystemUIInputModule type bulunamadı! Input System paketini kontrol edin.");
                }
            }
            else
            {
                // Modül zaten yoksa ekle
                var existingNew = esGO.GetComponent("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
                if (existingNew == null)
                {
                    var newModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                    if (newModuleType != null)
                    {
                        esGO.gameObject.AddComponent(newModuleType);
                        Debug.Log("[QuizInputFix] InputSystemUIInputModule eklendi.");
                    }
                }
            }
        }

        // 3. Buton Collider kontrolü
        var col = btnGO.GetComponent<Collider>();
        if (col == null)
        {
            btnGO.AddComponent<BoxCollider>();
            Debug.Log("[QuizInputFix] BoxCollider eklendi.");
        }
        else
        {
            Debug.Log($"[QuizInputFix] Collider OK: {col.GetType().Name}, enabled={col.enabled}, isTrigger={((col is BoxCollider bc) ? bc.isTrigger.ToString() : "-")}");
        }

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[QuizInputFix] Tamamlandı. Artık Play modda mouse sol tıklama butonu tetikler.");
    }
}
