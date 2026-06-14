using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using ARFishQuiz;

namespace ARFishQuiz.EditorTools
{
    /// <summary>
    /// Sahnede başarım sistemini (manager + UI: sol üst buton, liste paneli, toast) tek tıkla kurar.
    /// </summary>
    public static class AchievementSystemSetup
    {
        private const string RootName = "AchievementSystem";

        [MenuItem("Tools/AR Fish/Achievements/Setup Achievement System In Scene", priority = 10)]
        public static void SetupInActiveScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("[AchievementSetup] Aktif sahne yok.");
                return;
            }

            // Önceki kurulumu temizle (idempotent)
            var existing = GameObject.Find(RootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            var root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create AchievementSystem");

            // 1) Manager
            var managerGo = new GameObject("Manager");
            managerGo.transform.SetParent(root.transform, false);
            managerGo.AddComponent<AchievementManager>();

            // 2) Canvas (overlay, en üstte)
            var canvasGo = new GameObject("AchievementOverlayCanvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(root.transform, false);
            canvasGo.layer = LayerMask.NameToLayer("UI");
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 31010;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // 3) Sol üstteki "Başarımlar" butonu
            var btnGo = CreateUI("AchievementsButton", canvasGo.transform);
            var btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0f, 1f);
            btnRt.anchorMax = new Vector2(0f, 1f);
            btnRt.pivot = new Vector2(0f, 1f);
            btnRt.anchoredPosition = new Vector2(25f, -25f);
            btnRt.sizeDelta = new Vector2(280f, 130f);
            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.85f, 0.55f, 0.10f, 0.98f);
            var btnButton = btnGo.AddComponent<Button>();
            var btnOutline = btnGo.AddComponent<Outline>();
            btnOutline.effectColor = Color.black;
            btnOutline.effectDistance = new Vector2(3f, -3f);

            var btnLabel = CreateText("Label", btnGo.transform, "Başarımlar", 38f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            var btnLabelRt = btnLabel.rectTransform;
            btnLabelRt.anchorMin = new Vector2(0f, 0.45f);
            btnLabelRt.anchorMax = new Vector2(1f, 1f);
            btnLabelRt.offsetMin = Vector2.zero;
            btnLabelRt.offsetMax = Vector2.zero;

            var btnCounter = CreateText("Counter", btnGo.transform, "0/0", 28f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(1f, 0.95f, 0.7f));
            var btnCounterRt = btnCounter.rectTransform;
            btnCounterRt.anchorMin = new Vector2(0f, 0f);
            btnCounterRt.anchorMax = new Vector2(1f, 0.45f);
            btnCounterRt.offsetMin = Vector2.zero;
            btnCounterRt.offsetMax = Vector2.zero;

            // 4) Liste Paneli (büyük, ortada, ScrollRect ile)
            var listPanel = CreateUI("ListPanel", canvasGo.transform);
            var listPanelRt = listPanel.GetComponent<RectTransform>();
            listPanelRt.anchorMin = new Vector2(0.5f, 0.5f);
            listPanelRt.anchorMax = new Vector2(0.5f, 0.5f);
            listPanelRt.pivot = new Vector2(0.5f, 0.5f);
            listPanelRt.anchoredPosition = Vector2.zero;
            listPanelRt.sizeDelta = new Vector2(980f, 1700f);
            var listBg = listPanel.AddComponent<Image>();
            listBg.color = new Color(0.06f, 0.10f, 0.18f, 0.97f);
            var listOutline = listPanel.AddComponent<Outline>();
            listOutline.effectColor = new Color(0.85f, 0.55f, 0.10f, 1f);
            listOutline.effectDistance = new Vector2(4f, -4f);

            // Header
            var header = CreateText("Header", listPanel.transform, "BAŞARIMLAR (0/0)", 56f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(1f, 0.95f, 0.55f));
            var headerRt = header.rectTransform;
            headerRt.anchorMin = new Vector2(0f, 1f);
            headerRt.anchorMax = new Vector2(1f, 1f);
            headerRt.pivot = new Vector2(0.5f, 1f);
            headerRt.anchoredPosition = new Vector2(0f, -25f);
            headerRt.sizeDelta = new Vector2(-40f, 90f);

            // Close button (sağ üst)
            var closeBtnGo = CreateUI("CloseButton", listPanel.transform);
            var closeRt = closeBtnGo.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1f, 1f);
            closeRt.anchorMax = new Vector2(1f, 1f);
            closeRt.pivot = new Vector2(1f, 1f);
            closeRt.anchoredPosition = new Vector2(-15f, -15f);
            closeRt.sizeDelta = new Vector2(110f, 90f);
            var closeImg = closeBtnGo.AddComponent<Image>();
            closeImg.color = new Color(0.8f, 0.2f, 0.2f, 0.95f);
            var closeBtn = closeBtnGo.AddComponent<Button>();
            var closeLbl = CreateText("Label", closeBtnGo.transform, "✕", 60f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            var closeLblRt = closeLbl.rectTransform;
            closeLblRt.anchorMin = Vector2.zero; closeLblRt.anchorMax = Vector2.one;
            closeLblRt.offsetMin = Vector2.zero; closeLblRt.offsetMax = Vector2.zero;

            // ScrollRect
            var scrollGo = CreateUI("Scroll", listPanel.transform);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0f, 0f);
            scrollRt.anchorMax = new Vector2(1f, 1f);
            scrollRt.offsetMin = new Vector2(20f, 20f);
            scrollRt.offsetMax = new Vector2(-20f, -130f);
            var scrollImg = scrollGo.AddComponent<Image>();
            scrollImg.color = new Color(0.03f, 0.05f, 0.08f, 0.6f);
            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            var viewport = CreateUI("Viewport", scrollGo.transform);
            var viewportRt = viewport.GetComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;
            viewport.AddComponent<RectMask2D>();
            var viewportImg = viewport.AddComponent<Image>();
            viewportImg.color = new Color(0f, 0f, 0f, 0.0001f);

