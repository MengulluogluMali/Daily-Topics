#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class PreviewQuizPanel
{
    public static void Execute()
    {
        var canvasGO = GameObject.Find("zargana_balıgı_target/QuizCanvas");
        if (canvasGO == null) return;

        var info = canvasGO.transform.Find("InfoPanel")?.gameObject;
        var quiz = canvasGO.transform.Find("QuizPanel")?.gameObject;
        if (info != null) info.SetActive(false);
        if (quiz != null) quiz.SetActive(true);

        var qPanel = quiz?.transform.Find("QuestionPanel")?.gameObject;
        var rPanel = quiz?.transform.Find("ResultPanel")?.gameObject;
        if (qPanel != null) qPanel.SetActive(true);
        if (rPanel != null) rPanel.SetActive(false);
    }
}
#endif
