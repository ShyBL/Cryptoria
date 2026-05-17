// ──────────────────────────────────────────────────────────────────────────────
//  CryptoriaLists.cs
//  Observable SO-backed runtime lists for Cryptoria.
//  Pattern from SO Architecture Reference §5.
//
//  Usage:
//    [SerializeField] private ActiveAllyListSO _activeAllies;
//    _activeAllies.Add(allyRuntimeState);
//    _activeAllies.OnItemAdded   += OnAllyEntered;
//    _activeAllies.Modified      += RefreshFormationUI;
// ──────────────────────────────────────────────────────────────────────────────

// ── Abstract base ─────────────────────────────────────────────────────────────
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class ScriptableListSO<T> : ScriptableObject,
    ISerializationCallbackReceiver,
    IEnumerable<T>
{
    [Header("Info")]
    [TextArea(1, 3)] public string description;
    public string tag;

    [Header("Reset")]
    public ResetOn resetOn = ResetOn.OnSingleSceneLoad;

    [SerializeField] private List<T> _items = new List<T>();

    public IReadOnlyList<T> Items => _items;
    public int Count => _items.Count;

    public event System.Action<T>              OnItemAdded;
    public event System.Action<T>              OnItemRemoved;
    public event System.Action<IEnumerable<T>> OnItemsAdded;
    public event System.Action<IEnumerable<T>> OnItemsRemoved;
    public event System.Action                 Modified;
    public event System.Action                 OnCleared;

    public void OnAfterDeserialize() { if (resetOn != ResetOn.None) _items.Clear(); }
    public void OnBeforeSerialize() { }

    private void OnEnable()
    {
        if (resetOn == ResetOn.OnApplicationStart) _items.Clear();
        if (resetOn == ResetOn.OnSingleSceneLoad)
        {
            _items.Clear();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return;
        Clear();
    }

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

    public void AddRange(IEnumerable<T> range)
    {
        var added = new List<T>();
        foreach (var item in range) { if (_items.Contains(item)) continue; _items.Add(item); added.Add(item); }
        if (added.Count == 0) return;
        OnItemsAdded?.Invoke(added);
        Modified?.Invoke();
    }

    public void RemoveRange(IEnumerable<T> range)
    {
        var removed = new List<T>();
        foreach (var item in range) { if (!_items.Remove(item)) continue; removed.Add(item); }
        if (removed.Count == 0) return;
        OnItemsRemoved?.Invoke(removed);
        Modified?.Invoke();
    }

    public bool Contains(T item) => _items.Contains(item);
    public T this[int index]     => _items[index];

    public void Clear()
    {
        if (_items.Count == 0) return;
        _items.Clear();
        OnCleared?.Invoke();
        Modified?.Invoke();
    }

    public void NotifyChanged() => Modified?.Invoke();

    public IEnumerator<T> GetEnumerator()           => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()         => _items.GetEnumerator();
}