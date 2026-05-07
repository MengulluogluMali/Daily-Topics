using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ARFishQuiz;

public static class QuizSeedQuestions
{
    public static void Execute()
    {
        var mgr = Object.FindAnyObjectByType<QuizManager>();
        if (mgr == null) { Debug.LogError("QuizManager bulunamadi"); return; }

        var so = new SerializedObject(mgr);
        var qArr = so.FindProperty("questions");
        qArr.arraySize = 0; // sifirla

        AddQuestion(qArr, "Nemo hangi balik turudur?",
            new[] { "Palyaco Baligi", "Ton Baligi", "Kopek Baligi", "Lahos" }, 0);
        AddQuestion(qArr, "Nemo hangi deniz canlisi ile birlikte yasar?",
            new[] { "Deniz Anasi", "Mercan", "Deniz Anemonu", "Yunus" }, 2);
        AddQuestion(qArr, "Nemo'nun babasinin adi nedir?",
            new[] { "Marlin", "Dory", "Gill", "Bruce" }, 0);
        AddQuestion(qArr, "Nemo'nun vucudunda hangi renkler bulunur?",
            new[] { "Mavi - Sari", "Turuncu - Beyaz - Siyah", "Kirmizi - Yesil", "Gri - Pembe" }, 1);
        AddQuestion(qArr, "Palyaco baliklari hangi okyanuslarda yasar?",
            new[] { "Kutup Denizleri", "Karadeniz", "Hint ve Pasifik Okyanusu", "Atlas Okyanusu" }, 2);

        so.ApplyModifiedProperties();

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log($"[QuizSeedQuestions] {qArr.arraySize} soru eklendi ve kaydedildi.");
    }

    private static void AddQuestion(SerializedProperty arr, string text, string[] answers, int correct)
    {
        arr.arraySize++;
        var elem = arr.GetArrayElementAtIndex(arr.arraySize - 1);
        elem.FindPropertyRelative("questionText").stringValue = text;

        var ansArr = elem.FindPropertyRelative("answers");
        ansArr.arraySize = answers.Length;
        for (int i = 0; i < answers.Length; i++)
            ansArr.GetArrayElementAtIndex(i).stringValue = answers[i];

        elem.FindPropertyRelative("correctAnswerIndex").intValue = correct;
    }
}
