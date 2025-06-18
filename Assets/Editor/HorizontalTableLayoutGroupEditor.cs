using UnityEditor;
using UnityEditor.UI;           // for LayoutGroupEditor types
using UnityEngine.UI;

[CustomEditor(typeof(HorizontalTableLayoutGroup))]
[CanEditMultipleObjects]
public class HorizontalTableLayoutGroupEditor
    : HorizontalOrVerticalLayoutGroupEditor // preserves the built-in H/VLG UI
{
    SerializedProperty columnCountProp;
    SerializedProperty columnWidthsProp;

    protected override void OnEnable()
    {
        base.OnEnable();
        // match the private fields in your component:
        columnCountProp = serializedObject.FindProperty("columnCount");
        columnWidthsProp = serializedObject.FindProperty("columnWidths");
    }

    public override void OnInspectorGUI()
    {
        // draw the normal H/V layout-group controls:
        base.OnInspectorGUI();

        // now draw your custom fields:
        serializedObject.Update();
        EditorGUILayout.PropertyField(columnCountProp);
        EditorGUILayout.PropertyField(columnWidthsProp, true);
        serializedObject.ApplyModifiedProperties();
    }
}
