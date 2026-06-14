using UnityEngine;
using ARFishQuiz;

public static class VerifyAquariumWired
{
    public static void Execute()
    {
        var go = GameObject.Find("AquariumSystem/DrawingManager");
        var mgr = go.GetComponent<AquariumDrawingManager>();
        var so = new UnityEditor.SerializedObject(mgr);
        string[] fields = { "btnBlack","btnRed","btnGreen","btnBlue","btnYellow","btnOrange","btnPurple","btnCyan","btnEraser","btnClear","btnExit","btnSaveExit","drawingArea","drawingSurface","colorPreview","brushSizeSlider","rootPanel" };
        foreach (var f in fields)
        {
            var p = so.FindProperty(f);
            Debug.Log($"{f}: {(p?.objectReferenceValue == null ? "NULL" : p.objectReferenceValue.name)}");
        }
    }
}
