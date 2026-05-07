using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class QuizFixLayout
{
    public static void Execute()
    {
        GameObject imageTarget = GameObject.Find("ImageTarget");
        if (imageTarget == null) { Debug.LogError("ImageTarget yok"); return; }

        // ---- Canvas: Kartın üstünde havada, okunabilir yönde ----
        var canvas = imageTarget.transform.Find("QuizCanvas");
        if (canvas != null)
        {
            canvas.localPosition = new Vector3(-0.37f, 6f, -1f);
            // Canvas yüzeyi kullanıcıya (genelde +Y yönünden bakan kameraya) dönük olsun,
            // yazılar düzgün okunsun diye Y eksenini 180 döndürüyoruz.
            canvas.localRotation = Quaternion.Euler(-90f, 180f, 0f);
            canvas.localScale = Vector3.one * 0.012f;
        }

        // ---- QuizButton: Kartın önünde, havada, üstüne yazılı ----
        var btn = imageTarget.transform.Find("QuizButton");
        if (btn != null)
        {
            btn.localPosition = new Vector3(3.5f, 1.5f, 0f);
            btn.localScale = new Vector3(2.5f, 1.0f, 0.3f);
            var label = btn.Find("Label");
            if (label != null)
            {
                // Butonun üst yüzeyinde, yukarıdan bakan kullanıcıya dönük
                label.localPosition = new Vector3(0f, 0.55f, 0f);
                label.localRotation = Quaternion.Euler(90f, 0f, 0f);
                label.localScale = Vector3.one * 0.4f;
            }
        }

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[QuizFixLayout] Canvas ve buton yerleşimi düzeltildi.");
    }
}
