using UnityEngine;

// ──────────────────────────────────────────────────────────────────────────────
//  PlayerProgressionSO.cs
//  Persistent cross-session player data — currencies, resistance bars.
//  resetOn = None on all child variables: this data survives scene loads.
//
//  GDD §5.1 (currencies), §6.3 (elemental resistance progression).
//
//  SO Architecture role: authored once as a project asset, wired into
//  managers via the Inspector. Any system can observe individual variables
//  (e.g. CoinUI wires to int_coins.OnValueChanged directly).
// ──────────────────────────────────────────────────────────────────────────────

[CreateAssetMenu(fileName = "player_progression", menuName = "Cryptoria/Player Progression")]
public class PlayerProgressionSO : ScriptableObject
{
    [Header("Currencies  (wire IntVariableSO assets with resetOn = None)")]
    [SerializeField] private IntVariableSO _coins;
    [SerializeField] private IntVariableSO _skillPointsCommon;
    [SerializeField] private IntVariableSO _skillPointsRare;
    [SerializeField] private IntVariableSO _skillPointsEpic;
    [SerializeField] private IntVariableSO _shards;
    [SerializeField] private IntVariableSO _fragmentsMemory;
    [SerializeField] private IntVariableSO _fragmentsLevel;
    [SerializeField] private IntVariableSO _fragmentsSkill;

    // ── Elemental resistance bars — one per ElementType ───────────────
    // Each bar accumulates damage taken of that element across runs.
    // Every 100 points earns +3% resistance (GDD §6.3).
    [Header("Elemental Resistance Damage Bars  (wire FloatVariableSO assets)")]
    [Tooltip("Accumulated damage taken per element — 100 points = +3% resistance")]
    [SerializeField] private FloatVariableSO _resistanceIce;
    [SerializeField] private FloatVariableSO _resistanceFire;
    [SerializeField] private FloatVariableSO _resistancePoison;
    [SerializeField] private FloatVariableSO _resistanceBleed;
    [SerializeField] private FloatVariableSO _resistanceRadiation;
    [SerializeField] private FloatVariableSO _resistanceCorrosion;
    [SerializeField] private FloatVariableSO _resistanceCursePurple;
    [SerializeField] private FloatVariableSO _resistanceCurseGreen;

    // ── Currency API ──────────────────────────────────────────────────

    public void AddCoins(int amount)          => _coins?.Add(amount);
    public void AddSkillPoints(int amount, SkillPointTier tier)
    {
        switch (tier)
        {
            case SkillPointTier.Common: _skillPointsCommon?.Add(amount); break;
            case SkillPointTier.Rare:   _skillPointsRare?.Add(amount);   break;
            case SkillPointTier.Epic:   _skillPointsEpic?.Add(amount);   break;
        }
    }
    public void AddShards(int amount)         => _shards?.Add(amount);
    public void AddFragmentsMemory(int amount) => _fragmentsMemory?.Add(amount);
    public void AddFragmentsLevel(int amount)  => _fragmentsLevel?.Add(amount);
    public void AddFragmentsSkill(int amount)  => _fragmentsSkill?.Add(amount);

    public int Coins              => _coins          != null ? _coins.currentValue          : 0;
    public int SkillPointsCommon  => _skillPointsCommon != null ? _skillPointsCommon.currentValue : 0;
    public int SkillPointsRare    => _skillPointsRare   != null ? _skillPointsRare.currentValue   : 0;
    public int SkillPointsEpic    => _skillPointsEpic   != null ? _skillPointsEpic.currentValue   : 0;

    // ── Resistance API ────────────────────────────────────────────────

    /// <summary>
    /// Record elemental damage received. Accumulates toward resistance thresholds.
    /// Every 100 points accumulated = +3% resistance (GDD §6.3).
    /// </summary>
    public void RecordElementalDamage(ElementType element, float amount)
    {
        FloatVariableSO bar = GetResistanceBar(element);
        if (bar == null) return;
        bar.Add(amount);
        // TODO: when bar crosses 100-point thresholds, award resistance upgrades
        // and notify channel (GameTopic.Economy_SkillPointsChanged or dedicated topic)
    }

    /// <summary>
    /// Returns the current resistance percentage (0–1) for the given element.
    /// Calculated as: floor(accumulatedDamage / 100) * 0.03, capped at a max TBD.
    /// </summary>
    public float GetResistancePercent(ElementType element)
    {
        FloatVariableSO bar = GetResistanceBar(element);
        if (bar == null) return 0f;
        return Mathf.Floor(bar.currentValue / 100f) * 0.03f;
    }

    private FloatVariableSO GetResistanceBar(ElementType element)
    {
        switch (element)
        {
            case ElementType.Ice:         return _resistanceIce;
            case ElementType.Fire:        return _resistanceFire;
            case ElementType.Poison:      return _resistancePoison;
            case ElementType.Bleed:       return _resistanceBleed;
            case ElementType.Radiation:   return _resistanceRadiation;
            case ElementType.Corrosion:   return _resistanceCorrosion;
            case ElementType.CursePurple: return _resistanceCursePurple;
            case ElementType.CurseGreen:  return _resistanceCurseGreen;
            default:
                Debug.LogWarning($"[PlayerProgressionSO] No resistance bar for element: {element}");
                return null;
        }
    }
}
