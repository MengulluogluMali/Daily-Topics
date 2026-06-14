using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using ARFishQuizEditor;

public static class AquariumRunSetup
{
    public static void Execute()
    {
        var scenePath = "Assets/Scenes/SampleScene.unity";
        if (EditorSceneManager.GetActiveScene().path != scenePath)
        {
            EditorSceneManager.OpenScene(scenePath);
        }
        AquariumSetup.Setup();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[AquariumRunSetup] Done.");
    }
}
