#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ARFishQuiz;

public static class InfoPanelSetup
{
    /// <summary>
    /// Zargana QuizCanvas'a (ve istenirse tüm balıklara) InfoPanel ekler ve
    /// QuizManager üzerinde referansları atar.
    /// </summary>
    public static void Execute()
    {
        // Hedef olarak zargana_balıgı_target/QuizCanvas
        string canvasPath = "zargana_balıgı_target/QuizCanvas";
        var canvasGO = GameObject.Find(canvasPath);
        if (canvasGO == null)
        {
            Debug.LogError($"[InfoPanelSetup] '{canvasPath}' bulunamadı.");
            return;
        }

        var quizManager = canvasGO.GetComponent<QuizManager>();
        if (quizManager == null)
        {
            Debug.LogError("[InfoPanelSetup] QuizManager bulunamadı.");
            return;
        }

        // Mevcut InfoPanel var mı?
        Transform existing = canvasGO.transform.Find("InfoPanel");
        GameObject infoPanel = existing != null ? existing.gameObject : null;
        if (infoPanel == null)
        {
            infoPanel = CreateInfoPanel(canvasGO);
        }

        // QuizManager üzerine referansları ata
        var so = new SerializedObject(quizManager);
        so.Update();

        SetObj(so, "infoPanel", infoPanel);
        SetObj(so, "infoTitleText", infoPanel.transform.Find("Title")?.GetComponent<TMP_Text>());
        SetObj(so, "infoScientificNameText", infoPanel.transform.Find("Scientific")?.GetComponent<TMP_Text>());
        SetObj(so, "infoDescriptionText", infoPanel.transform.Find("Description")?.GetComponent<TMP_Text>());
        SetObj(so, "infoHabitatText", infoPanel.transform.Find("Habitat")?.GetComponent<TMP_Text>());
        SetObj(so, "infoDietText", infoPanel.transform.Find("Diet")?.GetComponent<TMP_Text>());
        SetObj(so, "infoCloseButton", infoPanel.transform.Find("StartQuizButton")?.GetComponent<Button>());
        // showInfoOnStart varsayılan true, bırakıyoruz.

        // FishId varsayılan 'zargana' olarak set et
        var fishIdProp = so.FindProperty("fishId");
        if (fishIdProp != null && string.IsNullOrEmpty(fishIdProp.stringValue))
            fishIdProp.stringValue = "zargana";

        so.ApplyModifiedPropertiesWithoutUndo();

        // Sahnenin değişikliği kaydet
        EditorSceneManager.MarkSceneDirty(canvasGO.scene);
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[InfoPanelSetup] InfoPanel eklendi ve QuizManager referansları atandı.");
    }

    private static void SetObj(SerializedObject so, string propName, Object obj)
    {
        var p = so.FindProperty(propName);
        if (p != null) p.objectReferenceValue = obj;
    }

    private static GameObject CreateInfoPanel(GameObject canvasGO)
    {
        // Ana panel
        GameObject panel = new GameObject("InfoPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);

        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localPosition = Vector3.zero;
        rt.localScale = Vector3.one;

        var img = panel.GetComponent<Image>();
        img.color = new Color(0.05f, 0.1f, 0.25f, 0.95f);

        // Başlık
        CreateText(panel.transform, "Title", "Zargana",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(20f, -20f), new Vector2(-20f, -80f),
            42, FontStyles.Bold, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.3f, 1f));

        // Bilimsel ad (italik, küçük)
        CreateText(panel.transform, "Scientific", "Belone belone",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(20f, -85f), new Vector2(-20f, -125f),
            24, FontStyles.Italic, TextAlignmentOptions.Center, new Color(0.85f, 0.9f, 1f, 1f));

        // Açıklama (uzun metin)
        CreateText(panel.transform, "Description", "Bilgi yükleniyor...",
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(30f, 220f), new Vector2(-30f, -140f),
            22, FontStyles.Normal, TextAlignmentOptions.TopLeft, Color.white);

        // Yaşam alanı
        CreateText(panel.transform, "Habitat", "<b>Yaşam Alanı:</b>",
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(30f, 160f), new Vector2(-30f, 215f),
            20, FontStyles.Normal, TextAlignmentOptions.TopLeft, new Color(0.7f, 0.95f, 0.9f, 1f));

        // Beslenme
        CreateText(panel.transform, "Diet", "<b>Beslenme:</b>",
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(30f, 105f), new Vector2(-30f, 160f),
            20, FontStyles.Normal, TextAlignmentOptions.TopLeft, new Color(0.95f, 0.85f, 0.7f, 1f));

        // Quiz başlat / kapat butonu
        GameObject btnGO = new GameObject("StartQuizButton",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(panel.transform, false);
        var brt = btnGO.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.5f, 0f);
        brt.anchorMax = new Vector2(0.5f, 0f);
        brt.pivot = new Vector2(0.5f, 0f);
        brt.anchoredPosition = new Vector2(0f, 25f);
        brt.sizeDelta = new Vector2(360f, 60f);

        var bImg = btnGO.GetComponent<Image>();
        bImg.color = new Color(0.2f, 0.6f, 0.95f, 1f);

        // Buton metni
        GameObject btnTextGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        btnTextGO.transform.SetParent(btnGO.transform, false);
        var btr = btnTextGO.GetComponent<RectTransform>();
        btr.anchorMin = Vector2.zero;
        btr.anchorMax = Vector2.one;
        btr.offsetMin = Vector2.zero;
        btr.offsetMax = Vector2.zero;
        var btxt = btnTextGO.GetComponent<TextMeshProUGUI>();
        btxt.text = "Kapat";
        btxt.fontSize = 28;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;

        return panel;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
        float fontSize, FontStyles style, TextAlignmentOptions align, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = align;
        tmp.color = color;
        tmp.enableWordWrapping = true;
        tmp.richText = true;

        return tmp;
    }
}
#endif
