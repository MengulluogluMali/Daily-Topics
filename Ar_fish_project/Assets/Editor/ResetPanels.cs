#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ResetPanels
{
    public static void Execute()
    {
        var canvasGO = GameObject.Find("zargana_balıgı_target/QuizCanvas");
        if (canvasGO == null) return;

        var info = canvasGO.transform.Find("InfoPanel")?.gameObject;
        var quiz = canvasGO.transform.Find("QuizPanel")?.gameObject;
        if (info != null) info.SetActive(false);
        if (quiz != null) quiz.SetActive(false);

        EditorSceneManager.MarkSceneDirty(canvasGO.scene);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Paneller başlangıç durumuna alındı (her ikisi de kapalı). Runtime'da QuizManager InfoPanel'i otomatik açacak.");
    }
}
#endif
