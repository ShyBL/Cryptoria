using UnityEditor;
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// Generic base editor for all ScriptableListSO<T> types.
// ─────────────────────────────────────────────────────────────────────────────

public abstract class ScriptableListEditorBase<T> : Editor
{
    private void OnEnable()  => EditorApplication.update += ForceRepaintDuringPlay;
    private void OnDisable() => EditorApplication.update -= ForceRepaintDuringPlay;

    private void ForceRepaintDuringPlay()
    {
        if (Application.isPlaying) Repaint();
    }

    protected abstract ScriptableListSO<T> GetTarget();

    public override void OnInspectorGUI()
    {
        var list = GetTarget();

        // --- Description ---
        EditorGUI.BeginChangeCheck();
        string newDesc = EditorGUILayout.DelayedTextField(
            new GUIContent("Description", "Optional notes about this list's purpose."),
            list.description);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Change Description");
            list.description = newDesc;
            EditorUtility.SetDirty(target);
        }

        // --- Tag ---
        EditorGUI.BeginChangeCheck();
        string newTag = EditorGUILayout.DelayedTextField(
            new GUIContent("Tag", "Optional tag for filtering. E.g. 'Player', 'Enemy', 'UI'"),
            list.tag);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Change Tag");
            list.tag = newTag;
            EditorUtility.SetDirty(target);
        }

        EditorGUILayout.Space();

        // --- Reset Lifecycle ---
        EditorGUI.BeginChangeCheck();
        var newResetOn = (ResetOn)EditorGUILayout.EnumPopup(
            new GUIContent("Reset On", "None = static data, never auto-cleared."),
            list.resetOn);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Change Reset Mode");
            list.resetOn = newResetOn;
            EditorUtility.SetDirty(target);
        }

        EditorGUILayout.Space();

        // --- Static data hint ---
        if (list.resetOn == ResetOn.None && !Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Reset On = None: this list acts as static authored data. " +
                "Populate items here; they will not be cleared at runtime.",
                MessageType.Info);
        }

        // --- Live Contents (play mode only for runtime lists) ---
        if (!Application.isPlaying && list.resetOn != ResetOn.None)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to see runtime contents.", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField($"Count: {list.Count}", EditorStyles.boldLabel);

        if (list.Count == 0)
        {
            EditorGUILayout.HelpBox("List is empty.", MessageType.None);
        }
        else
        {
            GUI.enabled = false;
            foreach (var item in list)
                EditorGUILayout.LabelField(item != null ? item.ToString() : "(null)");
            GUI.enabled = true;
        }

        if (Application.isPlaying)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear List"))
                list.Clear();
            if (GUILayout.Button("Notify Changed"))
                list.NotifyChanged();
            EditorGUILayout.EndHorizontal();
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Concrete editors
// ─────────────────────────────────────────────────────────────────────────────

[CustomEditor(typeof(GameObjectListSO))]
public class GameObjectListEditor : ScriptableListEditorBase<UnityEngine.GameObject>
{
    protected override ScriptableListSO<UnityEngine.GameObject> GetTarget()
        => (GameObjectListSO)target;
}

[CustomEditor(typeof(TransformListSO))]
public class TransformListEditor : ScriptableListEditorBase<UnityEngine.Transform>
{
    protected override ScriptableListSO<UnityEngine.Transform> GetTarget()
        => (TransformListSO)target;
}

[CustomEditor(typeof(StringListSO))]
public class StringListEditor : ScriptableListEditorBase<string>
{
    protected override ScriptableListSO<string> GetTarget()
        => (StringListSO)target;
}

[CustomEditor(typeof(IntListSO))]
public class IntListEditor : ScriptableListEditorBase<int>
{
    protected override ScriptableListSO<int> GetTarget()
        => (IntListSO)target;
}
