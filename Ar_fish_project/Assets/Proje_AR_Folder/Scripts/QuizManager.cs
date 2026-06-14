using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ARFishQuiz
{
    [System.Serializable]
    public class QuizQuestion
    {
        [TextArea(2, 4)]
        public string questionText;
        public string[] answers = new string[4];
        [Tooltip("0-3 arası doğru cevap indeksi")]
        public int correctAnswerIndex;
    }

    /// <summary>
    /// Bir balık hedefi (Image Target) için bilgi paneli ve quiz akışını yöneten sınıf.
    /// AR ilk başladığında InfoPanel açılır; kullanıcı QuizButton'a basınca QuizPanel açılır
    /// ve sorular FishQuizDB.json'dan ilgili fishId'ye göre yüklenir.
    /// </summary>
    public class QuizManager : MonoBehaviour
    {
        [Header("Balık Kimliği (JSON ile eşleşmeli)")]
        [Tooltip("FishInfoDB ve FishQuizDB içindeki fishId. Inspector'da dropdown'dan seçilir.")]
        [FishId]
        [SerializeField] private string fishId = "zargana";

        [Header("Bilgi Paneli (AR açılınca gösterilir)")]
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private TMP_Text infoTitleText;
        [SerializeField] private TMP_Text infoScientificNameText;
        [SerializeField] private TMP_Text infoDescriptionText;
        [SerializeField] private TMP_Text infoHabitatText;
        [SerializeField] private TMP_Text infoDietText;
        [Tooltip("Balığın yenilebilirlik durumu (JSON 'durum' alanı).")]
        [SerializeField] private TMP_Text infoStatusText;
        [Tooltip("Balığın tarifi (JSON 'tarif' alanı). Tarif yoksa gizlenir.")]
        [SerializeField] private TMP_Text infoRecipeText;
        [SerializeField] private Button infoCloseButton;

        [Tooltip("AR sahnesi başladığında bilgi paneli otomatik açılsın mı?")]
        [SerializeField] private bool showInfoOnStart = true;

        [Header("Quiz UI Referansları")]
        [SerializeField] private GameObject quizPanel;
        [SerializeField] private GameObject questionPanel;
        [SerializeField] private GameObject resultPanel;
        [Tooltip("Quiz panelinin üstündeki başlık. Otomatik olarak '<DisplayName> Quiz' yazılır.")]
        [SerializeField] private TMP_Text quizTitleText;
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private TMP_Text questionCounterText;
        [SerializeField] private Button[] answerButtons = new Button[4];
        [SerializeField] private TMP_Text[] answerTexts = new TMP_Text[4];
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button closeButton;

        [Header("Renkler")]
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color correctColor = new Color(0.3f, 0.85f, 0.3f);
        [SerializeField] private Color wrongColor = new Color(0.9f, 0.3f, 0.3f);

        [Header("Ayarlar")]
        [SerializeField] private float nextQuestionDelay = 1.2f;

        [Tooltip("Eğer JSON'dan veri bulunamazsa burada manuel olarak verilen sorular kullanılır.")]
        [SerializeField] private List<QuizQuestion> fallbackQuestions = new List<QuizQuestion>();

        // Çalışma anında kullanılan sorular (JSON'dan veya fallback'tan)
        private List<QuizQuestion> activeQuestions = new List<QuizQuestion>();

        private int currentQuestionIndex = 0;
        private int score = 0;
        private bool isAnswering = false;

        public string FishId => fishId;

        private void Awake()
        {
            // Tüm panelleri kapat - Start'ta uygun olan açılır
            if (quizPanel != null) quizPanel.SetActive(false);
            if (infoPanel != null) infoPanel.SetActive(false);

            // Cevap butonları
            for (int i = 0; i < answerButtons.Length; i++)
            {
                int index = i;
                if (answerButtons[i] != null)
                {
                    answerButtons[i].onClick.RemoveAllListeners();
                    answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
                }
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(StartQuiz);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseQuiz);
            }

            if (infoCloseButton != null)
            {
                infoCloseButton.onClick.RemoveAllListeners();
                infoCloseButton.onClick.AddListener(CloseInfo);
            }
        }

        private void Start()
        {
            // Bilgi panelini ve quiz başlığını doldur
            PopulateInfoPanel();
            UpdateQuizTitle();

            if (showInfoOnStart && infoPanel != null)
            {
                infoPanel.SetActive(true);
            }
        }

        // Vuforia hedef bulunduğunda bu nesne aktive olur. Her aktivasyonda
        // info panel metinlerini JSON'dan tekrar doldur (Android'de ilk Start
        // sahne yüklenirken çalışmamış olabilir veya JSON ilk kez yüklendi).
        private void OnEnable()
        {
            if (!string.IsNullOrEmpty(fishId))
            {
                PopulateInfoPanel();
                UpdateQuizTitle();
            }
        }

        private void UpdateQuizTitle()
        {
            if (quizTitleText == null) return;
            FishInfo info = !string.IsNullOrEmpty(fishId) ? FishDatabase.GetInfo(fishId) : null;
            string name = (info != null && !string.IsNullOrEmpty(info.displayName)) ? info.displayName : fishId;
            quizTitleText.text = $"{name} Quiz";
        }

        // ============== INFO PANEL ==============

        private void PopulateInfoPanel()
        {
            if (string.IsNullOrEmpty(fishId)) return;
            FishInfo info = FishDatabase.GetInfo(fishId);
            if (info == null)
            {
                // JSON yüklenememiş olabilir (Android build'de Resources eksikse).
                // En azından prefab'ta kalmış yanlış metinleri silelim ki başka balığın
                // bilgisi görünmesin.
                Debug.LogWarning($"[QuizManager] '{fishId}' için FishInfoDB içinde bilgi bulunamadı. Metinler temizleniyor.");
                if (infoTitleText != null) infoTitleText.text = fishId;
                if (infoScientificNameText != null) infoScientificNameText.text = "";
                if (infoDescriptionText != null) infoDescriptionText.text = "";
                if (infoHabitatText != null) infoHabitatText.text = "";
                if (infoDietText != null) infoDietText.text = "";
                if (infoStatusText != null) infoStatusText.text = "";
                if (infoRecipeText != null)
                {
                    infoRecipeText.text = "";
                    infoRecipeText.gameObject.SetActive(false);
                }
                return;
            }

            if (infoTitleText != null) infoTitleText.text = info.displayName;
            if (infoScientificNameText != null) infoScientificNameText.text = info.scientificName;
            if (infoDescriptionText != null) infoDescriptionText.text = info.shortDescription;
            if (infoHabitatText != null) infoHabitatText.text = $"<b>Yaşam Alanı:</b> {info.habitat}";
            if (infoDietText != null) infoDietText.text = $"<b>Beslenme:</b> {info.diet}";

            // Durum (yenilebilirlik) — renkli etiketle göster
            if (infoStatusText != null)
            {
                infoStatusText.text = FormatDurum(info.durum);
            }

            // Tarif — sadece varsa göster, yoksa gizle
            if (infoRecipeText != null)
            {
                bool hasRecipe = !string.IsNullOrWhiteSpace(info.tarif);
                infoRecipeText.gameObject.SetActive(hasRecipe);
                infoRecipeText.text = hasRecipe ? $"<b>Tarif:</b> {info.tarif}" : "";
            }
        }

        /// <summary>
        /// JSON'daki "durum" kodunu kullanıcı dostu, renkli bir etikete çevirir.
        /// </summary>
        private static string FormatDurum(string durum)
        {
            if (string.IsNullOrWhiteSpace(durum))
                return "";

            switch (durum.Trim().ToLowerInvariant())
            {
                case "yenir":
                    return "<b>Durum:</b> <color=#5FD68A>Yenebilir</color>";
                case "yenmez":
                    return "<b>Durum:</b> <color=#E55B5B>Yenmez / Zehirli!</color>";
                case "dikkat":
                    return "<b>Durum:</b> <color=#F2C14E>Dikkat! Korunan / Riskli</color>";
                default:
                    return $"<b>Durum:</b> {durum}";
            }
        }

        public void ShowInfo()
        {
            PopulateInfoPanel();
            if (infoPanel != null) infoPanel.SetActive(true);
            if (quizPanel != null) quizPanel.SetActive(false);

            // Başarım sistemi: balık tarandı / keşfedildi bildir
            if (AchievementManager.Instance != null && !string.IsNullOrEmpty(fishId))
            {
                AchievementManager.Instance.NotifyFishScanned(fishId);
            }
        }

        public void CloseInfo()
        {
            if (infoPanel != null) infoPanel.SetActive(false);
        }

        /// <summary>
        /// UI "Çizim / Akvaryuma git" butonu için: bu balık adına çizim panelini açar.
        /// Çizim kaydedilince viewer otomatik açılır.
        /// </summary>
        public void OpenAquariumDrawing()
        {
            if (AquariumDrawingManager.Instance == null)
            {
                Debug.LogWarning("[QuizManager] AquariumDrawingManager bulunamadı!");
                return;
            }

            string idToUse = transform.parent != null ? transform.parent.name : fishId;
            AquariumDrawingManager.Instance.OpenForFish(
                idToUse,
                onSaved: () =>
                {
                    if (AquariumViewerManager.Instance != null)
                        AquariumViewerManager.Instance.OpenViewerForFish(idToUse);
                });
        }

        // ============== QUIZ ==============

        /// <summary>
        /// JSON'dan veya fallback'tan soruları activeQuestions listesine yükler.
        /// </summary>
        private void LoadQuestionsForFish()
        {
            activeQuestions = new List<QuizQuestion>();

            FishQuiz dbQuiz = !string.IsNullOrEmpty(fishId) ? FishDatabase.GetQuiz(fishId) : null;
            if (dbQuiz != null && dbQuiz.questions != null && dbQuiz.questions.Count > 0)
            {
                foreach (var q in dbQuiz.questions)
                {
                    if (q == null) continue;
                    activeQuestions.Add(new QuizQuestion
                    {
                        questionText = q.questionText,
                        answers = q.options != null ? (string[])q.options.Clone() : new string[0],
                        correctAnswerIndex = q.correctOptionIndex
                    });
                }
            }

            // Eğer JSON'dan yüklenemediyse fallback kullan
            if (activeQuestions.Count == 0 && fallbackQuestions != null && fallbackQuestions.Count > 0)
            {
                Debug.LogWarning($"[QuizManager] '{fishId}' için JSON'dan soru bulunamadı, fallback sorular kullanılıyor.");
                activeQuestions.AddRange(fallbackQuestions);
            }
        }

        /// <summary>
        /// Quiz'i başlatır; bilgi panelini kapatır, quiz panelini açar.
        /// </summary>
        public void StartQuiz()
        {
            LoadQuestionsForFish();
            UpdateQuizTitle();

            if (activeQuestions == null || activeQuestions.Count == 0)
            {
                Debug.LogError($"[QuizManager] '{fishId}' için hiç soru bulunamadı!");
                return;
            }

            if (infoPanel != null) infoPanel.SetActive(false);
            if (quizPanel != null) quizPanel.SetActive(true);
            if (questionPanel != null) questionPanel.SetActive(true);
            if (resultPanel != null) resultPanel.SetActive(false);

            currentQuestionIndex = 0;
            score = 0;
            isAnswering = false;
            LoadQuestion();
        }

        public void CloseQuiz()
        {
            if (quizPanel != null) quizPanel.SetActive(false);
        }

        private void LoadQuestion()
        {
            if (currentQuestionIndex >= activeQuestions.Count)
            {
                ShowResult();
                return;
            }

            QuizQuestion q = activeQuestions[currentQuestionIndex];

            if (questionText != null) questionText.text = q.questionText;
            if (questionCounterText != null)
                questionCounterText.text = $"Soru {currentQuestionIndex + 1} / {activeQuestions.Count}";

            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (answerButtons[i] == null) continue;

                bool hasAnswer = q.answers != null && i < q.answers.Length;
                answerButtons[i].gameObject.SetActive(hasAnswer);

                if (hasAnswer)
                {
                    if (i < answerTexts.Length && answerTexts[i] != null) answerTexts[i].text = q.answers[i];
                    var img = answerButtons[i].GetComponent<Image>();
                    if (img != null) img.color = defaultColor;
                    answerButtons[i].interactable = true;
                }
            }

            isAnswering = true;
        }

        private void OnAnswerSelected(int index)
        {
            if (!isAnswering) return;
            isAnswering = false;

            QuizQuestion q = activeQuestions[currentQuestionIndex];
            bool isCorrect = index == q.correctAnswerIndex;

            if (isCorrect) score++;

            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (answerButtons[i] == null) continue;
                answerButtons[i].interactable = false;

                var img = answerButtons[i].GetComponent<Image>();
                if (img == null) continue;

                if (i == q.correctAnswerIndex)
                    img.color = correctColor;
                else if (i == index)
                    img.color = wrongColor;
            }

            CancelInvoke(nameof(NextQuestion));
            Invoke(nameof(NextQuestion), nextQuestionDelay);
        }

        private void NextQuestion()
        {
            currentQuestionIndex++;
            LoadQuestion();
        }

        private void ShowResult()
        {
            if (questionPanel != null) questionPanel.SetActive(false);
            if (resultPanel != null) resultPanel.SetActive(true);

            int total = activeQuestions.Count;

            if (resultText != null)
            {
                string comment;
                float ratio = total > 0 ? (float)score / total : 0f;
                if (ratio >= 0.8f) comment = "Harika! Sen gerçek bir uzman gibisin!";
                else if (ratio >= 0.5f) comment = "Güzel iş! Biraz daha çalışırsan mükemmel olacak.";
                else comment = "Bu balık hakkında daha çok şey öğrenmelisin.";

                resultText.text = $"Puanın: {score} / {total}\n\n{comment}";
            }

            // Başarım sistemi: quiz tamamlandı bildir
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.NotifyQuizCompleted(fishId, score, total);
            }
        }
    }
}
