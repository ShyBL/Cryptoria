// ── FloatVariableSO ───────────────────────────────────────────────────────────

/// <summary>
/// Observable float SO variable.
/// Used for: elemental resistance bars (0–1), damage multipliers, progress values.
/// </summary>
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "float_newVariable", menuName = "Cryptoria/Variables/Float")]
public class FloatVariableSO : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("Info")]
    [TextArea(1, 3)] public string description;
    public string tag;

    [Header("Value")]
    public float initialValue;

    [Header("Reset")]
    public ResetOn resetOn = ResetOn.OnSingleSceneLoad;

    [Header("Clamping")]
    public bool  isClamped = false;
    public float minValue  = 0f;
    public float maxValue  = 1f;

    [Header("Debug")]
    public bool debugLogEnabled = false;

    [System.NonSerialized] public float currentValue;
    [System.NonSerialized] public float previousValue;

    public event System.Action<float> OnValueChanged;

    public void OnAfterDeserialize() => currentValue = initialValue;
    public void OnBeforeSerialize() { }

    private void OnEnable()
    {
        if (resetOn == ResetOn.OnApplicationStart || resetOn == ResetOn.OnSingleSceneLoad)
            currentValue = initialValue;
        if (resetOn == ResetOn.OnSingleSceneLoad)
            SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return;
        currentValue = initialValue;
        NotifyChanged();
    }

    public void Set(float value)
    {
        previousValue = currentValue;
        currentValue  = isClamped ? Mathf.Clamp(value, minValue, maxValue) : value;
        NotifyChanged();
    }

    public void Add(float amount) => Set(currentValue + amount);
    public void ResetToInitial() => Set(initialValue);

    public void NotifyChanged()
    {
        if (debugLogEnabled)
            Debug.Log($"[FloatVariableSO] {name} → {currentValue}", this);
        OnValueChanged?.Invoke(currentValue);
    }
}