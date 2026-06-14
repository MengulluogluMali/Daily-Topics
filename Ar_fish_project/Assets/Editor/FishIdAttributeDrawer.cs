#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ARFishQuiz;

[CustomPropertyDrawer(typeof(FishIdAttribute))]
public class FishIdAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        // Tüm fishId'leri JSON'dan çek
        List<string> ids = FishDatabase.GetAllFishIds();
        if (ids == null || ids.Count == 0)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        // "(none)" seçeneği ekle
        var displayList = new List<string> { "(none)" };
        displayList.AddRange(ids);

        int currentIndex = 0;
        if (!string.IsNullOrEmpty(property.stringValue))
        {
            int idx = ids.IndexOf(property.stringValue);
            currentIndex = idx >= 0 ? idx + 1 : 0;
        }

        EditorGUI.BeginChangeCheck();
        int newIndex = EditorGUI.Popup(position, label.text, currentIndex, displayList.ToArray());
        if (EditorGUI.EndChangeCheck())
        {
            property.stringValue = newIndex == 0 ? string.Empty : ids[newIndex - 1];
        }

        // Reload butonu (küçük) - JSON yenilenmiş olabilir
        // (Opsiyonel) Şu an sadece dropdown göstermek yeterli.
    }
}
#endif
