using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using ARFishQuiz;

public static class DiagPisi
{
    public static void Execute()
    {
        var sb = new StringBuilder();

        // 1) Sahnedeki gerçek target adlarını ve char kodlarını yaz
        var managers = Object.FindObjectsByType<QuizManager>(FindObjectsInactive.Include);
        foreach (var qm in managers)
        {
            var target = qm.transform.parent;
            if (target == null) continue;
            string n = target.name;
            if (n.Contains("pisi"))
            {
                sb.AppendLine($"SCENE NAME: '{n}'");
                sb.AppendLine("  CHARS: " + DumpChars(n));
                sb.AppendLine("  SAVE PATH: " + AquariumDrawingManager.GetSavePath(n));
                sb.AppendLine("  FILE EXISTS: " + File.Exists(AquariumDrawingManager.GetSavePath(n)));
            }
        }

        // 2) Viewer FishOrder içindeki pisi girdisini yaz
        string viewerPisi = "pisi_balıgı_target";
        sb.AppendLine($"VIEWER FishOrder pisi: '{viewerPisi}'");
        sb.AppendLine("  CHARS: " + DumpChars(viewerPisi));
        sb.AppendLine("  SAVE PATH: " + AquariumDrawingManager.GetSavePath(viewerPisi));

        // 3) Kaydedilmiş tüm PNG dosyalarını listele
        string folder = Path.Combine(Application.persistentDataPath, "Aquarium");
        sb.AppendLine("SAVE FOLDER: " + folder);
        if (Directory.Exists(folder))
        {
            foreach (var f in Directory.GetFiles(folder, "*.png"))
                sb.AppendLine("  PNG: " + Path.GetFileName(f));
        }
        else sb.AppendLine("  (folder yok)");

        Debug.Log("[DiagPisi]\n" + sb.ToString());
    }

    private static string DumpChars(string s)
    {
        var sb = new StringBuilder();
        foreach (char c in s)
            sb.Append($"{c}(U+{((int)c):X4}) ");
        return sb.ToString();
    }
}
