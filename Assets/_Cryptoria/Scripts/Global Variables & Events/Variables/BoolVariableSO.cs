using UnityEngine;

[CreateAssetMenu(fileName = "NewBoolVariable", menuName = "Signals/Variables/Bool")]
public class BoolVariableSO : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("Info")]
    [TextArea(1, 3)]
    public string description;
    [Tooltip("Optional tag for filtering. E.g. 'Player', 'Enemy', 'UI'")]
    public string tag;

    [Header("Value")]
    public bool initialValue;

    [Header("Reset")]
    public ResetOn resetOn = ResetOn.OnSingleSceneLoad;

    [Header("Debug")]
    public bool debugLogEnabled = false;

    [System.NonSerialized]
    public bool currentValue;

    [System.NonSerialized]
    public bool previousValue;

    public event System.Action<bool> OnValueChanged;

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

    public void Set(bool value)
    {
        previousValue = currentValue;
        currentValue = value;
        NotifyChanged();
    }

    public void Toggle() => Set(!currentValue);

    public void ResetToInitial() => Set(initialValue);

    /// <summary>
    /// Manually fires OnValueChanged without changing the value.
    /// </summary>
    public virtual void NotifyChanged()
    {
        if (debugLogEnabled)
            Debug.Log($"[BoolVariableSO] {name} → {currentValue}", this);
        OnValueChanged?.Invoke(currentValue);
    }
}
