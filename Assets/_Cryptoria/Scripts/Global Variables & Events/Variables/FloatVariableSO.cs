using UnityEngine;

[CreateAssetMenu(fileName = "NewFloatVariable", menuName = "Signals/Variables/Float")]
public class FloatVariableSO : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("Info")]
    [TextArea(1, 3)]
    public string description;
    [Tooltip("Optional tag for filtering. E.g. 'Player', 'Enemy', 'UI'")]
    public string tag;

    [Header("Value")]
    public float initialValue;

    [Header("Reset")]
    public ResetOn resetOn = ResetOn.OnSingleSceneLoad;

    [Header("Clamping")]
    public bool isClamped = false;
    public float minValue = 0f;
    public float maxValue = 100f;

    [Header("Debug")]
    public bool debugLogEnabled = false;

    [System.NonSerialized]
    public float currentValue;

    [System.NonSerialized]
    public float previousValue;

    public event System.Action<float> OnValueChanged;

    public void OnAfterDeserialize() => currentValue = initialValue;
    public void OnBeforeSerialize() { }

    protected virtual void OnEnable()
    {
        // Covers both normal boot and Fast Play Mode (domain reload disabled),
        // where OnAfterDeserialize may not fire between play sessions.
        if (resetOn == ResetOn.OnApplicationStart || resetOn == ResetOn.OnSingleSceneLoad)
            currentValue = initialValue;

        if (resetOn == ResetOn.OnSingleSceneLoad)
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected virtual void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (mode == UnityEngine.SceneManagement.LoadSceneMode.Additive) return;
        currentValue = initialValue;
        NotifyChanged();
    }

    public void Set(float value)
    {
        previousValue = currentValue;
        currentValue = isClamped ? Mathf.Clamp(value, minValue, maxValue) : value;
        NotifyChanged();
    }

    public void Add(float amount) => Set(currentValue + amount);

    public void ResetToInitial() => Set(initialValue);

    /// <summary>
    /// Manually fires OnValueChanged without changing the value.
    /// Use when a complex value type has been mutated field-by-field
    /// rather than reassigned through Set().
    /// </summary>
    public virtual void NotifyChanged()
    {
        if (debugLogEnabled)
            Debug.Log($"[FloatVariableSO] {name} → {currentValue}", this);
        OnValueChanged?.Invoke(currentValue);
    }
}
