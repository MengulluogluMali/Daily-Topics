using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace ARFishQuiz
{
    /// <summary>
    /// Sağ üstte her zaman açık olan "Akvaryum" butonu ve bu butona basıldığında
    /// kullanıcının çizdiği akvaryumu (kaçıncı balık ise o balığa ait) gösteren panel.
    /// </summary>
    public class AquariumViewerManager : MonoBehaviour
    {
        public static AquariumViewerManager Instance { get; private set; }

        [Header("UI")]
        [SerializeField] private Canvas overlayCanvas;       // her zaman aktif kanvas
        [SerializeField] private Button openViewerButton;    // sağ üstteki "Akvaryum" butonu
        [SerializeField] private GameObject viewerPanel;     // büyük görüntüleme paneli
        [SerializeField] private RawImage viewerImage;       // gösterilen çizim
        [SerializeField] private Text viewerTitle;           // başlık (balık adı)
        [SerializeField] private Text viewerCounterText;     // "1 / 3" göstergesi
        [SerializeField] private Button viewerCloseButton;
        [SerializeField] private Button viewerNextButton;
        [SerializeField] private Button viewerPrevButton;
        [SerializeField] private GameObject emptyStatePanel; // hiç çizim yoksa

        // Sırasıyla balıklar (Türkçe id sırası: kaçıncı çizdiğine göre değil de
        // proje balıkları sırasına göre listelenir, sadece kaydı olanlar görünür).
        // NOT: Bu id'ler sahnedeki ImageTarget objelerinin gerçek adlarıyla
        // ve Akvaryuma_git butonlarındaki fishId değerleriyle birebir eşleşmelidir,
        // aksi halde kaydedilen çizimler viewer'da listelenmez.
        private static readonly string[] FishOrder = new string[]
        {
            "zargana_balıgı_target",
            "mersin_balıgı_target",
            "kopek_balıgı_target",
            "pisi_balıgı_target",
            "fener_balıgı_target",
            "balon_balıgı_target",
            "vatoz_balıgı_target",
            "mavi_yengec_target",
            "benekli_dil_balıgı_target",
            "kirlangic_balıgı_target",
            "kalkan_balıgı_target",
            "uzun_burunlu_fare_balıgı_target",
        };

        private List<string> savedFishIds = new List<string>();
        private int currentIndex = 0;
        private Texture2D loadedTex;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (openViewerButton != null)  { openViewerButton.onClick.RemoveAllListeners();  openViewerButton.onClick.AddListener(OpenViewer); }
            if (viewerCloseButton != null) { viewerCloseButton.onClick.RemoveAllListeners(); viewerCloseButton.onClick.AddListener(CloseViewer); }
            if (viewerNextButton != null)  { viewerNextButton.onClick.RemoveAllListeners();  viewerNextButton.onClick.AddListener(NextFish); }
            if (viewerPrevButton != null)  { viewerPrevButton.onClick.RemoveAllListeners();  viewerPrevButton.onClick.AddListener(PrevFish); }

            if (viewerPanel != null) viewerPanel.SetActive(false);
        }

        public void OpenViewer()
        {
            RefreshSavedList();
            if (viewerPanel != null) viewerPanel.SetActive(true);

            if (savedFishIds.Count == 0)
            {
                ShowEmptyState();
                return;
            }
            if (emptyStatePanel != null) emptyStatePanel.SetActive(false);
            currentIndex = Mathf.Clamp(currentIndex, 0, savedFishIds.Count - 1);
            ShowCurrent();
        }

        public void CloseViewer()
        {
            if (viewerPanel != null) viewerPanel.SetActive(false);
            if (loadedTex != null) { Destroy(loadedTex); loadedTex = null; }
        }

        public void NextFish()
        {
            if (savedFishIds.Count == 0) return;
            currentIndex = (currentIndex + 1) % savedFishIds.Count;
            ShowCurrent();
        }

        public void PrevFish()
        {
            if (savedFishIds.Count == 0) return;
            currentIndex = (currentIndex - 1 + savedFishIds.Count) % savedFishIds.Count;
            ShowCurrent();
        }

        /// <summary>
        /// Belirli bir balığın akvaryumuna direkt atlamak için.
        /// Akvaryum çiz tamamlandığında çağrılabilir.
        /// </summary>
        public void OpenViewerForFish(string fishId)
        {
            RefreshSavedList();
            int idx = savedFishIds.IndexOf(fishId);
            if (idx < 0)
            {
                OpenViewer();
                return;
            }
            currentIndex = idx;
            if (viewerPanel != null) viewerPanel.SetActive(true);
            if (emptyStatePanel != null) emptyStatePanel.SetActive(false);
            ShowCurrent();
        }

        private void RefreshSavedList()
        {
            savedFishIds.Clear();
            foreach (var id in FishOrder)
            {
                string path = AquariumDrawingManager.GetSavePath(id);
                if (File.Exists(path))
                    savedFishIds.Add(id);
            }
        }

        private void ShowEmptyState()
        {
            if (emptyStatePanel != null) emptyStatePanel.SetActive(true);
            if (viewerImage != null) viewerImage.gameObject.SetActive(false);
            if (viewerTitle != null) viewerTitle.text = "Henüz akvaryum çizmediniz";
            if (viewerCounterText != null) viewerCounterText.text = "0 / 0";
        }

        private void ShowCurrent()
        {
            if (savedFishIds.Count == 0) { ShowEmptyState(); return; }
            string fishId = savedFishIds[currentIndex];
            string path = AquariumDrawingManager.GetSavePath(fishId);
            if (!File.Exists(path)) { RefreshSavedList(); ShowEmptyState(); return; }

            byte[] bytes = File.ReadAllBytes(path);
            if (loadedTex == null) loadedTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            loadedTex.LoadImage(bytes);
            loadedTex.Apply();

            if (viewerImage != null)
            {
                viewerImage.gameObject.SetActive(true);
                viewerImage.texture = loadedTex;
            }
            if (viewerTitle != null) viewerTitle.text = $"🐟 {PrettyName(fishId)} Akvaryumu";
            if (viewerCounterText != null)
                viewerCounterText.text = $"{currentIndex + 1} / {savedFishIds.Count}";
        }

        private static string PrettyName(string id)
        {
            // İçerideki kullandığımız id'lerden okunaklı Türkçe ad üret.
            switch (id)
            {
                case "zargana_balıgı_target":     return "Zargana Balığı";
                case "mersin_balıgı_target":      return "Mersin Balığı";
                case "kopek_balıgı_target":       return "Köpek Balığı";
                case "pisi_balıgı_target":        return "Pisi Balığı";
                case "fener_balıgı_target":       return "Fener Balığı";
                case "balon_balıgı_target":       return "Balon Balığı";
                case "vatoz_balıgı_target":       return "Vatoz Balığı";
                case "mavi_yengec_target":        return "Mavi Yengeç";
                case "benekli_dil_balıgı_target": return "Benekli Dil Balığı";
                case "kirlangic_balıgı_target":   return "Kırlangıç Balığı";
                case "kalkan_balıgı_target":      return "Kalkan Balığı";
            }
            string s = id.Replace("_target", "").Replace("_", " ");
            if (s.Length == 0) return id;
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}
