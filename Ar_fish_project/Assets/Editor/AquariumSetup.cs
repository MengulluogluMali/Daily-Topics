using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ARFishQuiz;

namespace ARFishQuizEditor
{
    /// <summary>
    /// Sahnede Akvaryum çizim ve görüntüleme sistemini kurar.
    /// Menü: Tools/Aquarium/Setup In Scene
    /// </summary>
    public static class AquariumSetup
    {
        private const string ROOT_NAME = "AquariumSystem";
        private const string DRAW_CANVAS_NAME = "AquariumDrawCanvas";
        private const string OVERLAY_CANVAS_NAME = "AquariumOverlayCanvas";

        [MenuItem("Tools/Aquarium/Setup In Scene")]
        public static void Setup()
        {
            EnsureEventSystem();

            GameObject root = GameObject.Find(ROOT_NAME);
            if (root == null) root = new GameObject(ROOT_NAME);

            // 1) Drawing system
            var drawingMgrGO = root.transform.Find("DrawingManager")?.gameObject;
            if (drawingMgrGO == null)
            {
                drawingMgrGO = new GameObject("DrawingManager");
                drawingMgrGO.transform.SetParent(root.transform, false);
            }
            var drawMgr = drawingMgrGO.GetComponent<AquariumDrawingManager>();
            if (drawMgr == null) drawMgr = drawingMgrGO.AddComponent<AquariumDrawingManager>();

            var drawCanvas = BuildDrawCanvas(drawingMgrGO.transform);
            WireDrawingManager(drawMgr, drawCanvas);

            // 2) Overlay (top-right Akvaryum button + Viewer)
            var viewerMgrGO = root.transform.Find("ViewerManager")?.gameObject;
            if (viewerMgrGO == null)
            {
                viewerMgrGO = new GameObject("ViewerManager");
                viewerMgrGO.transform.SetParent(root.transform, false);
            }
            var viewerMgr = viewerMgrGO.GetComponent<AquariumViewerManager>();
            if (viewerMgr == null) viewerMgr = viewerMgrGO.AddComponent<AquariumViewerManager>();

            var overlayCanvas = BuildOverlayCanvas(viewerMgrGO.transform);
            WireViewerManager(viewerMgr, overlayCanvas);

            // 3) Wire 3D Akvaryuma_git button (zargana)
            WireAkvaryumaGitButton();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[AquariumSetup] Akvaryum sistemi kuruldu.");
            Selection.activeGameObject = root;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        // ---------------- Draw Canvas ----------------

        private static GameObject BuildDrawCanvas(Transform parent)
        {
            var existing = parent.Find(DRAW_CANVAS_NAME);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var canvasGO = new GameObject(DRAW_CANVAS_NAME, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(parent, false);
            canvasGO.layer = LayerMask.NameToLayer("UI");
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000; // her şeyin üstünde
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // mobil portre referansı
            scaler.matchWidthOrHeight = 0.5f;

            // Root panel (dim background + everything) - tam opak ki AR kameraya gerek olmasın
            var rootPanel = CreateUI("RootPanel", canvasGO.transform);
            var rpRT = rootPanel.GetComponent<RectTransform>();
            Stretch(rpRT);
            var rpImg = rootPanel.AddComponent<Image>();
            rpImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            rpImg.raycastTarget = true; // arkaya tıklamayı geçirme

            // Drawing surface (white canvas)
            var surfaceGO = CreateUI("DrawingSurface", rootPanel.transform);
            var surfaceRT = surfaceGO.GetComponent<RectTransform>();
            surfaceRT.anchorMin = new Vector2(0.04f, 0.24f);
            surfaceRT.anchorMax = new Vector2(0.96f, 0.92f);
            surfaceRT.offsetMin = Vector2.zero;
            surfaceRT.offsetMax = Vector2.zero;
            var rawImg = surfaceGO.AddComponent<RawImage>();
            rawImg.color = Color.white;
            rawImg.raycastTarget = true;

            // Title
            var title = CreateUI("Title", rootPanel.transform);
            var titleRT = title.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0f, 0.93f);
            titleRT.anchorMax = new Vector2(1f, 1f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;
            var titleTxt = title.AddComponent<Text>();
            titleTxt.text = "Akvaryumunu Çiz";
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.fontSize = 36;
            titleTxt.color = Color.white;
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Bottom toolbar — 2 satırlı (mobil portreye sığsın)
            var toolbar = CreateUI("Toolbar", rootPanel.transform);
            var tbRT = toolbar.GetComponent<RectTransform>();
            tbRT.anchorMin = new Vector2(0f, 0f);
            tbRT.anchorMax = new Vector2(1f, 0.22f);
            tbRT.offsetMin = Vector2.zero;
            tbRT.offsetMax = Vector2.zero;
            var tbBg = toolbar.AddComponent<Image>();
            tbBg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

            var vlg = toolbar.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.padding = new RectOffset(10, 10, 8, 8);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = true;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            // Row1: renkler + silgi
            var row1GO = CreateUI("Row1", toolbar.transform);
            var row1H = row1GO.AddComponent<HorizontalLayoutGroup>();
            row1H.spacing = 6f;
            row1H.padding = new RectOffset(4, 4, 0, 0);
            row1H.childAlignment = TextAnchor.MiddleCenter;
            row1H.childForceExpandWidth = false;
            row1H.childForceExpandHeight = true;
            row1H.childControlWidth = true;
            row1H.childControlHeight = true;
            var row1LE = row1GO.AddComponent<LayoutElement>();
            row1LE.preferredHeight = 110;
            row1LE.flexibleHeight = 1;
            row1LE.flexibleWidth = 1;

            // Row2: brushSlider + colorPreview + clear + exit + saveExit
            var row2GO = CreateUI("Row2", toolbar.transform);
            var row2H = row2GO.AddComponent<HorizontalLayoutGroup>();
            row2H.spacing = 6f;
            row2H.padding = new RectOffset(4, 4, 0, 0);
            row2H.childAlignment = TextAnchor.MiddleCenter;
            row2H.childForceExpandWidth = false;
            row2H.childForceExpandHeight = true;
            row2H.childControlWidth = true;
            row2H.childControlHeight = true;
            var row2LE = row2GO.AddComponent<LayoutElement>();
            row2LE.preferredHeight = 110;
            row2LE.flexibleHeight = 1;
            row2LE.flexibleWidth = 1;

            // color buttons
            Color[] colors = new Color[]
            {
                Color.black,
                new Color(0.85f,0.15f,0.15f),
                new Color(0.15f,0.7f,0.2f),
                new Color(0.15f,0.35f,0.9f),
                new Color(1f,0.85f,0.1f),
                new Color(1f,0.55f,0.05f),
                new Color(0.55f,0.2f,0.7f),
                new Color(0.15f,0.7f,0.85f),
            };
            string[] colorNames = { "Black","Red","Green","Blue","Yellow","Orange","Purple","Cyan" };
            string[] colorMethods = { "OnClickColorBlack","OnClickColorRed","OnClickColorGreen","OnClickColorBlue","OnClickColorYellow","OnClickColorOrange","OnClickColorPurple","OnClickColorCyan" };

            var colorButtons = new Button[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                var btn = CreateButton(row1GO.transform, colorNames[i], "", colors[i]);
                var le = btn.gameObject.AddComponent<LayoutElement>();
                le.minWidth = 50; le.preferredWidth = 100; le.flexibleWidth = 1;
                le.minHeight = 80; le.preferredHeight = 100; le.flexibleHeight = 1;
                colorButtons[i] = btn;
            }

            // Eraser (Row1 sonu)
            var eraser = CreateButton(row1GO.transform, "Eraser", "Silgi", Color.white);
            eraser.GetComponentInChildren<Text>().color = Color.black;
            var leE = eraser.gameObject.AddComponent<LayoutElement>();
            leE.minWidth = 70; leE.preferredWidth = 120; leE.flexibleWidth = 1;
            leE.minHeight = 80; leE.preferredHeight = 100; leE.flexibleHeight = 1;

            // Brush size slider (Row2)
            var sliderGO = CreateUI("BrushSlider", row2GO.transform);
            var leS = sliderGO.AddComponent<LayoutElement>();
            leS.minWidth = 180; leS.preferredWidth = 220; leS.flexibleWidth = 1;
            leS.minHeight = 80; leS.preferredHeight = 100;
            var slider = BuildSlider(sliderGO);

            // Color preview (Row2)
            var preview = CreateUI("ColorPreview", row2GO.transform);
            var lp = preview.AddComponent<LayoutElement>();
            lp.minWidth = 70; lp.preferredWidth = 80; lp.flexibleWidth = 0;
            lp.minHeight = 80; lp.preferredHeight = 100;
            var prevImg = preview.AddComponent<Image>();
            prevImg.color = Color.black;

            // Clear button (Row2)
            var clear = CreateButton(row2GO.transform, "Clear", "Temizle", new Color(0.4f,0.4f,0.4f));
            var leC = clear.gameObject.AddComponent<LayoutElement>();
            leC.minWidth = 130; leC.preferredWidth = 150; leC.flexibleWidth = 0;
            leC.minHeight = 80; leC.preferredHeight = 100;

            // Exit (no save) (Row2)
            var exitBtn = CreateButton(row2GO.transform, "Exit", "Çıkış", new Color(0.7f,0.2f,0.2f));
            var leX = exitBtn.gameObject.AddComponent<LayoutElement>();
            leX.minWidth = 130; leX.preferredWidth = 150; leX.flexibleWidth = 0;
            leX.minHeight = 80; leX.preferredHeight = 100;

            // Save & Exit (Row2)
            var saveBtn = CreateButton(row2GO.transform, "SaveExit", "Kaydet ve Çık", new Color(0.2f,0.6f,0.25f));
            var leSv = saveBtn.gameObject.AddComponent<LayoutElement>();
            leSv.minWidth = 200; leSv.preferredWidth = 230; leSv.flexibleWidth = 0;
            leSv.minHeight = 80; leSv.preferredHeight = 100;

            // Save references for wiring
            DrawCanvasRefs.Last = new DrawCanvasRefs
            {
                canvas = canvasGO.GetComponent<Canvas>(),
                rootPanel = rootPanel,
                drawingSurface = rawImg,
                drawingArea = surfaceRT,
                colorPreview = prevImg,
                brushSlider = slider,
                btnBlack = colorButtons[0],
                btnRed = colorButtons[1],
                btnGreen = colorButtons[2],
                btnBlue = colorButtons[3],
                btnYellow = colorButtons[4],
                btnOrange = colorButtons[5],
                btnPurple = colorButtons[6],
                btnCyan = colorButtons[7],
                btnEraser = eraser,
                btnClear = clear,
                btnExit = exitBtn,
                btnSaveExit = saveBtn
            };

            return canvasGO;
        }

        private struct DrawCanvasRefs
        {
            public Canvas canvas;
            public GameObject rootPanel;
            public RawImage drawingSurface;
            public RectTransform drawingArea;
            public Image colorPreview;
            public Slider brushSlider;
            public Button btnBlack, btnRed, btnGreen, btnBlue, btnYellow, btnOrange, btnPurple, btnCyan;
            public Button btnEraser, btnClear, btnExit, btnSaveExit;
            public static DrawCanvasRefs Last;
        }

        private static void WireDrawingManager(AquariumDrawingManager mgr, GameObject canvasGO)
        {
            var refs = DrawCanvasRefs.Last;
            var so = new SerializedObject(mgr);
            so.FindProperty("drawingCanvas").objectReferenceValue = refs.canvas;
            so.FindProperty("drawingSurface").objectReferenceValue = refs.drawingSurface;
            so.FindProperty("drawingArea").objectReferenceValue = refs.drawingArea;
            so.FindProperty("colorPreview").objectReferenceValue = refs.colorPreview;
            so.FindProperty("brushSizeSlider").objectReferenceValue = refs.brushSlider;
            so.FindProperty("rootPanel").objectReferenceValue = refs.rootPanel;
            so.FindProperty("btnBlack").objectReferenceValue = refs.btnBlack;
            so.FindProperty("btnRed").objectReferenceValue = refs.btnRed;
            so.FindProperty("btnGreen").objectReferenceValue = refs.btnGreen;
            so.FindProperty("btnBlue").objectReferenceValue = refs.btnBlue;
            so.FindProperty("btnYellow").objectReferenceValue = refs.btnYellow;
            so.FindProperty("btnOrange").objectReferenceValue = refs.btnOrange;
            so.FindProperty("btnPurple").objectReferenceValue = refs.btnPurple;
            so.FindProperty("btnCyan").objectReferenceValue = refs.btnCyan;
            so.FindProperty("btnEraser").objectReferenceValue = refs.btnEraser;
            so.FindProperty("btnClear").objectReferenceValue = refs.btnClear;
            so.FindProperty("btnExit").objectReferenceValue = refs.btnExit;
            so.FindProperty("btnSaveExit").objectReferenceValue = refs.btnSaveExit;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void InvokeMgr(AquariumDrawingManager mgr, string methodName)
        {
            mgr.GetType().GetMethod(methodName)?.Invoke(mgr, null);
        }

        // ---------------- Overlay Canvas ----------------

        private static GameObject BuildOverlayCanvas(Transform parent)
        {
            var existing = parent.Find(OVERLAY_CANVAS_NAME);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var canvasGO = new GameObject(OVERLAY_CANVAS_NAME, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(parent, false);
            canvasGO.layer = LayerMask.NameToLayer("UI");
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 31000; // çizim canvas'ından düşük (32000) ama her şeyden yüksek
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // mobil portre referansı
            scaler.matchWidthOrHeight = 0.5f;

            // Top-right Akvaryum button (always visible) — mobil için büyük
            var openBtnGO = CreateUI("AkvaryumButton", canvasGO.transform);
            var rt = openBtnGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-25f, -25f);
            rt.sizeDelta = new Vector2(280f, 130f);
            var img = openBtnGO.AddComponent<Image>();
            img.color = new Color(0.1f, 0.55f, 0.85f, 0.98f);
            var btn = openBtnGO.AddComponent<Button>();
            btn.targetGraphic = img;
            // Outline ekleyelim ki AR arka planında bile gözüksün
            var outline = openBtnGO.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(3, -3);

            var labelGO = CreateUI("Label", openBtnGO.transform);
            var lrt = labelGO.GetComponent<RectTransform>();
            Stretch(lrt);
            var lbl = labelGO.AddComponent<Text>();
            lbl.text = "AKVARYUM";
            lbl.alignment = TextAnchor.MiddleCenter;
            lbl.color = Color.white;
            lbl.fontSize = 44;
            lbl.fontStyle = FontStyle.Bold;
            lbl.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Viewer panel (hidden by default)
            var viewer = CreateUI("ViewerPanel", canvasGO.transform);
            var vrt = viewer.GetComponent<RectTransform>();
            Stretch(vrt);
            var vbg = viewer.AddComponent<Image>();
            vbg.color = new Color(0f, 0f, 0f, 0.85f);

            // Title
            var vTitleGO = CreateUI("Title", viewer.transform);
            var vTitleRT = vTitleGO.GetComponent<RectTransform>();
            vTitleRT.anchorMin = new Vector2(0f, 0.9f);
            vTitleRT.anchorMax = new Vector2(1f, 1f);
            vTitleRT.offsetMin = Vector2.zero; vTitleRT.offsetMax = Vector2.zero;
            var vTitle = vTitleGO.AddComponent<Text>();
            vTitle.alignment = TextAnchor.MiddleCenter;
            vTitle.fontSize = 42;
            vTitle.color = Color.white;
            vTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            vTitle.text = "Akvaryum";

            // Image
            var vImgGO = CreateUI("ViewerImage", viewer.transform);
            var vImgRT = vImgGO.GetComponent<RectTransform>();
            vImgRT.anchorMin = new Vector2(0.15f, 0.18f);
            vImgRT.anchorMax = new Vector2(0.85f, 0.88f);
            vImgRT.offsetMin = Vector2.zero; vImgRT.offsetMax = Vector2.zero;
            var vRaw = vImgGO.AddComponent<RawImage>();
            vRaw.color = Color.white;

            // Counter
            var counterGO = CreateUI("Counter", viewer.transform);
            var crt = counterGO.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 0.10f);
            crt.anchorMax = new Vector2(1f, 0.16f);
            crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;
            var counterTxt = counterGO.AddComponent<Text>();
            counterTxt.alignment = TextAnchor.MiddleCenter;
            counterTxt.color = Color.white;
            counterTxt.fontSize = 28;
            counterTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            counterTxt.text = "1 / 1";

            // Prev / Next / Close
            var prevBtn = CreateButton(viewer.transform, "Prev", "<", new Color(0.2f,0.4f,0.7f));
            var prRT = prevBtn.GetComponent<RectTransform>();
            prRT.anchorMin = new Vector2(0f, 0.45f);
            prRT.anchorMax = new Vector2(0f, 0.55f);
            prRT.pivot = new Vector2(0f, 0.5f);
            prRT.anchoredPosition = new Vector2(20f, 0f);
            prRT.sizeDelta = new Vector2(80f, 80f);

            var nextBtn = CreateButton(viewer.transform, "Next", ">", new Color(0.2f,0.4f,0.7f));
            var nxRT = nextBtn.GetComponent<RectTransform>();
            nxRT.anchorMin = new Vector2(1f, 0.45f);
            nxRT.anchorMax = new Vector2(1f, 0.55f);
            nxRT.pivot = new Vector2(1f, 0.5f);
            nxRT.anchoredPosition = new Vector2(-20f, 0f);
            nxRT.sizeDelta = new Vector2(80f, 80f);

            var closeBtn = CreateButton(viewer.transform, "Close", "Kapat", new Color(0.7f,0.2f,0.2f));
            var clRT = closeBtn.GetComponent<RectTransform>();
            clRT.anchorMin = new Vector2(0.5f, 0f);
            clRT.anchorMax = new Vector2(0.5f, 0f);
            clRT.pivot = new Vector2(0.5f, 0f);
            clRT.anchoredPosition = new Vector2(0f, 20f);
            clRT.sizeDelta = new Vector2(220f, 70f);

            // Empty state
            var empty = CreateUI("EmptyState", viewer.transform);
            var ert = empty.GetComponent<RectTransform>();
            ert.anchorMin = new Vector2(0.2f, 0.4f);
            ert.anchorMax = new Vector2(0.8f, 0.6f);
            ert.offsetMin = Vector2.zero; ert.offsetMax = Vector2.zero;
            var emptyTxt = empty.AddComponent<Text>();
            emptyTxt.alignment = TextAnchor.MiddleCenter;
            emptyTxt.color = Color.white;
            emptyTxt.fontSize = 36;
            emptyTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            emptyTxt.text = "Henüz akvaryum çizmediniz.\nBir balığın akvaryum çiz butonuna basın!";
            empty.SetActive(false);

            viewer.SetActive(false);

            OverlayRefs.Last = new OverlayRefs
            {
                canvas = canvas,
                openBtn = btn,
                viewerPanel = viewer,
                viewerImage = vRaw,
                viewerTitle = vTitle,
                counter = counterTxt,
                prev = prevBtn,
                next = nextBtn,
                close = closeBtn,
                empty = empty
            };

            return canvasGO;
        }

        private struct OverlayRefs
        {
            public Canvas canvas;
            public Button openBtn;
            public GameObject viewerPanel;
            public RawImage viewerImage;
            public Text viewerTitle;
            public Text counter;
            public Button prev;
            public Button next;
            public Button close;
            public GameObject empty;
            public static OverlayRefs Last;
        }

        private static void WireViewerManager(AquariumViewerManager mgr, GameObject canvasGO)
        {
            var refs = OverlayRefs.Last;
            var so = new SerializedObject(mgr);
            so.FindProperty("overlayCanvas").objectReferenceValue = refs.canvas;
            so.FindProperty("openViewerButton").objectReferenceValue = refs.openBtn;
            so.FindProperty("viewerPanel").objectReferenceValue = refs.viewerPanel;
            so.FindProperty("viewerImage").objectReferenceValue = refs.viewerImage;
            so.FindProperty("viewerTitle").objectReferenceValue = refs.viewerTitle;
            so.FindProperty("viewerCounterText").objectReferenceValue = refs.counter;
            so.FindProperty("viewerCloseButton").objectReferenceValue = refs.close;
            so.FindProperty("viewerNextButton").objectReferenceValue = refs.next;
            so.FindProperty("viewerPrevButton").objectReferenceValue = refs.prev;
            so.FindProperty("emptyStatePanel").objectReferenceValue = refs.empty;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------------- Wire 3D Akvaryuma_git ----------------

        private static void WireAkvaryumaGitButton()
        {
            // Find all "Akvaryuma_git" objects under any *_target
            var allGOs = Object.FindObjectsOfType<GameObject>(true);
            foreach (var go in allGOs)
            {
                if (go.name != "Akvaryuma_git") continue;

                // Determine the target parent (top-most named *_target)
                Transform t = go.transform;
                string fishId = null;
                while (t != null)
                {
                    if (t.name.EndsWith("_target") || t.name == "kalkan_baligi")
                    {
                        fishId = t.name;
                        break;
                    }
                    t = t.parent;
                }
                if (string.IsNullOrEmpty(fishId)) fishId = go.transform.parent != null ? go.transform.parent.name : go.name;

                // Remove InfoButton3D if exists (this object should be the AQUARIUM button, not info)
                var infoBtn = go.GetComponent<InfoButton3D>();
                if (infoBtn != null) Object.DestroyImmediate(infoBtn);

                // Ensure collider
                if (go.GetComponent<Collider>() == null)
                    go.AddComponent<BoxCollider>();

                // Add or get AquariumButton3D
                var aqBtn = go.GetComponent<AquariumButton3D>();
                if (aqBtn == null) aqBtn = go.AddComponent<AquariumButton3D>();

                var so = new SerializedObject(aqBtn);
                so.FindProperty("fishId").stringValue = fishId;
                var camProp = so.FindProperty("raycastCamera");
                var arCam = GameObject.Find("ARCamera");
                if (arCam != null)
                {
                    var cam = arCam.GetComponent<Camera>();
                    if (cam != null) camProp.objectReferenceValue = cam;
                }
                so.ApplyModifiedPropertiesWithoutUndo();

                Debug.Log($"[AquariumSetup] Wired Akvaryuma_git on '{go.transform.parent?.name}/{go.name}' with fishId={fishId}");
            }
        }

        // ---------------- Helpers ----------------

        private static GameObject CreateUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Button CreateButton(Transform parent, string name, string label, Color bg)
        {
            var go = CreateUI(name, parent);
            var img = go.AddComponent<Image>();
            img.color = bg;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            if (!string.IsNullOrEmpty(label))
            {
                var lblGO = CreateUI("Label", go.transform);
                Stretch(lblGO.GetComponent<RectTransform>());
                var txt = lblGO.AddComponent<Text>();
                txt.text = label;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = Color.white;
                txt.fontSize = 24;
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            return btn;
        }

        private static Slider BuildSlider(GameObject sliderGO)
        {
            // Simple horizontal slider
            var rt = sliderGO.GetComponent<RectTransform>();
            var slider = sliderGO.AddComponent<Slider>();

            var bg = CreateUI("Background", sliderGO.transform);
            var bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0, 0.4f);
            bgRT.anchorMax = new Vector2(1, 0.6f);
            bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.4f,0.4f,0.4f);

            var fillArea = CreateUI("Fill Area", sliderGO.transform);
            var faRT = fillArea.GetComponent<RectTransform>();
            faRT.anchorMin = new Vector2(0, 0.4f);
            faRT.anchorMax = new Vector2(1, 0.6f);
            faRT.offsetMin = new Vector2(5, 0); faRT.offsetMax = new Vector2(-15, 0);

            var fill = CreateUI("Fill", fillArea.transform);
            var fillRT = fill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero; fillRT.offsetMax = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.2f,0.6f,0.95f);

            var handleArea = CreateUI("Handle Slide Area", sliderGO.transform);
            var haRT = handleArea.GetComponent<RectTransform>();
            haRT.anchorMin = new Vector2(0, 0); haRT.anchorMax = new Vector2(1, 1);
            haRT.offsetMin = new Vector2(10, 0); haRT.offsetMax = new Vector2(-10, 0);

            var handle = CreateUI("Handle", handleArea.transform);
            var hRT = handle.GetComponent<RectTransform>();
            hRT.sizeDelta = new Vector2(24, 40);
            var hImg = handle.AddComponent<Image>();
            hImg.color = Color.white;

            slider.fillRect = fillRT;
            slider.handleRect = hRT;
            slider.targetGraphic = hImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 1f;
            slider.maxValue = 40f;
            slider.value = 8f;
            slider.wholeNumbers = true;

            return slider;
        }
    }
}
