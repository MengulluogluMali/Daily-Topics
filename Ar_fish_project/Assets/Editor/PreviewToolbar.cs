using UnityEditor;
using UnityEngine;

public static class PreviewToolbar
{
    public static void Preview()
    {
        var rp = GameObject.Find("AquariumSystem/DrawingManager/AquariumDrawCanvas/RootPanel");
        if (rp != null)
        {
            rp.SetActive(true);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
    public static void Hide()
    {
        var rp = GameObject.Find("AquariumSystem/DrawingManager/AquariumDrawCanvas/RootPanel");
        if (rp != null)
        {
            rp.SetActive(false);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }
    }
}
