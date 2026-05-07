using UnityEngine;
using UnityEditor;
using ARFishQuiz;

public static class QuizPreviewToggle
{
    public static void ShowPanel()
    {
        var mgr = Object.FindFirstObjectByType<QuizManager>();
        if (mgr == null) { Debug.LogError("QuizManager yok"); return; }
        mgr.StartQuiz();
        EditorApplication.QueuePlayerLoopUpdate();
        Debug.Log("Quiz paneli açıldı (önizleme için).");
    }

    public static void HidePanel()
    {
        var mgr = Object.FindFirstObjectByType<QuizManager>();
        if (mgr == null) { Debug.LogError("QuizManager yok"); return; }
        mgr.CloseQuiz();
        Debug.Log("Quiz paneli kapandı.");
    }
}
