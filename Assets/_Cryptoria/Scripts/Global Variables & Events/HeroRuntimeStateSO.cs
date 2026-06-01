using UnityEngine;

// ──────────────────────────────────────────────────────────────────────────────
//  HeroRuntimeStateSO.cs
//  Observable SO holding the hero's live state for the current run/encounter.
//
//  SO Architecture role: a runtime SO (resetOn = None) created per-session
//  by the encounter managers. Because it is an SO, any system (UI, audio,
//  persistence) can observe it via OnValueChanged without a direct reference
//  to DungeonScrollManager or RoomCombatManager.
//
//  GDD §7.2, §7.3:
//    - Dungeon Scroll: hero health carries between rows; run over at 0.
//    - Room Combat:    hero stats reset fresh each encounter.
//    - Post-run:       hero fully restored to maxHealth on survival.
// ──────────────────────────────────────────────────────────────────────────────

[CreateAssetMenu(fileName = "hero_runtimeState", menuName = "Cryptoria/Hero Runtime State")]
public class HeroRuntimeStateSO : ScriptableObject
{
    [Header("Definition — drag in the Hero CardData asset")]
    [SerializeField] private CardData _heroDefinition;

    // ── Observable stat variables ─────────────────────────────────────
    // These are separate SO variables so any UI or system can wire to
    // exactly the stat they care about via OnValueChanged.

    [Header("Runtime Variable Assets (create one per stat, wire here)")]
    [SerializeField] private IntVariableSO _health;
    [SerializeField] private IntVariableSO _shield;
    [SerializeField] private IntVariableSO _mana;

    // ── Read-only accessors ───────────────────────────────────────────
    
    public int CurrentHealth
    {
        get => _health != null ? _health.currentValue : 0;
        set { if (_health != null) _health.Set(value); }
    }

    public int CurrentShield
    {
        get => _shield != null ? _shield.currentValue : 0;
        set { if (_shield != null) _shield.Set(value); }
    }

    public int CurrentMana
    {
        get => _mana != null ? _mana.currentValue : 0;
        set { if (_mana != null) _mana.Set(value); }
    }

    public int MaxHealth => _heroDefinition != null ? _heroDefinition.maxHealth : 0;
    public int Damage    => _heroDefinition != null ? _heroDefinition.damage    : 0;

    // ── Lifecycle ─────────────────────────────────────────────────────

    /// <summary>
    /// Call at the start of every Dungeon Scroll run or Room Combat encounter.
    /// Seeds all stat variables from the hero's CardData definition.
    /// </summary>
    public void InitialiseForEncounter()
    {
        if (_heroDefinition == null)
        {
            Debug.LogError("[HeroRuntimeStateSO] HeroDefinition is not assigned.", this);
            return;
        }

        if (_health != null) _health.Set(_heroDefinition.maxHealth);
        if (_shield != null) _shield.Set(_heroDefinition.shield);
        if (_mana   != null) _mana.Set(0); // Mana starts at 0 each encounter
    }

    /// <summary>
    /// Called after a successful Dungeon Scroll run (GDD §2.4).
    /// Restores hero to full health — shield and mana also reset.
    /// </summary>
    public void RestoreAfterRun()
    {
        InitialiseForEncounter();
        Debug.Log("[HeroRuntimeStateSO] Hero fully restored after successful run.");
    }

    /// <summary>Apply damage — shield absorbs first, remainder hits health.</summary>
    public void TakeDamage(int rawDamage)
    {
        if (_heroDefinition == null) return;

        int remaining = rawDamage;

        if (CurrentShield > 0)
        {
            int blocked     = UnityEngine.Mathf.Min(CurrentShield, remaining);
            CurrentShield  -= blocked;
            remaining      -= blocked;
        }

        CurrentHealth = UnityEngine.Mathf.Max(0, CurrentHealth - remaining);
    }

    public bool IsDead => CurrentHealth <= 0;
}
