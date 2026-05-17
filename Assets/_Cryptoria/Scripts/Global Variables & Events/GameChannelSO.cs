using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Multi-topic event bus for Cryptoria.
/// One SO asset wires all systems together without direct MonoBehaviour references.
///
/// Usage — raise:
///   _channel.Raise(GameTopic.Hero_HealthChanged, heroRuntimeState);
///
/// Usage — subscribe (in OnEnable / OnDisable):
///   _channel.Subscribe(GameTopic.Hero_HealthChanged, OnHealthChanged);
///   _channel.Unsubscribe(GameTopic.Hero_HealthChanged, OnHealthChanged);
///
/// Data convention: pass the most specific object as payload so listeners
/// can cast only what they need. Null is valid for signal-only events.
/// </summary>
[CreateAssetMenu(fileName = "channel_cryptoria", menuName = "Cryptoria/Signals/Channel")]
public class GameChannelSO : ScriptableObject
{
    [Header("Info")]
    [TextArea(1, 3)]
    public string description;

    private readonly Dictionary<GameTopic, Action<object>> _listeners
        = new Dictionary<GameTopic, Action<object>>();

    // ── Public API ────────────────────────────────────────────────────

    public void Raise(GameTopic topic, object data = null)
    {
        if (_listeners.TryGetValue(topic, out Action<object> action))
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
        if (_listeners[topic] == null || _listeners[topic].GetInvocationList().Length == 0)
            _listeners.Remove(topic);
    }

    // Editor-only debug helper
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
}
