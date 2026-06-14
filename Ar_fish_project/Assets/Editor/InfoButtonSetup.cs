#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ARFishQuiz;

public static class InfoButtonSetup
{
    /// <summary>
    /// 1) infoButton üzerindeki QuizButton3D scriptini kaldırır, InfoButton3D ekler ve QuizManager'a bağlar.
    /// 2) QuizManager.quizTitleText referansını QuizCanvas/QuizPanel/Title'a atar.
    /// 3) QuizPanel/Title metnini başlangıç olarak balık ismine göre günceller.
    /// </summary>
    public static void Execute()
    {
        // --- infoButton'u bul ---
        var canvasGO = GameObject.Find("zargana_balıgı_target/QuizCanvas");
        if (canvasGO == null) { Debug.LogError("QuizCanvas bulunamadı."); return; }

        var quizManager = canvasGO.GetComponent<QuizManager>();
        if (quizManager == null) { Debug.LogError("QuizManager bulunamadı."); return; }

        // infoButton ismi sonunda boşluk içerebilir; her iki olasılığı dene
        GameObject infoButton = GameObject.Find("zargana_balıgı_target/infoButton ") 
                                ?? GameObject.Find("zargana_balıgı_target/infoButton");
        if (infoButton == null)
        {
            // Sahnedeki tüm root altında "infoButton" başlangıçlı gameobject ara
            var root = GameObject.Find("zargana_balıgı_target");
            if (root != null)
            {
                foreach (Transform t in root.transform)
                {
                    if (t.name.Trim() == "infoButton")
                    {
                        infoButton = t.gameObject;
                        break;
                    }
                }
            }
        }
        if (infoButton == null) { Debug.LogError("infoButton bulunamadı."); return; }

        // --- QuizButton3D varsa kaldır ---
        var existingQuizBtn = infoButton.GetComponent<QuizButton3D>();
        if (existingQuizBtn != null)
        {
            Object.DestroyImmediate(existingQuizBtn, true);
            Debug.Log("[InfoButtonSetup] infoButton üzerindeki QuizButton3D kaldırıldı.");
        }

        // --- InfoButton3D ekle (yoksa) ---
        var infoBtnScript = infoButton.GetComponent<InfoButton3D>();
        if (infoBtnScript == null)
        {
            infoBtnScript = infoButton.AddComponent<InfoButton3D>();
        }

        // ARCamera referansı
        var arCam = GameObject.Find("ARCamera")?.GetComponent<Camera>();

        // SerializedObject ile alan ata
        var soBtn = new SerializedObject(infoBtnScript);
        soBtn.Update();
        var qmProp = soBtn.FindProperty("quizManager");
        if (qmProp != null) qmProp.objectReferenceValue = quizManager;
        var camProp = soBtn.FindProperty("raycastCamera");
        if (camProp != null && arCam != null) camProp.objectReferenceValue = arCam;
        var dbgProp = soBtn.FindProperty("debugLog");
        if (dbgProp != null) dbgProp.boolValue = true;
        soBtn.ApplyModifiedPropertiesWithoutUndo();

        // --- Quiz başlığını QuizManager.quizTitleText alanına ata ---
        var titleTMP = canvasGO.transform.Find("QuizPanel/Title")?.GetComponent<TMP_Text>();
        if (titleTMP != null)
        {
            var soQM = new SerializedObject(quizManager);
            soQM.Update();
            var titleProp = soQM.FindProperty("quizTitleText");
            if (titleProp != null)
            {
                titleProp.objectReferenceValue = titleTMP;
                Debug.Log("[InfoButtonSetup] QuizManager.quizTitleText -> QuizPanel/Title atandı.");
            }
            soQM.ApplyModifiedPropertiesWithoutUndo();

            // Editor'da görünüm için başlığı güncelle
            var info = FishDatabase.GetInfo("zargana");
            string displayName = info != null ? info.displayName : "Zargana";
            titleTMP.text = $"🐠 {displayName} Quiz 🐠";
        }

        EditorUtility.SetDirty(infoButton);
        EditorUtility.SetDirty(quizManager);
        if (titleTMP != null) EditorUtility.SetDirty(titleTMP);
        EditorSceneManager.MarkSceneDirty(canvasGO.scene);
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[InfoButtonSetup] InfoButton kurulumu tamamlandı.");
    }
}
#endif
