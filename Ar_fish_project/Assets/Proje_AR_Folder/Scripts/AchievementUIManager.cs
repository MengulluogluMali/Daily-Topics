using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ARFishQuiz
{
    /// <summary>
    /// Sahnedeki başarım UI'sini yöneten sınıf.
    /// - Sol üstteki "Başarımlar" butonu (her zaman görünür)
    /// - Tıklanınca açılan kaydırılabilir liste paneli
    /// - Yeni başarım açıldığında ekrandan kayan toast bildirimi
    /// </summary>
    public class AchievementUIManager : MonoBehaviour
    {
        public static AchievementUIManager Instance { get; private set; }

        [Header("Buton (sol üstte sürekli görünen)")]
        [SerializeField] private Button openPanelButton;
        [SerializeField] private TMP_Text buttonCounterText;   // "3 / 12" gibi

        [Header("Liste Paneli")]
        [SerializeField] private GameObject listPanel;
        [SerializeField] private Button closePanelButton;
        [SerializeField] private RectTransform listContent;    // ScrollView->Viewport->Content
        [SerializeField] private GameObject achievementRowPrefab; // runtime'da kullanılmaz, scriptle oluşturulur
        [SerializeField] private TMP_Text headerText;

        [Header("Toast Bildirimi")]
        [SerializeField] private RectTransform toastRoot;      // başlangıçta off-screen sağda
        [SerializeField] private TMP_Text toastIconText;
        [SerializeField] private TMP_Text toastTitleText;
        [SerializeField] private TMP_Text toastDescriptionText;
        [SerializeField] private float toastSlideDuration = 0.4f;
        [SerializeField] private float toastVisibleDuration = 3.0f;

        private readonly Queue<AchievementData> _pendingToasts = new Queue<AchievementData>();
        private bool _toastRunning = false;

        private readonly List<RectTransform> _spawnedRows = new List<RectTransform>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (listPanel != null) listPanel.SetActive(false);
            if (toastRoot != null) toastRoot.gameObject.SetActive(false);

            if (openPanelButton != null)
            {
                openPanelButton.onClick.RemoveAllListeners();
                openPanelButton.onClick.AddListener(OpenList);
            }
            if (closePanelButton != null)
            {
                closePanelButton.onClick.RemoveAllListeners();
                closePanelButton.onClick.AddListener(CloseList);
            }

            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked += HandleUnlocked;
                AchievementManager.Instance.OnProgressChanged += UpdateButtonCounter;
            }

            UpdateButtonCounter();
        }

        private void OnDestroy()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked -= HandleUnlocked;
                AchievementManager.Instance.OnProgressChanged -= UpdateButtonCounter;
            }
        }

        // ---------- Buton sayacı ----------
        private void UpdateButtonCounter()
        {
            if (buttonCounterText == null) return;
            int total = AchievementDatabase.All != null ? AchievementDatabase.All.Count : 0;
            int unlocked = 0;
            if (AchievementManager.Instance != null)
                unlocked = AchievementManager.Instance.UnlockedIds.Count;
            buttonCounterText.text = $"{unlocked}/{total}";
        }

        // ---------- Panel ----------
        public void OpenList()
        {
            if (listPanel == null) return;
            listPanel.SetActive(true);
            RebuildList();
        }

        public void CloseList()
        {
            if (listPanel == null) return;
            listPanel.SetActive(false);
        }

        public void RebuildList()
        {
            if (listContent == null) return;

            // mevcut satırları temizle
            foreach (var rt in _spawnedRows)
                if (rt != null) Destroy(rt.gameObject);
            _spawnedRows.Clear();

            int unlocked = 0;
            int total = 0;
            foreach (var a in AchievementDatabase.All)
            {
                if (a == null) continue;
                total++;
                bool isUnlocked = AchievementManager.Instance != null && AchievementManager.Instance.IsUnlocked(a.achievementId);
                if (isUnlocked) unlocked++;
                var row = CreateRow(a, isUnlocked);
                if (row != null) _spawnedRows.Add(row);
            }

            if (headerText != null)
                headerText.text = $"BAŞARIMLAR  <size=70%><color=#cfeaff>({unlocked}/{total})</color></size>";
        }

        private RectTransform CreateRow(AchievementData a, bool isUnlocked)
        {
            var go = new GameObject("Row_" + a.achievementId, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(listContent, false);
            var bg = go.GetComponent<Image>();
            bg.color = isUnlocked ? new Color(0.12f, 0.30f, 0.20f, 0.92f) : new Color(0.10f, 0.12f, 0.18f, 0.92f);

            var le = go.GetComponent<LayoutElement>();
            le.minHeight = 160f;
            le.preferredHeight = 160f;

            // Outline (kenarlık)
            var outline = go.AddComponent<Outline>();
            outline.effectColor = isUnlocked ? new Color(0.4f, 0.95f, 0.55f, 1f) : new Color(0.3f, 0.4f, 0.55f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);

            // Icon kutusu (kategori rengiyle dolu, içinde rozet işareti)
            var iconBox = new GameObject("IconBox", typeof(RectTransform), typeof(Image));
            iconBox.transform.SetParent(rt, false);
            var iconRt = iconBox.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0f, 0.5f);
            iconRt.anchorMax = new Vector2(0f, 0.5f);
            iconRt.pivot = new Vector2(0f, 0.5f);
            iconRt.anchoredPosition = new Vector2(20f, 0f);
            iconRt.sizeDelta = new Vector2(120f, 120f);
            var iconImg = iconBox.GetComponent<Image>();
            iconImg.color = isUnlocked ? RarityColor(a.rarity) : new Color(0.18f, 0.22f, 0.30f, 1f);
            var iconBoxOl = iconBox.AddComponent<Outline>();
            iconBoxOl.effectColor = isUnlocked ? new Color(1f, 1f, 1f, 0.8f) : new Color(0.4f, 0.5f, 0.65f, 1f);
            iconBoxOl.effectDistance = new Vector2(2f, -2f);

            var iconLabel = new GameObject("Glyph", typeof(RectTransform));
            iconLabel.transform.SetParent(iconBox.transform, false);
            var iconLblRt = iconLabel.GetComponent<RectTransform>();
            iconLblRt.anchorMin = Vector2.zero; iconLblRt.anchorMax = Vector2.one;
            iconLblRt.offsetMin = Vector2.zero; iconLblRt.offsetMax = Vector2.zero;
            var iconText = iconLabel.AddComponent<TextMeshProUGUI>();
            iconText.text = isUnlocked ? "OK" : CategoryGlyph(a.category);
            iconText.alignment = TextAlignmentOptions.Center;
            iconText.fontSize = 70f;
            iconText.fontStyle = FontStyles.Bold;
            iconText.color = isUnlocked ? Color.white : new Color(1f, 1f, 1f, 0.85f);

            // Title
            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(rt, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0f, 1f);
            titleRt.anchoredPosition = new Vector2(160f, -10f);
            titleRt.sizeDelta = new Vector2(-180f, 60f);
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = a.title;
            titleText.fontSize = 38f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = isUnlocked ? new Color(1f, 0.95f, 0.55f) : new Color(0.85f, 0.9f, 1f, 0.85f);
            titleText.enableWordWrapping = false;
            titleText.overflowMode = TextOverflowModes.Ellipsis;

            // Description
            var descGo = new GameObject("Description", typeof(RectTransform));
            descGo.transform.SetParent(rt, false);
            var descRt = descGo.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0f, 0f);
            descRt.anchorMax = new Vector2(1f, 1f);
            descRt.pivot = new Vector2(0f, 0.5f);
            descRt.offsetMin = new Vector2(160f, 50f);
            descRt.offsetMax = new Vector2(-20f, -65f);
            var descText = descGo.AddComponent<TextMeshProUGUI>();
            descText.text = a.description;
            descText.fontSize = 26f;
            descText.color = isUnlocked ? new Color(1f, 1f, 1f, 0.92f) : new Color(1f, 1f, 1f, 0.65f);
            descText.enableWordWrapping = true;

            // Progress bar background
            var pbBg = new GameObject("ProgressBg", typeof(RectTransform), typeof(Image));
            pbBg.transform.SetParent(rt, false);
            var pbBgRt = pbBg.GetComponent<RectTransform>();
            pbBgRt.anchorMin = new Vector2(0f, 0f);
            pbBgRt.anchorMax = new Vector2(1f, 0f);
            pbBgRt.pivot = new Vector2(0.5f, 0f);
            pbBgRt.anchoredPosition = new Vector2(0f, 12f);
            pbBgRt.sizeDelta = new Vector2(-180f, 18f);
            pbBgRt.offsetMin = new Vector2(160f, 12f);
            pbBgRt.offsetMax = new Vector2(-180f, 30f);
            var pbBgImg = pbBg.GetComponent<Image>();
            pbBgImg.color = new Color(0.05f, 0.07f, 0.12f, 0.9f);

            // Progress bar fill
            float pct = 0f;
            string label = "";
            if (AchievementManager.Instance != null) pct = AchievementManager.Instance.GetProgress(a, out label);
            pct = Mathf.Clamp01(pct);

            var pbFill = new GameObject("ProgressFill", typeof(RectTransform), typeof(Image));
            pbFill.transform.SetParent(pbBg.transform, false);
            var pbFillRt = pbFill.GetComponent<RectTransform>();
            pbFillRt.anchorMin = new Vector2(0f, 0f);
            pbFillRt.anchorMax = new Vector2(pct, 1f);
            pbFillRt.offsetMin = Vector2.zero;
            pbFillRt.offsetMax = Vector2.zero;
            var pbFillImg = pbFill.GetComponent<Image>();
            pbFillImg.color = isUnlocked ? new Color(0.35f, 0.95f, 0.55f) : RarityColor(a.rarity);

            // Progress label
            var pbLabelGo = new GameObject("ProgressLabel", typeof(RectTransform));
            pbLabelGo.transform.SetParent(rt, false);
            var pbLblRt = pbLabelGo.GetComponent<RectTransform>();
            pbLblRt.anchorMin = new Vector2(1f, 0f);
            pbLblRt.anchorMax = new Vector2(1f, 0f);
            pbLblRt.pivot = new Vector2(1f, 0f);
            pbLblRt.anchoredPosition = new Vector2(-20f, 8f);
            pbLblRt.sizeDelta = new Vector2(160f, 30f);
            var pbLblText = pbLabelGo.AddComponent<TextMeshProUGUI>();
            pbLblText.text = isUnlocked ? "✓ Tamam" : (string.IsNullOrEmpty(label) ? "" : label);
            pbLblText.fontSize = 22f;
            pbLblText.alignment = TextAlignmentOptions.Right;
            pbLblText.color = isUnlocked ? new Color(0.4f, 1f, 0.55f) : new Color(0.85f, 0.9f, 1f, 0.85f);

            return rt;
        }

        private static Color RarityColor(string rarity)
        {
            switch (rarity)
            {
                case "common":   return new Color(0.55f, 0.75f, 1f);
                case "rare":     return new Color(0.45f, 0.55f, 1f);
                case "epic":     return new Color(0.75f, 0.45f, 1f);
                case "legendary":return new Color(1f, 0.7f, 0.2f);
            }
            return new Color(0.55f, 0.75f, 1f);
        }

        private static string CategoryGlyph(string category)
        {
            switch (category)
            {
                case "discovery":  return "?";
                case "collection": return "+";
                case "special":    return "!";
                case "quiz":       return "Q";
                case "drawing":    return "P";
                case "ar":         return "T";
                case "master":     return "*";
            }
            return "?";
        }

        // ---------- Toast ----------
        private void HandleUnlocked(AchievementData a)
        {
            if (a == null) return;
            _pendingToasts.Enqueue(a);
            UpdateButtonCounter();
            if (!_toastRunning) StartCoroutine(RunToasts());
        }

        private IEnumerator RunToasts()
        {
            _toastRunning = true;
            while (_pendingToasts.Count > 0)
            {
                var a = _pendingToasts.Dequeue();
                yield return ShowToast(a);
                yield return new WaitForSeconds(0.15f);
            }
            _toastRunning = false;
        }

        private IEnumerator ShowToast(AchievementData a)
        {
            if (toastRoot == null) yield break;
            toastRoot.gameObject.SetActive(true);

            if (toastIconText != null) toastIconText.text = "+";
            if (toastTitleText != null) toastTitleText.text = $"Başarım Açıldı: {a.title}";
            if (toastDescriptionText != null) toastDescriptionText.text = a.description;

            // Slide in: ekran sağ dışından merkeze (canvas reference 1080x1920 olduğu için)
            float w = toastRoot.rect.width;
            Vector2 startPos = new Vector2(w + 60f, toastRoot.anchoredPosition.y);
            Vector2 endPos   = new Vector2(-30f, toastRoot.anchoredPosition.y);
            toastRoot.anchoredPosition = startPos;

            float t = 0f;
            while (t < toastSlideDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / toastSlideDuration);
                k = 1f - (1f - k) * (1f - k); // ease out
                toastRoot.anchoredPosition = Vector2.Lerp(startPos, endPos, k);
                yield return null;
            }
            toastRoot.anchoredPosition = endPos;

            yield return new WaitForSecondsRealtime(toastVisibleDuration);

            // Slide out
            t = 0f;
            while (t < toastSlideDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / toastSlideDuration);
                k = k * k; // ease in
                toastRoot.anchoredPosition = Vector2.Lerp(endPos, startPos, k);
                yield return null;
            }
            toastRoot.gameObject.SetActive(false);

            // Liste açıksa yenile
            if (listPanel != null && listPanel.activeInHierarchy) RebuildList();
        }
    }
}
