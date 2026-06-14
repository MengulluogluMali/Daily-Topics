using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ARFishQuiz
{
    /// <summary>
    /// Tüm ekranı kaplayan beyaz tuval üstünde renkli çizim yapma paneli.
    /// Akvaryum çiz butonuna basıldığında açılır, çıkış / kaydet ile kapatılır.
    /// Her balık için ayrı PNG dosyası saklar (persistentDataPath/Aquarium/{fishId}.png).
    /// </summary>
    public class AquariumDrawingManager : MonoBehaviour
    {
        public static AquariumDrawingManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Canvas drawingCanvas;
        [SerializeField] private RawImage drawingSurface;       // beyaz tuval (RawImage)
        [SerializeField] private RectTransform drawingArea;     // tıklama alanı (RawImage rect)
        [SerializeField] private Image colorPreview;            // seçili renk önizlemesi
        [SerializeField] private Slider brushSizeSlider;        // 1..40
        [SerializeField] private GameObject rootPanel;          // tüm panel kapsayıcısı (aktif/pasif)

        [Header("Toolbar Buttons (otomatik bağlanır)")]
        [SerializeField] private Button btnBlack;
        [SerializeField] private Button btnRed;
        [SerializeField] private Button btnGreen;
        [SerializeField] private Button btnBlue;
        [SerializeField] private Button btnYellow;
        [SerializeField] private Button btnOrange;
        [SerializeField] private Button btnPurple;
        [SerializeField] private Button btnCyan;
        [SerializeField] private Button btnEraser;
        [SerializeField] private Button btnClear;
        [SerializeField] private Button btnExit;
        [SerializeField] private Button btnSaveExit;

        [Header("Settings")]
        [SerializeField] private int textureWidth = 1024;
        [SerializeField] private int textureHeight = 1024;
        [SerializeField] private Color backgroundColor = Color.white;
        [SerializeField] private int defaultBrushSize = 8;

        // Mevcut durum
        private Texture2D drawTexture;
        private Color currentColor = Color.black;
        private int currentBrushSize = 8;
        private bool isErasing = false;

        // Drag tracking
        private bool isDragging = false;
        private Vector2 lastPixelPos;
        private bool hasLastPos = false;

        private string currentFishId;
        private System.Action onSavedCallback;
        private System.Action onClosedCallback;

        // Pixel buffer for fast updates
        private Color32[] pixelBuffer;

        private bool _drawingNotified = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            DontDestroyOnLoad(gameObject);

            currentBrushSize = defaultBrushSize;
            if (brushSizeSlider != null)
            {
                brushSizeSlider.minValue = 1;
                brushSizeSlider.maxValue = 40;
                brushSizeSlider.value = currentBrushSize;
                brushSizeSlider.onValueChanged.AddListener(OnBrushSizeChanged);
            }

            InitTexture();
            UpdateColorPreview();
            BindButtons();

            if (rootPanel != null)
                rootPanel.SetActive(false);
        }

        private void BindButtons()
        {
            // Sahne save'inde lambda listener'lar kaybolduğu için runtime'da bağlıyoruz.
            BindBtn(btnBlack,   OnClickColorBlack);
            BindBtn(btnRed,     OnClickColorRed);
            BindBtn(btnGreen,   OnClickColorGreen);
            BindBtn(btnBlue,    OnClickColorBlue);
            BindBtn(btnYellow,  OnClickColorYellow);
            BindBtn(btnOrange,  OnClickColorOrange);
            BindBtn(btnPurple,  OnClickColorPurple);
            BindBtn(btnCyan,    OnClickColorCyan);
            BindBtn(btnEraser,  OnClickEraser);
            BindBtn(btnClear,   OnClickClear);
            BindBtn(btnExit,    OnClickExitWithoutSave);
            BindBtn(btnSaveExit,OnClickSaveAndExit);
        }

        private static void BindBtn(Button b, UnityEngine.Events.UnityAction action)
        {
            if (b == null) return;
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(action);
        }

        private void InitTexture()
        {
            drawTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
            drawTexture.filterMode = FilterMode.Bilinear;
            drawTexture.wrapMode = TextureWrapMode.Clamp;
            pixelBuffer = new Color32[textureWidth * textureHeight];
            ClearTexture(false);
            if (drawingSurface != null)
                drawingSurface.texture = drawTexture;
        }

        private void ClearTexture(bool apply = true)
        {
            Color32 c = backgroundColor;
            for (int i = 0; i < pixelBuffer.Length; i++)
                pixelBuffer[i] = c;
            drawTexture.SetPixels32(pixelBuffer);
            if (apply) drawTexture.Apply();
        }

        // ---------------- Public API ----------------

        public void OpenForFish(string fishId, System.Action onSaved = null, System.Action onClosed = null)
        {
            currentFishId = fishId;
            onSavedCallback = onSaved;
            onClosedCallback = onClosed;

            // Önce mevcut çizim varsa yükle, yoksa beyaz tuval
            if (!LoadExistingForFish(fishId))
                ClearTexture();

            if (rootPanel != null) rootPanel.SetActive(true);
            if (drawingCanvas != null) drawingCanvas.gameObject.SetActive(true);

            // AR kamerayı kapatmıyoruz; çizim canvas'ı tam opak ve sortingOrder yüksek,
            // kullanıcı zaten arka planı görmez, AR session bozulmasın.

            isDragging = false;
            hasLastPos = false;
            _drawingNotified = false;

            // Başarım sistemi: çizim panelinin açıldığını bildir (drawing_count için)
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.NotifyDrawingCreated(currentFishId);
                _drawingNotified = true;
            }
        }

        public void OnClickSaveAndExit()
        {
            SaveCurrentToFish(currentFishId);

            // Başarım sistemi: çizim kaydedildi bildir
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.NotifyDrawingSaved(currentFishId);
            }

            onSavedCallback?.Invoke();
            CloseInternal();
        }

        public void OnClickExitWithoutSave()
        {
            CloseInternal();
        }

        public void OnClickClear()
        {
            ClearTexture();
        }

        public void OnClickColorRed()    => SetColor(new Color(0.85f, 0.15f, 0.15f), false);
        public void OnClickColorGreen()  => SetColor(new Color(0.15f, 0.7f, 0.2f), false);
        public void OnClickColorBlue()   => SetColor(new Color(0.15f, 0.35f, 0.9f), false);
        public void OnClickColorYellow() => SetColor(new Color(1f, 0.85f, 0.1f), false);
        public void OnClickColorBlack()  => SetColor(Color.black, false);
        public void OnClickColorOrange() => SetColor(new Color(1f, 0.55f, 0.05f), false);
        public void OnClickColorPurple() => SetColor(new Color(0.55f, 0.2f, 0.7f), false);
        public void OnClickColorCyan()   => SetColor(new Color(0.15f, 0.7f, 0.85f), false);
        public void OnClickEraser()      => SetColor(backgroundColor, true);

        // ---------------- Internals ----------------

        private void SetColor(Color c, bool eraser)
        {
            currentColor = c;
            isErasing = eraser;
            UpdateColorPreview();
        }

        private void UpdateColorPreview()
        {
            if (colorPreview != null)
                colorPreview.color = isErasing ? Color.white : currentColor;
        }

        private void OnBrushSizeChanged(float v)
        {
            currentBrushSize = Mathf.Max(1, Mathf.RoundToInt(v));
        }

        private void Update()
        {
            if (rootPanel == null || !rootPanel.activeInHierarchy) return;
            HandleDrawingInput();
        }

        private void HandleDrawingInput()
        {
            bool pressed = false;
            bool held = false;
            bool released = false;
            Vector2 screenPos = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                pressed  = Mouse.current.leftButton.wasPressedThisFrame;
                held     = Mouse.current.leftButton.isPressed;
                released = Mouse.current.leftButton.wasReleasedThisFrame;
                screenPos = Mouse.current.position.ReadValue();
            }
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame) pressed = true;
                held = true;
                screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
            {
                released = true;
            }
