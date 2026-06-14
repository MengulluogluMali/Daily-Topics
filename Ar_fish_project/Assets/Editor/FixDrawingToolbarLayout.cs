using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ARFishQuizEditor
{
    /// <summary>
    /// Drawing toolbar'ı mobil portreye sığacak şekilde 2 satıra böler:
    /// üst satır: renkler + silgi
    /// alt satır: fırça boyutu + temizle + çıkış + kaydet ve çık
    /// Menü: Tools/Aquarium/Fix Drawing Toolbar Layout
    /// </summary>
    public static class FixDrawingToolbarLayout
    {
        [MenuItem("Tools/Aquarium/Fix Drawing Toolbar Layout")]
        public static void Fix()
        {
            var toolbar = GameObject.Find("AquariumSystem/DrawingManager/AquariumDrawCanvas/RootPanel/Toolbar");
            if (toolbar == null)
            {
                Debug.LogWarning("[FixToolbar] Toolbar bulunamadı.");
                return;
            }

            // 1) Toolbar'ı genişlet (alt %22) ve LayoutGroup'unu Vertical yap
            var tbRT = toolbar.GetComponent<RectTransform>();
            tbRT.anchorMin = new Vector2(0f, 0f);
            tbRT.anchorMax = new Vector2(1f, 0.22f);
            tbRT.offsetMin = Vector2.zero;
            tbRT.offsetMax = Vector2.zero;
            tbRT.pivot = new Vector2(0.5f, 0.5f);
            tbRT.anchoredPosition = Vector2.zero;
            tbRT.sizeDelta = Vector2.zero;

            // Eski layoutgroup'ları temizle
            var oldH = toolbar.GetComponent<HorizontalLayoutGroup>();
            if (oldH != null) Object.DestroyImmediate(oldH);
            var oldV = toolbar.GetComponent<VerticalLayoutGroup>();
            if (oldV != null) Object.DestroyImmediate(oldV);

            var vLayout = toolbar.AddComponent<VerticalLayoutGroup>();
            vLayout.spacing = 8f;
            vLayout.padding = new RectOffset(10, 10, 8, 8);
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = true;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = true;

            // 2) Mevcut çocukları topla
            var children = new List<Transform>();
            foreach (Transform c in toolbar.transform) children.Add(c);

            // 3) Eski Row1/Row2 varsa sil (yeni baştan oluşturacağız)
            foreach (var c in children)
            {
                if (c.name == "Row1" || c.name == "Row2")
                {
                    // İçindeki gerçek butonları toolbar altına taşı, sonra Row'u sil
                    var inner = new List<Transform>();
                    foreach (Transform g in c) inner.Add(g);
                    foreach (var g in inner) g.SetParent(toolbar.transform, false);
                    Object.DestroyImmediate(c.gameObject);
                }
            }

            // Yenden topla
            children.Clear();
            foreach (Transform c in toolbar.transform) children.Add(c);

            // 4) Row1/Row2 oluştur
            var row1 = CreateRow(toolbar.transform, "Row1");
            var row2 = CreateRow(toolbar.transform, "Row2");

            string[] row1Names = { "Black", "Red", "Green", "Blue", "Yellow", "Orange", "Purple", "Cyan", "Eraser" };
            string[] row2Names = { "BrushSlider", "ColorPreview", "Clear", "Exit", "SaveExit" };

            // Tanımlanan sıraya göre yerleştirelim
            foreach (var n in row1Names)
            {
                var c = children.Find(x => x != null && x.name == n);
                if (c != null) c.SetParent(row1, false);
            }
            foreach (var n in row2Names)
            {
                var c = children.Find(x => x != null && x.name == n);
                if (c != null) c.SetParent(row2, false);
            }
            // Tanımlanmayan kalan elemanlar varsa Row1'e at
            foreach (var c in children)
            {
                if (c == null) continue;
                if (c.parent == toolbar.transform && c != row1 && c != row2)
                    c.SetParent(row1, false);
            }

            // 5) Row1 / Row2 LayoutElement
            EnsureRowLayoutElement(row1.gameObject, 110);
            EnsureRowLayoutElement(row2.gameObject, 110);

            // 6) Row1 elemanları (renkler + silgi) — flexible width = 1
            foreach (Transform c in row1)
            {
                ResetChildRect(c);
                var le = c.GetComponent<LayoutElement>();
                if (le == null) le = c.gameObject.AddComponent<LayoutElement>();
                le.minWidth = 50;
                le.preferredWidth = 100;
                le.flexibleWidth = 1;
                le.flexibleHeight = 1;
                le.minHeight = 80;
                le.preferredHeight = 100;
            }

            // 7) Row2 elemanları için boyutlar (action buttons)
            foreach (Transform c in row2)
            {
                ResetChildRect(c);
                var le = c.GetComponent<LayoutElement>();
                if (le == null) le = c.gameObject.AddComponent<LayoutElement>();
                le.minHeight = 80;
                le.preferredHeight = 100;
                switch (c.name)
                {
                    case "BrushSlider":
                        le.minWidth = 180; le.preferredWidth = 220; le.flexibleWidth = 1; break;
                    case "ColorPreview":
                        le.minWidth = 70; le.preferredWidth = 80; le.flexibleWidth = 0; break;
                    case "Clear":
                        le.minWidth = 130; le.preferredWidth = 150; le.flexibleWidth = 0; break;
                    case "Exit":
                        le.minWidth = 130; le.preferredWidth = 150; le.flexibleWidth = 0; break;
                    case "SaveExit":
                        le.minWidth = 200; le.preferredWidth = 230; le.flexibleWidth = 0; break;
                    default:
                        le.preferredWidth = 100; le.flexibleWidth = 1; break;
                }
            }

            // 8) Title ve drawing surface alanlarını yeniden konumlandır (toolbar 22% kapladığı için)
            var surface = GameObject.Find("AquariumSystem/DrawingManager/AquariumDrawCanvas/RootPanel/DrawingSurface");
            if (surface != null)
            {
                var srt = surface.GetComponent<RectTransform>();
                srt.anchorMin = new Vector2(0.04f, 0.24f);
                srt.anchorMax = new Vector2(0.96f, 0.92f);
                srt.offsetMin = Vector2.zero;
                srt.offsetMax = Vector2.zero;
                srt.pivot = new Vector2(0.5f, 0.5f);
                srt.anchoredPosition = Vector2.zero;
                srt.sizeDelta = Vector2.zero;
            }

            var title = GameObject.Find("AquariumSystem/DrawingManager/AquariumDrawCanvas/RootPanel/Title");
            if (title != null)
            {
                var trt = title.GetComponent<RectTransform>();
                trt.anchorMin = new Vector2(0f, 0.93f);
                trt.anchorMax = new Vector2(1f, 1f);
                trt.offsetMin = Vector2.zero;
                trt.offsetMax = Vector2.zero;
                trt.pivot = new Vector2(0.5f, 0.5f);
                trt.anchoredPosition = Vector2.zero;
                trt.sizeDelta = Vector2.zero;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[FixToolbar] Drawing toolbar 2 satıra dönüştürüldü.");
        }

        private static void ResetChildRect(Transform c)
        {
            var rt = c.GetComponent<RectTransform>();
            if (rt == null) return;
            // Layout group child'ı için anchor önemli değil ama temizleyelim
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }

        private static void EnsureRowLayoutElement(GameObject row, float prefHeight)
        {
            var le = row.GetComponent<LayoutElement>();
            if (le == null) le = row.AddComponent<LayoutElement>();
            le.minHeight = 80;
            le.preferredHeight = prefHeight;
            le.flexibleHeight = 1;
            le.flexibleWidth = 1;
        }

        private static Transform CreateRow(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 6f;
            h.padding = new RectOffset(4, 4, 0, 0);
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = true;
            h.childControlWidth = true;
            h.childControlHeight = true;
            return go.transform;
        }
    }
}
