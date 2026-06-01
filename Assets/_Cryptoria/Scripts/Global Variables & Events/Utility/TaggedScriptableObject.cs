using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// TaggedScriptableObject
//
// Base class for all SO assets in this architecture that want a filterable tag.
// All Variable SOs, List SOs, and GameChannelSO should inherit from this
// once you migrate to a shared base (see migration note below).
//
// For now, because our variable SOs already inherit directly from ScriptableObject
// and Unity doesn't support multiple inheritance, the tag is added as a separate
// component-style field via the [SoTag] attribute approach described below.
//
// MIGRATION PATH (two options):
//
//   Option A — Shared base class (recommended for new types):
//     public class FloatVariableSO : TaggedScriptableObject, ISerializationCallbackReceiver
//     Lets you remove the [TextArea] description and string tag fields from
//     each individual SO and inherit them from here.
//
//   Option B — Stay flat, add tag field directly (current state):
//     Each SO already has `public string description`. Simply add:
//     public string tag;
//     to each SO manually. Use SoTagUtility (below) to filter by tag from code.
//
// This file provides Option A's base class ready to use, plus the utility
// filter methods that work regardless of which option you choose.
// ─────────────────────────────────────────────────────────────────────────────

public abstract class TaggedScriptableObject : ScriptableObject
{
    [Header("Info")]
    [TextArea(1, 3)]
    public string description;

    [Tooltip("Optional tag for filtering in editor utilities. E.g. 'Player', 'Enemy', 'UI'")]
    public string tag;
}
