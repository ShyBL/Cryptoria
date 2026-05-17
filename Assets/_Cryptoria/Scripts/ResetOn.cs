public enum ResetOn
{
    OnSingleSceneLoad,   // Default — resets on every full (non-additive) scene load
    OnApplicationStart,  // Resets once on boot — for cross-scene persistent state
    None                 // Never auto-resets — for runtime-created or authored static data
}