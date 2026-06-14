using UnityEditor;
using UnityEngine;
using ARFishQuiz;
using System.Reflection;
using System.IO;
using TMPro;

public static class CheckMersinSetup
{
    public static void Run()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            var qms = root.GetComponentsInChildren<QuizManager>(true);
            foreach (var qm in qms)
            {
                string id = (string)typeof(QuizManager).GetField("fishId", BindingFlags.NonPublic|BindingFlags.Instance).GetValue(qm);
                Debug.Log($"QuizManager on '{GetPath(qm.transform)}' fishId='{id}'");
            }
        }
    }

    public static void CheckResources()
    {
        var ti = Resources.Load<TextAsset>("FishInfoDB");
        var tq = Resources.Load<TextAsset>("FishQuizDB");
        Debug.Log($"Resources FishInfoDB={(ti==null?"NULL":"OK len="+ti.text.Length)}");
        Debug.Log($"Resources FishQuizDB={(tq==null?"NULL":"OK len="+tq.text.Length)}");

        FishDatabase.Reload();
        var info = FishDatabase.GetInfo("mersin");
        Debug.Log($"Mersin info: name='{info?.displayName}' desc='{info?.shortDescription?.Substring(0, System.Math.Min(40, info?.shortDescription?.Length ?? 0))}'");
    }

    public static void CheckMersinTexts()
    {
        var go = GameObject.Find("mersin_balıgı_target/QuizCanvas/InfoPanel");
        if (go == null) { Debug.Log("mersin info panel not found"); return; }
        foreach (var t in go.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            Debug.Log($"Mersin InfoPanel '{t.name}' text='{t.text}'");
        }
    }

    public static void CheckTargets()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            var its = root.GetComponentsInChildren<Vuforia.ImageTargetBehaviour>(true);
            foreach (var it in its)
            {
                Debug.Log($"ImageTarget GO='{GetPath(it.transform)}' TargetName='{it.TargetName}'");
            }
        }
    }

    static string GetPath(Transform t)
    {
        if (t.parent == null) return t.name;
        return GetPath(t.parent) + "/" + t.name;
    }
}
