using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public static class DiagnoseAquariumButtons
{
    public static void Execute()
    {
        var canvas = GameObject.Find("AquariumSystem/DrawingManager/AquariumDrawCanvas");
        if (canvas == null) { Debug.LogError("AquariumDrawCanvas yok!"); return; }
        var btns = canvas.GetComponentsInChildren<Button>(true);
        Debug.Log($"Toplam buton: {btns.Length}");
        foreach (var b in btns)
        {
            int count = b.onClick.GetPersistentEventCount();
            string info = $"Buton: {b.name} | persistent listeners: {count}";
            for (int i = 0; i < count; i++)
            {
                info += $"\n  [{i}] target={b.onClick.GetPersistentTarget(i)} method={b.onClick.GetPersistentMethodName(i)}";
            }
            Debug.Log(info);
        }
    }
}
