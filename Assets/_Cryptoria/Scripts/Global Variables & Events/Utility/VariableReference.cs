using System;
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// VariableReference<TVariable, TValue>
//
// Allows a serialized field to be EITHER a hardcoded local value OR a
// reference to a ScriptableObject variable asset — toggled per-instance
// in the inspector.
//
// Usage in a MonoBehaviour:
//   [SerializeField] private FloatReference _speed;
//   float current = _speed.Value;          // reads local float or SO, same call
//
// When UseLocal = true  → reads/writes the local inline value (no SO needed)
// When UseLocal = false → reads/writes through the wired SO asset
//
// IMPORTANT: Setter on Value only works for the active mode:
//   - local mode  → sets localValue directly
//   - SO mode     → calls variable.Set(), fires OnValueChanged on the SO
// ─────────────────────────────────────────────────────────────────────────────

[Serializable]
public class FloatReference
{
    public bool useLocal = true;
    public float localValue;
    public FloatVariableSO variable;

    public float Value
    {
        get => useLocal ? localValue : (variable != null ? variable.currentValue : 0f);
        set
        {
            if (useLocal) localValue = value;
            else variable?.Set(value);
        }
    }

    // Subscribe to the SO's OnValueChanged when in variable mode.
    // No-op when in local mode (caller should guard or use the event conditionally).
    public event Action<float> OnValueChanged
    {
        add    { if (!useLocal && variable != null) variable.OnValueChanged += value; }
        remove { if (!useLocal && variable != null) variable.OnValueChanged -= value; }
    }
}

[Serializable]
public class IntReference
{
    public bool useLocal = true;
    public int localValue;
    public IntVariableSO variable;

    public int Value
    {
        get => useLocal ? localValue : (variable != null ? variable.currentValue : 0);
        set
        {
            if (useLocal) localValue = value;
            else variable?.Set(value);
        }
    }

    public event Action<int> OnValueChanged
    {
        add    { if (!useLocal && variable != null) variable.OnValueChanged += value; }
        remove { if (!useLocal && variable != null) variable.OnValueChanged -= value; }
    }
}

[Serializable]
public class BoolReference
{
    public bool useLocal = true;
    public bool localValue;
    public BoolVariableSO variable;

    public bool Value
    {
        get => useLocal ? localValue : (variable != null ? variable.currentValue : false);
        set
        {
            if (useLocal) localValue = value;
            else variable?.Set(value);
        }
    }

    public event Action<bool> OnValueChanged
    {
        add    { if (!useLocal && variable != null) variable.OnValueChanged += value; }
        remove { if (!useLocal && variable != null) variable.OnValueChanged -= value; }
    }
}

[Serializable]
public class Vector3Reference
{
    public bool useLocal = true;
    public Vector3 localValue;
    public Vector3VariableSO variable;

    public Vector3 Value
    {
        get => useLocal ? localValue : (variable != null ? variable.currentValue : Vector3.zero);
        set
        {
            if (useLocal) localValue = value;
            else variable?.Set(value);
        }
    }

    public event Action<Vector3> OnValueChanged
    {
        add    { if (!useLocal && variable != null) variable.OnValueChanged += value; }
        remove { if (!useLocal && variable != null) variable.OnValueChanged -= value; }
    }
}
