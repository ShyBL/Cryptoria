using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Observable integer SO variable.
/// Used for: hero health, hero shield, mana, coins, skill points, shards, fragments.
/// </summary>
[CreateAssetMenu(fileName = "int_newVariable", menuName = "Cryptoria/Variables/Int")]
public class IntVariableSO : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("Info")]
    [TextArea(1, 3)] public string description;
    public string tag;

    [Header("Value")]
    public int initialValue;

    [Header("Reset")]
    public ResetOn resetOn = ResetOn.OnSingleSceneLoad;

    [Header("Clamping")]
    public bool isClamped = false;
    public int  minValue  = 0;
    public int  maxValue  = 100;

    [Header("Debug")]
    public bool debugLogEnabled = false;

    [System.NonSerialized] public int currentValue;
    [System.NonSerialized] public int previousValue;

    public event System.Action<int> OnValueChanged;

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

    public void Set(int value)
    {
        previousValue = currentValue;
        currentValue  = isClamped ? UnityEngine.Mathf.Clamp(value, minValue, maxValue) : value;
        NotifyChanged();
    }

    public void Add(int amount) => Set(currentValue + amount);
    public void ResetToInitial() => Set(initialValue);

    public void NotifyChanged()
    {
        if (debugLogEnabled)
            Debug.Log($"[IntVariableSO] {name} → {currentValue}", this);
        OnValueChanged?.Invoke(currentValue);
    }
}