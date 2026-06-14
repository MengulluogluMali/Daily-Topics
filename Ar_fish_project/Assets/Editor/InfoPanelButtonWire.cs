#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using ARFishQuiz;

public static class InfoPanelButtonWire
{
    /// <summary>
    /// InfoPanel üzerindeki "StartQuizButton" butonunu QuizManager.StartQuiz'i
    /// çağıracak şekilde bağlar ve butonun metnini "Quiz'i Başlat" olarak ayarlar.
    /// Ayrıca infoCloseButton referansını null yapar (zaten bu buton quiz başlatıyor).
    /// </summary>
    public static void Execute()
    {
        string canvasPath = "zargana_balıgı_target/QuizCanvas";
        var canvasGO = GameObject.Find(canvasPath);
        if (canvasGO == null)
        {
            Debug.LogError($"[InfoPanelButtonWire] '{canvasPath}' bulunamadı.");
            return;
        }

        var qm = canvasGO.GetComponent<QuizManager>();
        if (qm == null)
        {
            Debug.LogError("[InfoPanelButtonWire] QuizManager bulunamadı.");
            return;
        }

        var btn = canvasGO.transform.Find("InfoPanel/StartQuizButton")?.GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogError("[InfoPanelButtonWire] StartQuizButton bulunamadı.");
            return;
        }

        // Buton metnini güncelle
        var txt = btn.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
        if (txt != null) txt.text = "Quiz'i Başlat";

        // Persistent listener'ları temizle
        for (int i = btn.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            UnityEventTools.RemovePersistentListener(btn.onClick, i);
        }

        // QuizManager.StartQuiz'i persistent olarak ekle
        UnityAction action = qm.StartQuiz;
        UnityEventTools.AddPersistentListener(btn.onClick, action);

        // QuizManager.infoCloseButton referansını null yap (çakışmasın diye)
        var so = new SerializedObject(qm);
        so.Update();
        var p = so.FindProperty("infoCloseButton");
        if (p != null) p.objectReferenceValue = null;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(btn);
        EditorUtility.SetDirty(qm);
        EditorSceneManager.MarkSceneDirty(canvasGO.scene);
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[InfoPanelButtonWire] Buton bağlandı: StartQuizButton -> QuizManager.StartQuiz");
    }
}
#endif
