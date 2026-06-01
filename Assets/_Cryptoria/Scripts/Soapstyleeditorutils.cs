// ─────────────────────────────────────────────────────────────────────────────
// CRYPTORIA — Editor utility file. Must live in an Editor/ folder.
//
// Shared drawing helpers used by the upgraded variable editors.
// Provides the two features SOAP's own inspector has that our canonical
// editors were missing:
//
//   1. DrawSubscriberList  — shows every object subscribed to OnValueChanged
//                            in play mode, with click-to-ping support.
//                            Matches SOAP's debugging panel exactly.
//
//   2. DrawValueBar        — visual progress bar between min and max.
//                            Only shown when isClamped is true.
// ─────────────────────────────────────────────────────────────────────────────
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class SoapStyleEditorUtils
{
    // ── Value bar ─────────────────────────────────────────────────────
    // Draws a coloured progress bar between min and max.
    // Call this after the current value field when isClamped is true.

    public static void DrawValueBar(float current, float min, float max, Color color)
    {
        float range = max - min;
        float fill  = range > 0 ? Mathf.Clamp01((current - min) / range) : 0f;

        Rect barOuter = EditorGUILayout.GetControlRect(false, 16f);
        // Indent to align with field values
        barOuter.x    += EditorGUIUtility.labelWidth;
        barOuter.width -= EditorGUIUtility.labelWidth;

        // Background
        EditorGUI.DrawRect(barOuter, new Color(0.18f, 0.18f, 0.18f));

        // Fill
        Rect fillRect  = barOuter;
        fillRect.width  = barOuter.width * fill;
        EditorGUI.DrawRect(fillRect, color);

        // Border
        Rect border = new Rect(barOuter.x - 1, barOuter.y - 1, barOuter.width + 2, barOuter.height + 2);
        DrawRectOutline(border, new Color(0.1f, 0.1f, 0.1f));

        // Percentage label
        string label = $"{current:0.##}  ({Mathf.RoundToInt(fill * 100f)}%)";
        GUI.Label(barOuter, "  " + label, EditorStyles.miniLabel);
    }

    public static void DrawValueBar(int current, int min, int max, Color color)
        => DrawValueBar((float)current, min, max, color);

    // ── Subscriber list ───────────────────────────────────────────────
    // Reads the invocation list from an event Action<T> via reflection
    // and draws each subscriber as a clickable row.
    //
    // Usage:
    //   SoapStyleEditorUtils.DrawSubscriberList(script.OnValueChanged, ref _subscriberFold);

    public static void DrawSubscriberList<T>(Action<T> evt, ref bool foldout)
    {
        if (!Application.isPlaying) return;

        Delegate[] subscribers = evt?.GetInvocationList();
        int count = subscribers != null ? subscribers.Length : 0;

        EditorGUILayout.Space(4);

        // Foldout header with count badge
        Rect headerRect = EditorGUILayout.GetControlRect(false, 20f);
        EditorGUI.DrawRect(headerRect, new Color(0.22f, 0.22f, 0.22f));
        headerRect.x += 4;
        foldout = EditorGUI.Foldout(headerRect, foldout,
            $"Subscribers  ({count})", true, EditorStyles.foldoutHeader);

        if (!foldout || subscribers == null || count == 0)
        {
            if (foldout && count == 0)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("No subscribers.", EditorStyles.miniLabel);
                EditorGUI.indentLevel--;
            }
            return;
        }

        EditorGUI.indentLevel++;

        foreach (Delegate d in subscribers)
        {
            // Try to get the target MonoBehaviour / Component
            UnityEngine.Object unityTarget = d.Target as UnityEngine.Object;
            string targetName  = unityTarget != null ? unityTarget.name : d.Target?.GetType().Name ?? "unknown";
            string methodName  = d.Method.Name;
            string typeName    = d.Target?.GetType().Name ?? "static";

            EditorGUILayout.BeginHorizontal();

            // Object icon + clickable label
            if (unityTarget != null)
            {
                GUIContent icon = EditorGUIUtility.ObjectContent(unityTarget, unityTarget.GetType());
                icon.text       = $"{targetName}  →  {typeName}.{methodName}";

                if (GUILayout.Button(icon, EditorStyles.label, GUILayout.Height(18f)))
                {
                    // Single click: ping in hierarchy/project
                    EditorGUIUtility.PingObject(unityTarget);
                }

                // Double-click workaround: selection button
                if (Event.current.type == EventType.MouseDown &&
                    Event.current.clickCount == 2 &&
                    GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    Selection.activeObject = unityTarget;
                    Event.current.Use();
                }
            }
            else
            {
                // Non-Unity target (plain C# class, lambda, etc.)
                GUILayout.Label($"[C#]  {typeName}.{methodName}", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUI.indentLevel--;
    }

    // ── Private helpers ───────────────────────────────────────────────

    private static void DrawRectOutline(Rect rect, Color color)
    {
        EditorGUI.DrawRect(new Rect(rect.x,                  rect.y,                  rect.width, 1),            color);
        EditorGUI.DrawRect(new Rect(rect.x,                  rect.y + rect.height - 1, rect.width, 1),           color);
        EditorGUI.DrawRect(new Rect(rect.x,                  rect.y,                  1,           rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.x + rect.width - 1, rect.y,                  1,           rect.height), color);
    }
}