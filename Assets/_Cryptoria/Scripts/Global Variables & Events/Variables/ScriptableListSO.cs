using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic ScriptableObject-backed runtime list.
///
/// Events:
///   OnItemAdded / OnItemRemoved  — fires per item, carries the item as arg
///   Modified                     — fires on any mutation, no arg (cheapest subscriber hook)
///   OnItemsAdded / OnItemsRemoved — fires once after a batch Add/RemoveRange, carries the batch
///   OnCleared                    — fires when the list is cleared
///
/// ResetOn behaviour:
///   OnSingleSceneLoad — list is cleared on every full scene load (default)
///   OnApplicationStart — list is cleared once when the game starts
///   None — list is never auto-cleared; supports static authored data populated in the editor
///
/// Usage:
///   public GameObjectListSO activeEnemies;
///   activeEnemies.Add(gameObject);               // in enemy OnEnable
///   activeEnemies.Remove(gameObject);            // in enemy OnDisable
///   activeEnemies.OnItemAdded += HandleSpawn;
///   activeEnemies.Modified    += RefreshUI;
/// </summary>
public abstract class ScriptableListSO<T> : ScriptableObject,
    ISerializationCallbackReceiver,
    IEnumerable<T>
{
    [Header("Info")]
    [TextArea(1, 3)]
    public string description;
    [Tooltip("Optional tag for filtering. E.g. 'Player', 'Enemy', 'UI'")]
    public string tag;

    [Header("Reset")]
    public ResetOn resetOn = ResetOn.OnSingleSceneLoad;

    // Serialized backing store — populated in editor when ResetOn = None.
    // For runtime modes (SceneLoad / ApplicationStart) this is cleared on
    // OnEnable so the runtime list always starts empty regardless of any
    // editor-time contents.
    [SerializeField]
    private List<T> _items = new List<T>();

    public IReadOnlyList<T> Items => _items;
    public int Count => _items.Count;

    // --- Per-item events ---
    public event System.Action<T>              OnItemAdded;
    public event System.Action<T>              OnItemRemoved;

    // --- Batch events ---
    public event System.Action<IEnumerable<T>> OnItemsAdded;
    public event System.Action<IEnumerable<T>> OnItemsRemoved;

    // --- Aggregate events ---
    public event System.Action                 Modified;
    public event System.Action                 OnCleared;

    // ISerializationCallbackReceiver.
    // For runtime reset modes we clear here so the list is empty before
    // OnEnable fires. For ResetOn.None we do NOT clear — authored data must survive.
    public void OnAfterDeserialize()
    {
        if (resetOn != ResetOn.None)
            _items.Clear();
    }
    public void OnBeforeSerialize() { }

    private void OnEnable()
    {
        // Fast Play Mode (domain reload disabled) means OnAfterDeserialize
        // may NOT fire. Force a clear here for runtime modes to cover that path.
        if (resetOn == ResetOn.OnApplicationStart)
            _items.Clear();

        if (resetOn == ResetOn.OnSingleSceneLoad)
        {
            _items.Clear();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (mode == UnityEngine.SceneManagement.LoadSceneMode.Additive) return;
        Clear();
    }

    // ── Single-item mutations ──────────────────────────────────────────────

    public void Add(T item)
    {
        if (_items.Contains(item)) return;
        _items.Add(item);
        OnItemAdded?.Invoke(item);
        Modified?.Invoke();
    }

    public void Remove(T item)
    {
        if (!_items.Remove(item)) return;
        OnItemRemoved?.Invoke(item);
        Modified?.Invoke();
    }

    // ── Batch mutations ───────────────────────────────────────────────────

    /// <summary>
    /// Adds all items in the range, then fires OnItemsAdded once with the
    /// added subset (duplicates are skipped). Modified fires once at the end.
    /// </summary>
    public void AddRange(IEnumerable<T> range)
    {
        var added = new List<T>();
        foreach (var item in range)
        {
            if (_items.Contains(item)) continue;
            _items.Add(item);
            added.Add(item);
        }
        if (added.Count == 0) return;
        OnItemsAdded?.Invoke(added);
        Modified?.Invoke();
    }

    /// <summary>
    /// Removes all items in the range, then fires OnItemsRemoved once with
    /// the removed subset. Modified fires once at the end.
    /// </summary>
    public void RemoveRange(IEnumerable<T> range)
    {
        var removed = new List<T>();
        foreach (var item in range)
        {
            if (!_items.Remove(item)) continue;
            removed.Add(item);
        }
        if (removed.Count == 0) return;
        OnItemsRemoved?.Invoke(removed);
        Modified?.Invoke();
    }

    // ── Other ─────────────────────────────────────────────────────────────

    public bool Contains(T item) => _items.Contains(item);

    /// <summary>
    /// Direct index access — matches List<T> ergonomics.
    /// </summary>
    public T this[int index] => _items[index];

    public void Clear()
    {
        if (_items.Count == 0) return;
        _items.Clear();
        OnCleared?.Invoke();
        Modified?.Invoke();
    }

    /// <summary>
    /// Manually fire Modified without changing any value.
    /// Use when an item's internal state has changed in a way the list
    /// cannot detect (e.g. mutating a field on a class-type element).
    /// </summary>
    public void NotifyChanged() => Modified?.Invoke();

    // ── IEnumerable<T> ────────────────────────────────────────────────────

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
