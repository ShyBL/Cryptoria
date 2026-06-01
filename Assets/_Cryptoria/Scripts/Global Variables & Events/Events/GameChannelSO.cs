using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGameChannel", menuName = "Signals/Channel")]
public class GameChannelSO : ScriptableObject
{
    [Header("Info")]
    [TextArea(1, 3)]
    public string description;
    [Tooltip("Optional tag for filtering. E.g. 'Player', 'Enemy', 'UI'")]
    public string tag;

    private readonly Dictionary<GameTopic, Action<object>> _listeners
        = new Dictionary<GameTopic, Action<object>>();

    /// <summary>
    /// Returns a snapshot of active topic → listener count pairs.
    /// Only topics with at least one listener are included.
    /// </summary>
    public Dictionary<GameTopic, int> GetListenerCounts()
    {
        var counts = new Dictionary<GameTopic, int>();
        foreach (var kvp in _listeners)
        {
            int count = kvp.Value.GetInvocationList().Length;
            if (count > 0) counts[kvp.Key] = count;
        }
        return counts;
    }

    public void Raise(GameTopic topic, object data = null)
    {
        if (_listeners.TryGetValue(topic, out var action))
            action.Invoke(data);
    }

    public void Subscribe(GameTopic topic, Action<object> listener)
    {
        if (!_listeners.ContainsKey(topic))
            _listeners[topic] = delegate { };
        _listeners[topic] += listener;
    }

    public void Unsubscribe(GameTopic topic, Action<object> listener)
    {
        if (!_listeners.ContainsKey(topic)) return;
        _listeners[topic] -= listener;

        // Clean up the key when no listeners remain so GetListenerCounts()
        // doesn't report phantom zero-count entries.
        if (_listeners[topic] == null || _listeners[topic].GetInvocationList().Length == 0)
            _listeners.Remove(topic);
    }
}
