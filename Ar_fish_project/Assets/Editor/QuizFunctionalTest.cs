using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using ARFishQuiz;
using System.Reflection;
using System.Text;

public static class QuizFunctionalTest
{
    public static void Execute()
    {
        StringBuilder log = new StringBuilder();
        log.AppendLine("========= QUIZ FUNCTIONAL TEST =========");

        // 1. Gerekli nesnelerin varlığı
        var btnGO = GameObject.Find("ImageTarget/QuizButton");
        var canvasGO = GameObject.Find("ImageTarget/QuizCanvas");
        var panelGO = GameObject.Find("ImageTarget/QuizCanvas/QuizPanel");

        log.AppendLine($"[1] QuizButton var mi?    : {(btnGO != null ? "EVET" : "HAYIR")}");
        log.AppendLine($"[1] QuizCanvas var mi?    : {(canvasGO != null ? "EVET" : "HAYIR")}");
        log.AppendLine($"[1] QuizPanel var mi?     : {(panelGO != null ? "EVET" : "HAYIR")}");

        if (btnGO == null || canvasGO == null || panelGO == null)
        {
            Debug.LogError(log.ToString() + "\nGerekli nesneler eksik!");
            return;
        }

        // 2. Script referansları
        var quizBtn = btnGO.GetComponent<QuizButton3D>();
        var manager = canvasGO.GetComponent<QuizManager>();
        var collider = btnGO.GetComponent<Collider>();
        log.AppendLine($"[2] QuizButton3D component: {(quizBtn != null ? "OK" : "YOK")}");
        log.AppendLine($"[2] QuizManager component : {(manager != null ? "OK" : "YOK")}");
        log.AppendLine($"[2] Collider (tiklanabilir): {(collider != null ? collider.GetType().Name : "YOK")}");

        // 3. QuizButton3D -> QuizManager referansi bagli mi?
        var soBtn = new SerializedObject(quizBtn);
        var mgrRef = soBtn.FindProperty("quizManager").objectReferenceValue;
        log.AppendLine($"[3] QuizButton3D.quizManager bagli mi?: {(mgrRef == manager ? "EVET" : "HAYIR")}");

        // 4. QuizManager'in tum UI referanslari bagli mi?
        var soMgr = new SerializedObject(manager);
        string[] refFields = { "quizPanel", "questionPanel", "resultPanel", "questionText",
                              "questionCounterText", "resultText", "restartButton", "closeButton" };
        foreach (var f in refFields)
        {
            var p = soMgr.FindProperty(f);
            log.AppendLine($"[4] {f,-22}: {(p.objectReferenceValue != null ? "BAGLI" : "BOS!")}");
        }
        var btnArr = soMgr.FindProperty("answerButtons");
        var txtArr = soMgr.FindProperty("answerTexts");
        log.AppendLine($"[4] answerButtons boyut   : {btnArr.arraySize}");
        log.AppendLine($"[4] answerTexts boyut     : {txtArr.arraySize}");
        int answerButtonsBound = 0;
        for (int i = 0; i < btnArr.arraySize; i++)
            if (btnArr.GetArrayElementAtIndex(i).objectReferenceValue != null) answerButtonsBound++;
        log.AppendLine($"[4] answerButtons bagli   : {answerButtonsBound}/4");

        // 5. Soru listesi
        var qField = typeof(QuizManager).GetField("questions", BindingFlags.NonPublic | BindingFlags.Instance);
        var questions = qField.GetValue(manager) as System.Collections.IList;
        log.AppendLine($"[5] Soru sayisi           : {(questions != null ? questions.Count : 0)}");

        // 6. EventSystem var mi?
        var es = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        log.AppendLine($"[6] EventSystem var mi    : {(es != null ? "EVET" : "HAYIR")}");

        // 7. QuizManager.StartQuiz() cagrildiginda panel aciliyor mu?
        panelGO.SetActive(false);
        bool beforeActive = panelGO.activeSelf;
        manager.StartQuiz();
        bool afterActive = panelGO.activeSelf;
        log.AppendLine($"[7] StartQuiz() panel aciyor mu?: once={beforeActive}, sonra={afterActive}");

        // 8. Ilk soru yuklendi mi?
        var questionTextRef = soMgr.FindProperty("questionText").objectReferenceValue as TMP_Text;
        var counterRef = soMgr.FindProperty("questionCounterText").objectReferenceValue as TMP_Text;
        log.AppendLine($"[8] Ilk soru metni        : '{(questionTextRef != null ? questionTextRef.text : "NULL")}'");
        log.AppendLine($"[8] Sayac metni           : '{(counterRef != null ? counterRef.text : "NULL")}'");

        // 9. Cevap butonlarindaki metinler dolu mu?
        for (int i = 0; i < txtArr.arraySize; i++)
        {
            var t = txtArr.GetArrayElementAtIndex(i).objectReferenceValue as TMP_Text;
            log.AppendLine($"[9] Cevap {i}              : '{(t != null ? t.text : "NULL")}'");
        }

        // 10. Bir cevaba tiklamayi simule et (yanlis cevap)
        var ansField = typeof(QuizManager).GetMethod("OnAnswerSelected", BindingFlags.NonPublic | BindingFlags.Instance);
        int wrongIndex = 0; // 0. soruda dogru cevap 0 (Palyaco Baligi), yanlis olarak 1 secelim
        ansField.Invoke(manager, new object[] { 1 });
        log.AppendLine($"[10] Yanlis cevap simule edildi (index=1)");

        // 11. Paneli kapat
        manager.CloseQuiz();
        log.AppendLine($"[11] Panel kapatildi. Aktif mi?: {panelGO.activeSelf}");

        log.AppendLine("========= TEST TAMAMLANDI =========");
        Debug.Log(log.ToString());
    }
}
