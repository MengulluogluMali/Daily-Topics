using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SaveSceneNow
{
    public static void Execute()
    {
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Scene saved.");
    }
}
