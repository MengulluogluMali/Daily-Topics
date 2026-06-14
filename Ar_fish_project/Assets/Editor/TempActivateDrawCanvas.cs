using UnityEngine;
using UnityEditor;

public static class TempActivateDrawCanvas
{
    public static void Execute()
    {
        var go = GameObject.Find("AquariumSystem/DrawingManager/AquariumDrawCanvas");
        if (go != null)
        {
            // RootPanel is inactive by default
            var rp = go.transform.Find("RootPanel");
            if (rp != null) rp.gameObject.SetActive(true);
        }
    }
    public static void Deactivate()
    {
        var go = GameObject.Find("AquariumSystem/DrawingManager/AquariumDrawCanvas");
        if (go != null)
        {
            var rp = go.transform.Find("RootPanel");
            if (rp != null) rp.gameObject.SetActive(false);
        }
    }
}