            var content = CreateUI("Content", viewport.transform);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0f, 0f);

            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(15, 15, 15, 15);
            vlg.spacing = 12f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRt;
            scrollRect.content = contentRt;

            // 5) Toast bildirimi
            var toast = CreateUI("Toast", canvasGo.transform);
            var toastRt = toast.GetComponent<RectTransform>();
            // Toast root pivotu sağ üst, ekranın sağına yapışık
            toastRt.anchorMin = new Vector2(1f, 1f);
            toastRt.anchorMax = new Vector2(1f, 1f);
            toastRt.pivot = new Vector2(1f, 1f);
            toastRt.anchoredPosition = new Vector2(-30f, -180f); // Akvaryum butonunun altı
            toastRt.sizeDelta = new Vector2(900f, 240f);
            var toastBg = toast.AddComponent<Image>();
            toastBg.color = new Color(0.10f, 0.18f, 0.30f, 0.97f);
            var toastOl = toast.AddComponent<Outline>();
            toastOl.effectColor = new Color(1f, 0.9f, 0.4f, 1f);
            toastOl.effectDistance = new Vector2(3f, -3f);

            var toastIcon = CreateText("Icon", toast.transform, "+", 110f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.3f));
            var toastIconRt = toastIcon.rectTransform;
            toastIconRt.anchorMin = new Vector2(0f, 0f);
            toastIconRt.anchorMax = new Vector2(0f, 1f);
            toastIconRt.pivot = new Vector2(0f, 0.5f);
            toastIconRt.anchoredPosition = new Vector2(20f, 0f);
            toastIconRt.sizeDelta = new Vector2(180f, 0f);

            var toastTitle = CreateText("Title", toast.transform, "Başarım Açıldı", 38f, FontStyles.Bold, TextAlignmentOptions.Left, new Color(1f, 0.95f, 0.55f));
            var toastTitleRt = toastTitle.rectTransform;
            toastTitleRt.anchorMin = new Vector2(0f, 0.5f);
            toastTitleRt.anchorMax = new Vector2(1f, 1f);
            toastTitleRt.offsetMin = new Vector2(210f, 0f);
            toastTitleRt.offsetMax = new Vector2(-20f, -15f);

            var toastDesc = CreateText("Desc", toast.transform, "Açıklama", 28f, FontStyles.Normal, TextAlignmentOptions.Left, Color.white);
            var toastDescRt = toastDesc.rectTransform;
            toastDescRt.anchorMin = new Vector2(0f, 0f);
            toastDescRt.anchorMax = new Vector2(1f, 0.5f);
            toastDescRt.offsetMin = new Vector2(210f, 15f);
            toastDescRt.offsetMax = new Vector2(-20f, 0f);

            toast.SetActive(false);
            listPanel.SetActive(false);

            // 6) UI Manager + bağlantılar
            var uiMgr = canvasGo.AddComponent<AchievementUIManager>();
            var soUi = new SerializedObject(uiMgr);
            soUi.FindProperty("openPanelButton").objectReferenceValue = btnButton;
            soUi.FindProperty("buttonCounterText").objectReferenceValue = btnCounter;
            soUi.FindProperty("listPanel").objectReferenceValue = listPanel;
            soUi.FindProperty("closePanelButton").objectReferenceValue = closeBtn;
            soUi.FindProperty("listContent").objectReferenceValue = contentRt;
            soUi.FindProperty("headerText").objectReferenceValue = header;
            soUi.FindProperty("toastRoot").objectReferenceValue = toastRt;
            soUi.FindProperty("toastIconText").objectReferenceValue = toastIcon;
            soUi.FindProperty("toastTitleText").objectReferenceValue = toastTitle;
            soUi.FindProperty("toastDescriptionText").objectReferenceValue = toastDesc;
            soUi.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[AchievementSetup] Achievement System sahneye eklendi. Aktif sahne kaydedildi.");
        }

        // ---------- helpers ----------
        private static GameObject CreateUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI");
            return go;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, FontStyles style, TextAlignmentOptions align, Color color)
        {
            var go = CreateUI(name, parent);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.fontStyle = style;
            t.alignment = align;
            t.color = color;
            t.enableWordWrapping = true;
            return t;
        }
    }
}
