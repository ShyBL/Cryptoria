using UnityEditor;
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// Generic base drawer for all VariableReference types.
//
// Renders a single inspector row:
//   [■ Use Local] [value field  -or-  SO object field          ]
//
// When useLocal = true  → shows the inline value field
// When useLocal = false → shows the SO asset reference field
// ─────────────────────────────────────────────────────────────────────────────

public abstract class VariableReferenceDrawerBase : PropertyDrawer
{
    // Name of the serialized value field on the concrete reference type
    protected abstract string ValueFieldName { get; }
    // Name of the serialized SO field on the concrete reference type
    protected abstract string VariableFieldName { get; }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var useLocalProp   = property.FindPropertyRelative("useLocal");
        var valueProp      = property.FindPropertyRelative(ValueFieldName);
        var variableProp   = property.FindPropertyRelative(VariableFieldName);

        // Layout: [label] [toggle 18px] [value/SO field remainder]
        float toggleWidth = 18f;
        float spacing     = 4f;
        float labelWidth  = EditorGUIUtility.labelWidth;

        Rect labelRect  = new Rect(position.x, position.y, labelWidth, position.height);
        Rect toggleRect = new Rect(position.x + labelWidth, position.y, toggleWidth, position.height);
        Rect fieldRect  = new Rect(position.x + labelWidth + toggleWidth + spacing,
                                   position.y,
                                   position.width - labelWidth - toggleWidth - spacing,
                                   position.height);

        EditorGUI.LabelField(labelRect, label);

        EditorGUI.BeginChangeCheck();
        bool newUseLocal = EditorGUI.Toggle(toggleRect, useLocalProp.boolValue);
        if (EditorGUI.EndChangeCheck())
            useLocalProp.boolValue = newUseLocal;

        GUIContent fieldLabel = GUIContent.none;
        if (useLocalProp.boolValue)
            EditorGUI.PropertyField(fieldRect, valueProp, fieldLabel);
        else
            EditorGUI.PropertyField(fieldRect, variableProp, fieldLabel);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUIUtility.singleLineHeight;
}

// ─────────────────────────────────────────────────────────────────────────────
// Concrete drawers — one per reference type
// ─────────────────────────────────────────────────────────────────────────────

[CustomPropertyDrawer(typeof(FloatReference))]
public class FloatReferenceDrawer : VariableReferenceDrawerBase
{
    protected override string ValueFieldName    => "localValue";
    protected override string VariableFieldName => "variable";
}

[CustomPropertyDrawer(typeof(IntReference))]
public class IntReferenceDrawer : VariableReferenceDrawerBase
{
    protected override string ValueFieldName    => "localValue";
    protected override string VariableFieldName => "variable";
}

[CustomPropertyDrawer(typeof(BoolReference))]
public class BoolReferenceDrawer : VariableReferenceDrawerBase
{
    protected override string ValueFieldName    => "localValue";
    protected override string VariableFieldName => "variable";
}

[CustomPropertyDrawer(typeof(Vector3Reference))]
public class Vector3ReferenceDrawer : VariableReferenceDrawerBase
{
    protected override string ValueFieldName    => "localValue";
    protected override string VariableFieldName => "variable";
}
