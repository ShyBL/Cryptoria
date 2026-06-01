using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// RuntimeVariableUtils
//
// Utility for creating ScriptableObject variable and list instances at runtime,
// without needing pre-authored project assets. Useful for per-entity state:
// enemy HP bars, per-prophet devotion counters, per-shrine capacity, etc.
//
// Usage — blank instance:
//   var hp = RuntimeVariableUtils.Create<FloatVariableSO>("Enemy_01_HP");
//   hp.initialValue = 100f;
//   hp.resetOn = ResetOn.None;   // runtime instances should never auto-reset
//   hp.OnAfterDeserialize();     // seed currentValue from initialValue
//
// Usage — deep copy from a template asset:
//   [SerializeField] private FloatVariableSO _hpTemplate;  // drag in inspector
//   var hp = RuntimeVariableUtils.CreateFrom(_hpTemplate, "Enemy_01_HP");
//   // hp has all settings from the template, but is a separate instance
//
// Lifecycle:
//   Runtime SOs are NOT assets — they live in memory only and are garbage-
//   collected when all references are dropped. Explicitly destroy them when
//   the owning entity is destroyed to avoid leaks with external subscribers:
//   RuntimeVariableUtils.Destroy(hp);
// ─────────────────────────────────────────────────────────────────────────────

public static class RuntimeVariableUtils
{
    // ── Generic create ────────────────────────────────────────────────────

    /// <summary>
    /// Creates a blank runtime instance of any ScriptableObject type.
    /// Does NOT add it to the AssetDatabase — lives in memory only.
    /// </summary>
    public static T Create<T>(string instanceName) where T : ScriptableObject
    {
        var instance = ScriptableObject.CreateInstance<T>();
        instance.name = instanceName;
        return instance;
    }

    /// <summary>
    /// Deep-copies a template SO asset into a new runtime instance.
    /// All serialized fields (initialValue, resetOn, isClamped, etc.) are
    /// copied from the template. The instance is independent — changes to
    /// one do not affect the other.
    /// </summary>
    public static T CreateFrom<T>(T template, string instanceName) where T : ScriptableObject
    {
        if (template == null)
        {
            Debug.LogWarning($"[RuntimeVariableUtils] Template is null. Creating blank {typeof(T).Name} instead.");
            return Create<T>(instanceName);
        }

        // JsonUtility round-trip is the safest cross-platform deep copy for SOs.
        // It copies all [SerializeField] fields without reflection.
        var instance = ScriptableObject.CreateInstance<T>();
        instance.name = instanceName;
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(template), instance);
        return instance;
    }

    // ── Typed convenience overloads ───────────────────────────────────────
    // These seed currentValue from initialValue immediately so the instance
    // is ready to use without a manual OnAfterDeserialize() call.

    public static FloatVariableSO CreateFloat(string name, float initialValue = 0f,
        bool isClamped = false, float min = 0f, float max = 100f)
    {
        var v = Create<FloatVariableSO>(name);
        v.initialValue = initialValue;
        v.resetOn      = ResetOn.None;
        v.isClamped    = isClamped;
        v.minValue     = min;
        v.maxValue     = max;
        v.OnAfterDeserialize();
        return v;
    }

    public static IntVariableSO CreateInt(string name, int initialValue = 0,
        bool isClamped = false, int min = 0, int max = 100)
    {
        var v = Create<IntVariableSO>(name);
        v.initialValue = initialValue;
        v.resetOn      = ResetOn.None;
        v.isClamped    = isClamped;
        v.minValue     = min;
        v.maxValue     = max;
        v.OnAfterDeserialize();
        return v;
    }

    public static BoolVariableSO CreateBool(string name, bool initialValue = false)
    {
        var v = Create<BoolVariableSO>(name);
        v.initialValue = initialValue;
        v.resetOn      = ResetOn.None;
        v.OnAfterDeserialize();
        return v;
    }

    public static Vector3VariableSO CreateVector3(string name, Vector3 initialValue = default)
    {
        var v = Create<Vector3VariableSO>(name);
        v.initialValue = initialValue;
        v.resetOn      = ResetOn.None;
        v.OnAfterDeserialize();
        return v;
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────

    /// <summary>
    /// Destroys a runtime SO instance and releases its memory.
    /// Call this when the owning entity is destroyed (OnDestroy).
    /// Failure to call this when external objects hold event subscriptions
    /// will cause subscriber leaks until the SO is eventually GC'd.
    /// </summary>
    public static void Destroy(ScriptableObject instance)
    {
        if (instance == null) return;
        Object.Destroy(instance);
    }
}
