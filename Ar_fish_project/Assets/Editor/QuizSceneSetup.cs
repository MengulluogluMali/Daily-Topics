using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using ARFishQuiz;

public static class QuizSceneSetup
{
    public static void Execute()
    {
        var scene = EditorSceneManager.GetActiveScene();
        GameObject imageTarget = GameObject.Find("ImageTarget");
        if (imageTarget == null)
        {
            Debug.LogError("ImageTarget bulunamadı!");
            return;
        }

        // ---- 1) ImageTarget üzerine 3D Quiz Butonu oluştur ----
        // Önce var olan butonu temizle
        var existingBtn = imageTarget.transform.Find("QuizButton");
        if (existingBtn != null) Object.DestroyImmediate(existingBtn.gameObject);

        GameObject quizBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
        quizBtn.name = "QuizButton";
        quizBtn.transform.SetParent(imageTarget.transform, false);
        // ImageTarget'ın yanına yerleştir (sağ tarafına)
        quizBtn.transform.localPosition = new Vector3(4.5f, 0.3f, 2.5f);
        quizBtn.transform.localScale = new Vector3(1.8f, 0.6f, 1.0f);

        // Buton rengini ayarla (Turuncu - Nemo teması)
        var renderer = quizBtn.GetComponent<MeshRenderer>();
        Material btnMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        btnMat.color = new Color(1f, 0.55f, 0.1f);
        btnMat.name = "QuizButtonMat";
        renderer.sharedMaterial = btnMat;

        // ---- 2) Butonun üstüne "QUIZ" yazısı ekle ----
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(quizBtn.transform, false);
        labelGO.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        labelGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        labelGO.transform.localScale = Vector3.one * 0.5f;
        var tmp = labelGO.AddComponent<TextMeshPro>();
        tmp.text = "QUIZ";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 4f;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;

        // ---- 3) World Space Quiz Canvas oluştur ----
        var existingCanvas = imageTarget.transform.Find("QuizCanvas");
        if (existingCanvas != null) Object.DestroyImmediate(existingCanvas.gameObject);

        GameObject canvasGO = new GameObject("QuizCanvas");
        canvasGO.transform.SetParent(imageTarget.transform, false);
        canvasGO.transform.localPosition = new Vector3(0f, 2f, 0f);
        canvasGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        canvasGO.transform.localScale = Vector3.one * 0.01f;

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 2f;
        canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(800, 600);

        // EventSystem yoksa oluştur
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ---- 4) Quiz Panel (Ana Container) ----
        GameObject quizPanel = CreateUIPanel("QuizPanel", canvasRT, new Color(0.05f, 0.1f, 0.25f, 0.95f));
        RectTransform quizPanelRT = quizPanel.GetComponent<RectTransform>();
        quizPanelRT.anchorMin = Vector2.zero;
        quizPanelRT.anchorMax = Vector2.one;
        quizPanelRT.offsetMin = Vector2.zero;
        quizPanelRT.offsetMax = Vector2.zero;

