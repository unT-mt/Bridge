#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;  // エディタ上でフィールドを無効にする
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;   // 元の状態に戻す
    }
}
#endif
