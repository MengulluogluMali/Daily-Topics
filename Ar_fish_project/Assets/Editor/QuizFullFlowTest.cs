using UnityEngine;
using UnityEditor;
using TMPro;
using ARFishQuiz;
using System.Reflection;
using System.Text;
using System.Collections;

public static class QuizFullFlowTest
{
    public static void Execute()
    {
        StringBuilder log = new StringBuilder();
        log.AppendLine("========= TAM QUIZ AKIS TESTI =========");

        var mgr = Object.FindAnyObjectByType<QuizManager>();
        if (mgr == null) { Debug.LogError("QuizManager yok"); return; }

        var type = typeof(QuizManager);
        var soMgr = new SerializedObject(mgr);
        var questionsList = type.GetField("questions", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(mgr) as IList;
        log.AppendLine($"[SETUP] Soru sayisi: {questionsList.Count}");

        var panelGO = soMgr.FindProperty("quizPanel").objectReferenceValue as GameObject;
        var questionPanel = soMgr.FindProperty("questionPanel").objectReferenceValue as GameObject;
        var resultPanel = soMgr.FindProperty("resultPanel").objectReferenceValue as GameObject;
        var qTextRef = soMgr.FindProperty("questionText").objectReferenceValue as TMP_Text;
        var counterRef = soMgr.FindProperty("questionCounterText").objectReferenceValue as TMP_Text;
        var resultTextRef = soMgr.FindProperty("resultText").objectReferenceValue as TMP_Text;

        // QUIZ'I BASLAT
        mgr.StartQuiz();
        log.AppendLine($"\n[Baslangic] Panel aktif: {panelGO.activeSelf}");
        log.AppendLine($"[Baslangic] QuestionPanel aktif: {questionPanel.activeSelf}");
        log.AppendLine($"[Baslangic] ResultPanel aktif: {resultPanel.activeSelf}");
        log.AppendLine($"[Baslangic] Sayac: '{counterRef.text}'");
        log.AppendLine($"[Baslangic] Soru: '{qTextRef.text}'");

        // Coroutine olmadan: tum sorulari sirayla yanitla
        var onAnswerMethod = type.GetMethod("OnAnswerSelected", BindingFlags.NonPublic | BindingFlags.Instance);
        var nextQuestionMethod = type.GetMethod("NextQuestion", BindingFlags.NonPublic | BindingFlags.Instance);
        var showResultMethod = type.GetMethod("ShowResult", BindingFlags.NonPublic | BindingFlags.Instance);
        var scoreField = type.GetField("score", BindingFlags.NonPublic | BindingFlags.Instance);
        var currentIdxField = type.GetField("currentQuestionIndex", BindingFlags.NonPublic | BindingFlags.Instance);

        // Invoke ile geciken NextQuestion'i durdurup manuel cagiracagiz (test icin)
        for (int i = 0; i < questionsList.Count; i++)
        {
            var q = questionsList[i];
            int correctIdx = (int)q.GetType().GetField("correctAnswerIndex").GetValue(q);

            // Test icin ilk 3 soruya dogru, sonraki 2 soruya yanlis cevap verelim
            int chosen = i < 3 ? correctIdx : (correctIdx + 1) % 4;

            onAnswerMethod.Invoke(mgr, new object[] { chosen });
            // Invoke kuyruga NextQuestion ekler, biz manuel cagiralim:
            mgr.CancelInvoke("NextQuestion");
            nextQuestionMethod.Invoke(mgr, null);

            log.AppendLine($"[Soru {i + 1}] Verilen={chosen}, Dogru={correctIdx}, Skor={scoreField.GetValue(mgr)}");
        }

        log.AppendLine($"\n[Sonuc] QuestionPanel aktif: {questionPanel.activeSelf}");
        log.AppendLine($"[Sonuc] ResultPanel aktif: {resultPanel.activeSelf}");
        log.AppendLine($"[Sonuc] ResultText: '{resultTextRef.text}'");
        log.AppendLine($"[Sonuc] Toplam skor: {scoreField.GetValue(mgr)} / {questionsList.Count}");

        // Temizle
        mgr.CloseQuiz();
        log.AppendLine($"\n[Kapat] Panel aktif: {panelGO.activeSelf}");
        log.AppendLine("========= TEST TAMAMLANDI =========");

        Debug.Log(log.ToString());
    }
}
