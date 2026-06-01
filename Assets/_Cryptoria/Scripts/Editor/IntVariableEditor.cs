using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IntVariableSO))]
public class IntVariableEditor : Editor
{
    private void OnEnable()  => EditorApplication.update += ForceRepaintDuringPlay;
    private void OnDisable() => EditorApplication.update -= ForceRepaintDuringPlay;

    private void ForceRepaintDuringPlay()
    {
        if (Application.isPlaying) Repaint();
    }

    public override void OnInspectorGUI()
    {
        var script = (IntVariableSO)target;

        // --- Description ---
        EditorGUI.BeginChangeCheck();
        string newDesc = EditorGUILayout.DelayedTextField(
            new GUIContent("Description", "Optional notes about this variable's purpose."),
            script.description);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Change Description");
            script.description = newDesc;
            EditorUtility.SetDirty(target);
        }

        // --- Tag ---
        EditorGUI.BeginChangeCheck();
        string newTag = EditorGUILayout.DelayedTextField(
            new GUIContent("Tag", "Optional tag for filtering. E.g. 'Player', 'Enemy', 'UI'"),
            script.tag);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Change Tag");
            script.tag = newTag;
            EditorUtility.SetDirty(target);
        }

        EditorGUILayout.Space();

        // --- Initial / Reset Value ---
        EditorGUI.BeginChangeCheck();
        int newInitial = EditorGUILayout.IntField("Initial (Reset) Value", script.initialValue);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Change Initial Value");
            script.initialValue = newInitial;
            EditorUtility.SetDirty(target);
        }

        // --- Reset Lifecycle ---
        EditorGUI.BeginChangeCheck();
        var newResetOn = (ResetOn)EditorGUILayout.EnumPopup("Reset On", script.resetOn);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Change Reset Mode");
            script.resetOn = newResetOn;
            EditorUtility.SetDirty(target);
        }

        // --- Clamping ---
        EditorGUI.BeginChangeCheck();
        bool newClamped = EditorGUILayout.Toggle("Is Clamped", script.isClamped);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Toggle Clamp");
            script.isClamped = newClamped;
            EditorUtility.SetDirty(target);
        }

        if (script.isClamped)
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            int newMin = EditorGUILayout.IntField("Min Value", script.minValue);
            int newMax = EditorGUILayout.IntField("Max Value", script.maxValue);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Change Clamp Range");
                script.minValue = newMin;
                script.maxValue = newMax;
                EditorUtility.SetDirty(target);
            }
            EditorGUI.indentLevel--;
        }

        // --- Debug Log ---
        EditorGUI.BeginChangeCheck();
        bool newDebug = EditorGUILayout.Toggle(
            new GUIContent("Debug Log Enabled", "Logs to console every time this value changes."),
            script.debugLogEnabled);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Toggle Debug Log");
            script.debugLogEnabled = newDebug;
            EditorUtility.SetDirty(target);
        }

        EditorGUILayout.Space();

        // --- Live Runtime Values ---
        GUI.enabled = false;
        EditorGUILayout.IntField("Current (Runtime) Value", script.currentValue);
        EditorGUILayout.IntField("Previous (Runtime) Value", script.previousValue);
        GUI.enabled = true;

        if (Application.isPlaying)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset to Initial Value"))
                script.ResetToInitial();
            if (GUILayout.Button("Notify Changed"))
                script.NotifyChanged();
            EditorGUILayout.EndHorizontal();
        }
    }
}