#else
            pressed  = Input.GetMouseButtonDown(0);
            held     = Input.GetMouseButton(0);
            released = Input.GetMouseButtonUp(0);
            screenPos = Input.mousePosition;
            if (Input.touchCount > 0)
            {
                var t = Input.GetTouch(0);
                screenPos = t.position;
                if (t.phase == TouchPhase.Began) pressed = true;
                if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) held = true;
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) released = true;
            }
#endif

            if (released)
            {
                isDragging = false;
                hasLastPos = false;
                return;
            }

            if (!held && !pressed) return;

            // UI üstünde miyiz? (butonlara basarken çizmeyelim)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() && !isDragging)
            {
                // Eğer drag başlamadıysa ve UI üstündeyse çizme
                if (!IsPointerOverDrawingArea(screenPos))
                    return;
            }

            if (drawingArea == null) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    drawingArea, screenPos,
                    drawingCanvas != null ? drawingCanvas.worldCamera : null,
                    out Vector2 localPoint))
                return;

            Rect r = drawingArea.rect;
            if (localPoint.x < r.xMin || localPoint.x > r.xMax ||
                localPoint.y < r.yMin || localPoint.y > r.yMax)
            {
                if (!isDragging) return;
            }

            float u = (localPoint.x - r.xMin) / r.width;
            float v = (localPoint.y - r.yMin) / r.height;
            int px = Mathf.Clamp(Mathf.RoundToInt(u * textureWidth), 0, textureWidth - 1);
            int py = Mathf.Clamp(Mathf.RoundToInt(v * textureHeight), 0, textureHeight - 1);
            Vector2 pixelPos = new Vector2(px, py);

            if (pressed)
            {
                isDragging = true;
                hasLastPos = false;
            }

            if (isDragging)
            {
                if (hasLastPos)
                    DrawLine(lastPixelPos, pixelPos, currentBrushSize, currentColor);
                else
                    DrawCircle(px, py, currentBrushSize, currentColor);

                drawTexture.SetPixels32(pixelBuffer);
                drawTexture.Apply();

                lastPixelPos = pixelPos;
                hasLastPos = true;
            }
        }

        private bool IsPointerOverDrawingArea(Vector2 screenPos)
        {
            if (drawingArea == null) return false;
            return RectTransformUtility.RectangleContainsScreenPoint(
                drawingArea, screenPos,
                drawingCanvas != null ? drawingCanvas.worldCamera : null);
        }

        private void DrawCircle(int cx, int cy, int radius, Color color)
        {
            int r2 = radius * radius;
            int xMin = Mathf.Max(0, cx - radius);
            int xMax = Mathf.Min(textureWidth - 1, cx + radius);
            int yMin = Mathf.Max(0, cy - radius);
            int yMax = Mathf.Min(textureHeight - 1, cy + radius);
            Color32 c32 = color;

            for (int y = yMin; y <= yMax; y++)
            {
                int dy = y - cy;
                int rowOffset = y * textureWidth;
                for (int x = xMin; x <= xMax; x++)
                {
                    int dx = x - cx;
                    if (dx * dx + dy * dy <= r2)
                    {
                        pixelBuffer[rowOffset + x] = c32;
                    }
                }
            }
        }

        private void DrawLine(Vector2 a, Vector2 b, int radius, Color color)
        {
            float dist = Vector2.Distance(a, b);
            int steps = Mathf.Max(1, Mathf.CeilToInt(dist));
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(a.x, b.x, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(a.y, b.y, t));
                DrawCircle(x, y, radius, color);
            }
        }

        // ---------------- Save / Load ----------------

        private static string GetSaveFolder()
        {
            string folder = Path.Combine(Application.persistentDataPath, "Aquarium");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            return folder;
        }

        public static string GetSavePath(string fishId)
        {
            return Path.Combine(GetSaveFolder(), Sanitize(fishId) + ".png");
        }

        private static string Sanitize(string s)
        {
            if (string.IsNullOrEmpty(s)) return "default";
            foreach (var c in Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            return s;
        }

        private void SaveCurrentToFish(string fishId)
        {
            if (string.IsNullOrEmpty(fishId)) fishId = "default";
            byte[] png = drawTexture.EncodeToPNG();
            string path = GetSavePath(fishId);
            File.WriteAllBytes(path, png);
            Debug.Log($"[AquariumDrawingManager] Akvaryum kaydedildi: {path}");
        }

        private bool LoadExistingForFish(string fishId)
        {
            if (string.IsNullOrEmpty(fishId)) return false;
            string path = GetSavePath(fishId);
            if (!File.Exists(path)) return false;

            byte[] data = File.ReadAllBytes(path);
            Texture2D loaded = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!loaded.LoadImage(data)) return false;

            // Hedef tuvale ölçekleyerek kopyala
            ClearTexture(false);
            Color32[] src = loaded.GetPixels32();
            int sw = loaded.width, sh = loaded.height;

            // Eğer aynı boyuttaysa direkt kopyala
            if (sw == textureWidth && sh == textureHeight)
            {
                pixelBuffer = src;
            }
            else
            {
                // basit nearest-neighbor scaling
                for (int y = 0; y < textureHeight; y++)
                {
                    int sy = Mathf.Clamp((int)((y / (float)textureHeight) * sh), 0, sh - 1);
                    for (int x = 0; x < textureWidth; x++)
                    {
                        int sx = Mathf.Clamp((int)((x / (float)textureWidth) * sw), 0, sw - 1);
                        pixelBuffer[y * textureWidth + x] = src[sy * sw + sx];
                    }
                }
            }
            drawTexture.SetPixels32(pixelBuffer);
            drawTexture.Apply();
            Destroy(loaded);
            return true;
        }

        private void CloseInternal()
        {
            if (rootPanel != null) rootPanel.SetActive(false);
            onClosedCallback?.Invoke();
        }

        public bool IsOpen => rootPanel != null && rootPanel.activeInHierarchy;
    }
}
