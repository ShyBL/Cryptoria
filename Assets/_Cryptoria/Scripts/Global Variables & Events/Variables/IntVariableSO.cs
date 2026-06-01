using UnityEngine;

[CreateAssetMenu(fileName = "NewIntVariable", menuName = "Signals/Variables/Int")]
public class IntVariableSO : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("Info")]
    [TextArea(1, 3)]
    public string description;
    [Tooltip("Optional tag for filtering. E.g. 'Player', 'Enemy', 'UI'")]
    public string tag;

    [Header("Value")]
    public int initialValue;

    [Header("Reset")]
    public ResetOn resetOn = ResetOn.OnSingleSceneLoad;

    [Header("Clamping")]
    public bool isClamped = false;
    public int minValue = 0;
    public int maxValue = 100;

    [Header("Debug")]
    public bool debugLogEnabled = false;

    [System.NonSerialized]
    public int currentValue;

    [System.NonSerialized]
    public int previousValue;

    public event System.Action<int> OnValueChanged;

    public void OnAfterDeserialize() => currentValue = initialValue;
    public void OnBeforeSerialize() { }

    protected virtual void OnEnable()
    {
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

    public void Set(int value)
    {
        previousValue = currentValue;
        currentValue = isClamped ? Mathf.Clamp(value, minValue, maxValue) : value;
        NotifyChanged();
    }

    public void Add(int amount) => Set(currentValue + amount);

    public void ResetToInitial() => Set(initialValue);

    /// <summary>
    /// Manually fires OnValueChanged without changing the value.
    /// </summary>
    public virtual void NotifyChanged()
    {
        if (debugLogEnabled)
            Debug.Log($"[IntVariableSO] {name} → {currentValue}", this);
        OnValueChanged?.Invoke(currentValue);
    }
}
