using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// --- Laufzeit-Attribut ---
public sealed class ReadOnlyFieldAttribute : PropertyAttribute { }

#if UNITY_EDITOR
// --- Editor-spezifische Darstellung ---
[CustomPropertyDrawer(typeof(ReadOnlyFieldAttribute))]
public class ReadOnlyFieldDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUI.GetPropertyHeight(property, label, true);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool prev = GUI.enabled;
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = prev;
    }
}
#endif
