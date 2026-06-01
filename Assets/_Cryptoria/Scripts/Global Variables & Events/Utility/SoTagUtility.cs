using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// ─────────────────────────────────────────────────────────────────────────────
// SoTagUtility
//
// Provides tag filtering across all SO assets that have a public `tag` field,
// regardless of whether they inherit TaggedScriptableObject or just declare
// the field directly (Option B migration path).
//
// Editor usage:
//   var playerVars = SoTagUtility.FindAllWithTag<FloatVariableSO>("Player");
//
// The filter methods work in both editor and runtime, but asset scanning
// via AssetDatabase is editor-only. At runtime, use the overload that
// accepts a pre-built list instead.
// ─────────────────────────────────────────────────────────────────────────────

public static class SoTagUtility
{
#if UNITY_EDITOR
    /// <summary>
    /// Finds all assets of type T in the project that have a matching tag.
    /// Editor-only. Uses AssetDatabase.
    /// </summary>
    public static List<T> FindAllWithTag<T>(string tag) where T : ScriptableObject
    {
        var results = new List<T>();
        if (string.IsNullOrEmpty(tag)) return results;

        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) continue;

            string assetTag = GetTag(asset);
            if (!string.IsNullOrEmpty(assetTag) &&
                assetTag.Equals(tag, System.StringComparison.OrdinalIgnoreCase))
            {
                results.Add(asset);
            }
        }
        return results;
    }

    /// <summary>
    /// Returns all unique tag strings found across all assets of type T.
    /// Useful for building filter dropdowns in editor windows.
    /// </summary>
    public static List<string> GetAllTags<T>() where T : ScriptableObject
    {
        var tags = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) continue;
            string t = GetTag(asset);
            if (!string.IsNullOrEmpty(t)) tags.Add(t);
        }
        return new List<string>(tags);
    }
#endif

    /// <summary>
    /// Filters a pre-built list by tag. Works at runtime.
    /// </summary>
    public static List<T> FilterByTag<T>(IEnumerable<T> source, string tag)
        where T : ScriptableObject
    {
        var results = new List<T>();
        if (string.IsNullOrEmpty(tag)) return results;
        foreach (var item in source)
        {
            string assetTag = GetTag(item);
            if (!string.IsNullOrEmpty(assetTag) &&
                assetTag.Equals(tag, System.StringComparison.OrdinalIgnoreCase))
                results.Add(item);
        }
        return results;
    }

    // ── Internal reflection helper ────────────────────────────────────────
    // Reads the public `tag` field from any SO that declares one,
    // regardless of inheritance chain.

    private static readonly System.Reflection.FieldInfo[] _cache
        = new System.Reflection.FieldInfo[0];

    private static readonly Dictionary<System.Type, System.Reflection.FieldInfo>
        _tagFieldCache = new Dictionary<System.Type, System.Reflection.FieldInfo>();

    private static string GetTag(ScriptableObject so)
    {
        if (so == null) return null;
        var type = so.GetType();

        if (!_tagFieldCache.TryGetValue(type, out var field))
        {
            field = type.GetField("tag",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);
            _tagFieldCache[type] = field;
        }

        return field?.GetValue(so) as string;
    }
}
