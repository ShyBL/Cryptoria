using UnityEngine;

[CreateAssetMenu(fileName = "NewVector3Variable", menuName = "Signals/Variables/Vector3")]
public class Vector3VariableSO : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("Info")]
    [TextArea(1, 3)]
    public string description;
    [Tooltip("Optional tag for filtering. E.g. 'Player', 'Enemy', 'UI'")]
    public string tag;

    [Header("Value")]
    public Vector3 initialValue;

    [Header("Reset")]
    public ResetOn resetOn = ResetOn.OnSingleSceneLoad;

    [Header("Debug")]
    public bool debugLogEnabled = false;

    [System.NonSerialized]
    public Vector3 currentValue;

    [System.NonSerialized]
    public Vector3 previousValue;

    public event System.Action<Vector3> OnValueChanged;

    public void OnAfterDeserialize() => currentValue = initialValue;
    public void OnBeforeSerialize() { }

    private void OnEnable()
    {
        if (resetOn == ResetOn.OnApplicationStart || resetOn == ResetOn.OnSingleSceneLoad)
            currentValue = initialValue;

        if (resetOn == ResetOn.OnSingleSceneLoad)
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (mode == UnityEngine.SceneManagement.LoadSceneMode.Additive) return;
        currentValue = initialValue;
        NotifyChanged();
    }

    public void Set(Vector3 value)
    {
        previousValue = currentValue;
        currentValue = value;
        NotifyChanged();
    }

    public void Add(Vector3 amount) => Set(currentValue + amount);

    public void ResetToInitial() => Set(initialValue);

    /// <summary>
    /// Manually fires OnValueChanged without changing the value.
    /// </summary>
    public void NotifyChanged()
    {
        if (debugLogEnabled)
            Debug.Log($"[Vector3VariableSO] {name} → {currentValue}", this);
        OnValueChanged?.Invoke(currentValue);
    }
}
