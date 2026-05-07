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
    /// Balık Nemo ile ilgili quiz sistemini yöneten sınıf.
    /// Quiz panelini gösterir/gizler, soruları sırayla sunar,
    /// kullanıcı cevaplarına göre puan hesaplar ve sonunda skoru gösterir.
    /// </summary>
    public class QuizManager : MonoBehaviour
    {
        [Header("UI Referansları")]
        [SerializeField] private GameObject quizPanel;
        [SerializeField] private GameObject questionPanel;
        [SerializeField] private GameObject resultPanel;
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

        [Header("Sorular")]
        [SerializeField] private List<QuizQuestion> questions = new List<QuizQuestion>();

        private int currentQuestionIndex = 0;
        private int score = 0;
        private bool isAnswering = false;

        private void Awake()
        {
            if (questions == null || questions.Count == 0)
            {
                CreateDefaultQuestions();
            }

            if (quizPanel != null) quizPanel.SetActive(false);

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
        }

        private void CreateDefaultQuestions()
        {
            questions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    questionText = "Nemo hangi balık türüdür?",
                    answers = new[] { "Palyaço Balığı", "Ton Balığı", "Köpek Balığı", "Lahos" },
                    correctAnswerIndex = 0
                },
                new QuizQuestion
                {
                    questionText = "Nemo hangi deniz canlısı ile birlikte yaşar?",
                    answers = new[] { "Deniz Anası", "Mercan", "Deniz Anemonu", "Yunus" },
                    correctAnswerIndex = 2
                },
                new QuizQuestion
                {
                    questionText = "Nemo'nun babasının adı nedir?",
                    answers = new[] { "Marlin", "Dory", "Gill", "Bruce" },
                    correctAnswerIndex = 0
                },
                new QuizQuestion
                {
                    questionText = "Nemo'nun vücudunda hangi renkler bulunur?",
                    answers = new[] { "Mavi - Sarı", "Turuncu - Beyaz - Siyah", "Kırmızı - Yeşil", "Gri - Pembe" },
                    correctAnswerIndex = 1
                },
                new QuizQuestion
                {
                    questionText = "Palyaço balıkları hangi okyanuslarda yaşar?",
                    answers = new[] { "Kutup Denizleri", "Karadeniz", "Hint ve Pasifik Okyanusu", "Atlas Okyanusu" },
                    correctAnswerIndex = 2
                }
            };
        }

        /// <summary>
        /// Quiz'i başlatır; paneli açar, skoru sıfırlar ve ilk soruyu yükler.
        /// </summary>
        public void StartQuiz()
        {
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
            if (currentQuestionIndex >= questions.Count)
            {
                ShowResult();
                return;
            }

            QuizQuestion q = questions[currentQuestionIndex];

            if (questionText != null) questionText.text = q.questionText;
            if (questionCounterText != null)
                questionCounterText.text = $"Soru {currentQuestionIndex + 1} / {questions.Count}";

            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (answerButtons[i] == null) continue;

                bool hasAnswer = i < q.answers.Length;
                answerButtons[i].gameObject.SetActive(hasAnswer);

                if (hasAnswer)
                {
                    if (answerTexts[i] != null) answerTexts[i].text = q.answers[i];
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

            QuizQuestion q = questions[currentQuestionIndex];
            bool isCorrect = index == q.correctAnswerIndex;

            if (isCorrect) score++;

            // Seçilen butonu ve doğru olanı renklendir
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

            if (resultText != null)
            {
                string comment;
                float ratio = questions.Count > 0 ? (float)score / questions.Count : 0f;
                if (ratio >= 0.8f) comment = "Harika! Sen gerçek bir Nemo uzmanısın!";
                else if (ratio >= 0.5f) comment = "Güzel iş! Biraz daha çalışırsan mükemmel olacak.";
                else comment = "Nemo hakkında daha çok şey öğrenmelisin.";

                resultText.text = $"Puanın: {score} / {questions.Count}\n\n{comment}";
            }
        }
    }
}
