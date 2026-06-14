#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ARFishQuiz;

public static class PreviewInfoPanel
{
    public static void Execute()
    {
        string canvasPath = "zargana_balıgı_target/QuizCanvas";
        var canvasGO = GameObject.Find(canvasPath);
        if (canvasGO == null) { Debug.LogError("Canvas yok."); return; }

        var infoPanel = canvasGO.transform.Find("InfoPanel")?.gameObject;
        var quizPanel = canvasGO.transform.Find("QuizPanel")?.gameObject;
        if (infoPanel == null) { Debug.LogError("InfoPanel yok."); return; }

        // QuizPanel'i kapat, InfoPanel'i aç
        if (quizPanel != null) quizPanel.SetActive(false);
        infoPanel.SetActive(true);

        // Önizleme amaçlı verileri populate et
        var info = FishDatabase.GetInfo("zargana");
        if (info != null)
        {
            var t = infoPanel.transform.Find("Title")?.GetComponent<TMP_Text>();
            if (t != null) t.text = info.displayName;
            var s = infoPanel.transform.Find("Scientific")?.GetComponent<TMP_Text>();
            if (s != null) s.text = info.scientificName;
            var d = infoPanel.transform.Find("Description")?.GetComponent<TMP_Text>();
            if (d != null) d.text = info.shortDescription;
            var h = infoPanel.transform.Find("Habitat")?.GetComponent<TMP_Text>();
            if (h != null) h.text = $"<b>Yaşam Alanı:</b> {info.habitat}";
            var di = infoPanel.transform.Find("Diet")?.GetComponent<TMP_Text>();
            if (di != null) di.text = $"<b>Beslenme:</b> {info.diet}";
        }

        Debug.Log("Önizleme için InfoPanel açıldı ve dolduruldu.");
    }
}
#endif
