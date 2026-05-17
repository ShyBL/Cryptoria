using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Observable bool SO variable.
/// Used for: isGamePaused, isDeckBuilderOpen, loadoutConfigured, heroExposed, etc.
/// </summary>
[CreateAssetMenu(fileName = "bool_newVariable", menuName = "Cryptoria/Variables/Bool")]
public class BoolVariableSO : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("Info")]
    [TextArea(1, 3)] public string description;
    public string tag;

    [Header("Value")]
    public bool initialValue;

    [Header("Reset")]
    public ResetOn resetOn = ResetOn.OnSingleSceneLoad;

    [Header("Debug")]
    public bool debugLogEnabled = false;

    [System.NonSerialized] public bool currentValue;
    [System.NonSerialized] public bool previousValue;

    public event System.Action<bool> OnValueChanged;

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

    public void Set(bool value)
    {
        previousValue = currentValue;
        currentValue  = value;
        NotifyChanged();
    }

    public void Toggle() => Set(!currentValue);
    public void ResetToInitial() => Set(initialValue);

    public void NotifyChanged()
    {
        if (debugLogEnabled)
            Debug.Log($"[BoolVariableSO] {name} → {currentValue}", this);
        OnValueChanged?.Invoke(currentValue);
    }
}