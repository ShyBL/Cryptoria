using System.Collections.Generic;
using UnityEngine;

// ──────────────────────────────────────────────────────────────────────────────
//  CardRuntimeState.cs
//  Per-instance mutable state for a card currently in play.
//  Created on spawn / encounter start, discarded when the card dies.
//
//  SO Architecture role: this is NOT a ScriptableObject.
//  It is a plain C# class managed by the combat managers.
//  CardData (SO) = read-only authored definition.
//  CardRuntimeState = live mutable copy for this encounter.
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// One active status effect on a target.
/// Reset rule (GDD §6.1): reapplying the same ElementType resets RemainingDuration —
/// it does NOT create a second instance or increase magnitude.
/// </summary>
public class StatusEffectInstance
{
    public ElementType Type;
    public int         Magnitude;
    public int         RemainingDuration; // Decremented each turn — removed when it reaches 0

    public StatusEffectInstance(ElementType type, int magnitude, int duration)
    {
        Type              = type;
        Magnitude         = magnitude;
        RemainingDuration = duration;
    }
}

/// <summary>
/// Mutable runtime state for one card instance in play.
/// </summary>
public class CardRuntimeState
{
    // ── Definition (never mutated) ────────────────────────────────────
    public CardData Data { get; private set; }

    // ── Live stats ────────────────────────────────────────────────────
    public int CurrentHealth { get; set; }
    public int CurrentShield { get; set; }

    /// <summary>
    /// Column index (0, 1, 2) in the current row or lane grid.
    /// -1 = not yet assigned. Used by Bloodthirsty Revenant and lane-disable logic.
    /// </summary>
    public int CurrentLane { get; set; } = -1;

    /// <summary>
    /// Row index from the front (0 = front row). Used by DungeonScrollManager.
    /// </summary>
    public int RowIndex { get; set; } = -1;

    /// <summary>
    /// Active status effects on this card. One instance per ElementType maximum.
    /// </summary>
    public List<StatusEffectInstance> ActiveStatuses { get; } = new List<StatusEffectInstance>();

    // ── Constructor ───────────────────────────────────────────────────

    public CardRuntimeState(CardData data)
    {
        Data          = data;
        CurrentHealth = data.maxHealth;
        CurrentShield = data.shield;
    }

    // ── Combat ────────────────────────────────────────────────────────

    /// <summary>
    /// Apply raw damage — shield absorbs first, remainder reduces health.
    /// Does NOT enforce survivesOverkill — that check is the caller's responsibility
    /// (DungeonScrollManager / RoomCombatManager checks CardData.survivesOverkill after this).
    /// </summary>
    public void TakeDamage(int rawDamage)
    {
        int remaining = rawDamage;

        if (CurrentShield > 0)
        {
            int blocked    = Mathf.Min(CurrentShield, remaining);
            CurrentShield -= blocked;
            remaining     -= blocked;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - remaining);
    }

    /// <summary>
    /// Apply or reset a status effect (GDD §6.1 — reset, not stack).
    /// If the same ElementType is already active, resets its duration only.
    /// </summary>
    public void ApplyStatus(ElementType type, int magnitude, int duration)
    {
        StatusEffectInstance existing = ActiveStatuses.Find(s => s.Type == type);
        if (existing != null)
        {
            existing.RemainingDuration = duration; // Reset — do not stack
            return;
        }
        ActiveStatuses.Add(new StatusEffectInstance(type, magnitude, duration));
    }

    /// <summary>
    /// Tick all active status effects — decrement duration and remove expired ones.
    /// Returns the total health damage dealt this tick (for channel broadcast payload).
    /// Shield-only effects (Radiation) are handled separately by the combat manager.
    /// </summary>
    public int TickStatuses()
    {
        int totalHealthDamage = 0;

        for (int i = ActiveStatuses.Count - 1; i >= 0; i--)
        {
            StatusEffectInstance s = ActiveStatuses[i];

            // Apply per-turn effect — combat managers resolve complex interactions
            // (e.g. Corrosion shield disable, Bleed multiplier) using the type flag
            switch (s.Type)
            {
                case ElementType.Fire:
                    // Burn — damages both health and shield each round (GDD §6.2)
                    if (CurrentShield > 0)
                    {
                        int shieldHit = Mathf.Min(CurrentShield, s.Magnitude);
                        CurrentShield -= shieldHit;
                    }
                    totalHealthDamage += s.Magnitude;
                    break;

                case ElementType.Poison:
                    // Poison — damages health directly, bypasses shield (GDD §6.2)
                    totalHealthDamage += s.Magnitude;
                    break;

                case ElementType.Radiation:
                    // RadiationDrain — chips shield only, bypasses health (GDD §6.2)
                    int radHit = Mathf.Min(CurrentShield, s.Magnitude);
                    CurrentShield -= radHit;
                    break;

                case ElementType.Bleed:
                    // Bleed — damage multiplier applied by combat manager, not here
                    // TickStatuses just tracks duration; combat manager reads the flag
                    break;

                case ElementType.Ice:
                case ElementType.Corrosion:
                case ElementType.CursePurple:
                case ElementType.CurseGreen:
                    // These are state flags — effect resolved by combat manager each turn
                    break;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - totalHealthDamage);
            totalHealthDamage = 0; // Already applied to CurrentHealth above

            s.RemainingDuration--;
            if (s.RemainingDuration <= 0)
                ActiveStatuses.RemoveAt(i);
        }

        return 0; // Damage already applied inline above
    }

    public bool IsDead => CurrentHealth <= 0;

    public bool HasStatus(ElementType type) =>
        ActiveStatuses.Exists(s => s.Type == type);
}