        // Başlık
        GameObject title = CreateTMPText("Title", quizPanel.transform, "🐠 NEMO QUIZ 🐠", 42, TextAlignmentOptions.Center);
        RectTransform titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0, -20);
        titleRT.sizeDelta = new Vector2(0, 80);
        title.GetComponent<TMP_Text>().color = new Color(1f, 0.7f, 0.2f);
        title.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;

        // Kapatma butonu
        GameObject closeBtn = CreateUIButton("CloseButton", quizPanel.transform, "X", new Color(0.8f, 0.2f, 0.2f));
        RectTransform closeRT = closeBtn.GetComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(1, 1);
        closeRT.anchorMax = new Vector2(1, 1);
        closeRT.pivot = new Vector2(1, 1);
        closeRT.anchoredPosition = new Vector2(-15, -15);
        closeRT.sizeDelta = new Vector2(60, 60);

        // ---- 5) Question Panel ----
        GameObject questionPanel = CreateUIPanel("QuestionPanel", quizPanel.transform, new Color(0, 0, 0, 0));
        RectTransform qpRT = questionPanel.GetComponent<RectTransform>();
        qpRT.anchorMin = new Vector2(0, 0);
        qpRT.anchorMax = new Vector2(1, 1);
        qpRT.offsetMin = new Vector2(20, 20);
        qpRT.offsetMax = new Vector2(-20, -100);

        // Soru sayaç
        GameObject counter = CreateTMPText("Counter", questionPanel.transform, "Soru 1 / 5", 28, TextAlignmentOptions.Center);
        RectTransform counterRT = counter.GetComponent<RectTransform>();
        counterRT.anchorMin = new Vector2(0, 1);
        counterRT.anchorMax = new Vector2(1, 1);
        counterRT.pivot = new Vector2(0.5f, 1f);
        counterRT.anchoredPosition = new Vector2(0, 0);
        counterRT.sizeDelta = new Vector2(0, 50);
        counter.GetComponent<TMP_Text>().color = new Color(0.8f, 0.9f, 1f);

        // Soru metni
        GameObject qText = CreateTMPText("QuestionText", questionPanel.transform, "Soru buraya gelecek", 32, TextAlignmentOptions.Center);
        RectTransform qTextRT = qText.GetComponent<RectTransform>();
        qTextRT.anchorMin = new Vector2(0, 1);
        qTextRT.anchorMax = new Vector2(1, 1);
        qTextRT.pivot = new Vector2(0.5f, 1f);
        qTextRT.anchoredPosition = new Vector2(0, -60);
        qTextRT.sizeDelta = new Vector2(0, 140);
        qText.GetComponent<TMP_Text>().color = Color.white;
        qText.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;

        // Cevap butonları (2x2 grid)
        Button[] answerButtons = new Button[4];
        TMP_Text[] answerTexts = new TMP_Text[4];

        float btnW = 350f;
        float btnH = 90f;
        float gapX = 20f;
        float gapY = 20f;
        float startY = -220f;

        for (int i = 0; i < 4; i++)
        {
            int row = i / 2;
            int col = i % 2;
            float x = (col - 0.5f) * (btnW + gapX);
            float y = startY - row * (btnH + gapY);

            GameObject abtn = CreateUIButton($"AnswerButton_{i}", questionPanel.transform, $"Cevap {i + 1}", Color.white);
            RectTransform abRT = abtn.GetComponent<RectTransform>();
            abRT.anchorMin = new Vector2(0.5f, 1f);
            abRT.anchorMax = new Vector2(0.5f, 1f);
            abRT.pivot = new Vector2(0.5f, 1f);
            abRT.anchoredPosition = new Vector2(x, y);
            abRT.sizeDelta = new Vector2(btnW, btnH);

            answerButtons[i] = abtn.GetComponent<Button>();
            answerTexts[i] = abtn.GetComponentInChildren<TMP_Text>();
            answerTexts[i].color = new Color(0.1f, 0.1f, 0.2f);
            answerTexts[i].fontSize = 26;
        }

        // ---- 6) Result Panel ----
        GameObject resultPanel = CreateUIPanel("ResultPanel", quizPanel.transform, new Color(0, 0, 0, 0));
        RectTransform rpRT = resultPanel.GetComponent<RectTransform>();
        rpRT.anchorMin = new Vector2(0, 0);
        rpRT.anchorMax = new Vector2(1, 1);
        rpRT.offsetMin = new Vector2(20, 20);
        rpRT.offsetMax = new Vector2(-20, -100);

        GameObject resText = CreateTMPText("ResultText", resultPanel.transform, "Sonuç", 36, TextAlignmentOptions.Center);
        RectTransform resTextRT = resText.GetComponent<RectTransform>();
        resTextRT.anchorMin = new Vector2(0, 0.3f);
        resTextRT.anchorMax = new Vector2(1, 1);
        resTextRT.offsetMin = Vector2.zero;
        resTextRT.offsetMax = Vector2.zero;
        resText.GetComponent<TMP_Text>().color = Color.white;
        resText.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;

        GameObject restartBtn = CreateUIButton("RestartButton", resultPanel.transform, "Tekrar Dene", new Color(0.2f, 0.7f, 0.4f));
        RectTransform rbRT = restartBtn.GetComponent<RectTransform>();
        rbRT.anchorMin = new Vector2(0.5f, 0);
        rbRT.anchorMax = new Vector2(0.5f, 0);
        rbRT.pivot = new Vector2(0.5f, 0);
        rbRT.anchoredPosition = new Vector2(0, 30);
        rbRT.sizeDelta = new Vector2(300, 80);
        var rbText = restartBtn.GetComponentInChildren<TMP_Text>();
        rbText.color = Color.white;
        rbText.fontSize = 28;
        rbText.fontStyle = FontStyles.Bold;

        // ---- 7) QuizManager component'ini ekle ve bağla ----
        var existingMgr = canvasGO.GetComponent<QuizManager>();
        if (existingMgr != null) Object.DestroyImmediate(existingMgr);
        QuizManager manager = canvasGO.AddComponent<QuizManager>();

        // Reflection ile private SerializeField'lara atama (SerializedObject daha temiz)
        var so = new SerializedObject(manager);
        so.FindProperty("quizPanel").objectReferenceValue = quizPanel;
        so.FindProperty("questionPanel").objectReferenceValue = questionPanel;
        so.FindProperty("resultPanel").objectReferenceValue = resultPanel;
        so.FindProperty("questionText").objectReferenceValue = qText.GetComponent<TMP_Text>();
        so.FindProperty("questionCounterText").objectReferenceValue = counter.GetComponent<TMP_Text>();

        var btnArr = so.FindProperty("answerButtons");
        btnArr.arraySize = 4;
        var txtArr = so.FindProperty("answerTexts");
        txtArr.arraySize = 4;
        for (int i = 0; i < 4; i++)
        {
            btnArr.GetArrayElementAtIndex(i).objectReferenceValue = answerButtons[i];
            txtArr.GetArrayElementAtIndex(i).objectReferenceValue = answerTexts[i];
        }

        so.FindProperty("resultText").objectReferenceValue = resText.GetComponent<TMP_Text>();
        so.FindProperty("restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        so.FindProperty("closeButton").objectReferenceValue = closeBtn.GetComponent<Button>();
        so.ApplyModifiedProperties();

        // Başlangıçta paneli kapat
        quizPanel.SetActive(false);

        // ---- 8) 3D butona QuizButton3D ekle ve bağla ----
        var qb3d = quizBtn.AddComponent<QuizButton3D>();
        var soBtn = new SerializedObject(qb3d);
        soBtn.FindProperty("quizManager").objectReferenceValue = manager;
        soBtn.ApplyModifiedProperties();

        // Sahneyi kaydet
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[QuizSceneSetup] Quiz sistemi başarıyla sahneye eklendi!");
    }

    private static GameObject CreateUIPanel(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    private static GameObject CreateTMPText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = true;
        return go;
    }

    private static GameObject CreateUIButton(string name, Transform parent, string label, Color bgColor)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = bgColor;

        GameObject textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(go.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 30;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        RectTransform trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        return go;
    }
}
