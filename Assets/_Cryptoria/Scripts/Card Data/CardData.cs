using UnityEngine;

// ──────────────────────────────────────────────────────────────────────────────
//  CardData.cs
//  Read-only definition data for every card in Cryptoria.
//  Never mutate at runtime — use CardRuntimeState for live mutable state.
//
//  SO Architecture role: authored asset dragged into collections, decks,
//  and spawn pools. Shared safely across scenes because it is never written to.
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>Stub for hero skill data — full design pending (GDD §5.3, §7.4).</summary>
[System.Serializable]
public class SkillData
{
    public string skillName;
    [TextArea(1, 2)]
    public string description;
    public int    manaCost;
    public int    cooldownTurns;
    // TODO: expand with effect type, damage formula, target type when hero system matures
}

[CreateAssetMenu(fileName = "card_new", menuName = "Cryptoria/Card Data")]
public class CardData : ScriptableObject
{
    // ── Identity ──────────────────────────────────────────────────────
    [Header("Identity")]
    public string   cardName;
    [TextArea(1, 3)]
    public string   description;
    public Sprite   cardArtwork;

    // ── Category & Archetype ─────────────────────────────────────────
    [Header("Category")]
    public CardCategory   cardCategory;
    public AllyArchetype  allyArchetype;    // Ally cards only
    public EnemyArchetype enemyArchetype;   // Enemy cards only — flag combos supported

    // ── Elements ──────────────────────────────────────────────────────
    [Header("Elements")]
    [Tooltip("Applied on every hit")]
    public ElementType primaryElement;
    [Tooltip("Dual-element cards only — applies on secondary trigger")]
    public ElementType secondaryElement;
    [Tooltip("Secondary element fires every N turns. 0 = never. (GDD §6.1)")]
    public int         secondaryTriggerEveryNTurns;

    // ── Core Stats ────────────────────────────────────────────────────
    [Header("Stats")]
    public int cardLevel;   // Drives combat scaling — not a flat damage value
    public int maxHealth;
    public int damage;
    public int shield;
    [Tooltip("Mana cost to play — Ally cards only (GDD §2.5)")]
    public int manaCost;

    // ── Status Effect Definition ──────────────────────────────────────
    [Header("Status Effect")]
    [Tooltip("Magnitude of the status effect applied on elemental hit")]
    public int statusMagnitude;
    [Tooltip("Duration in turns. Reapplication RESETS this value — does not stack (GDD §6.1)")]
    public int statusDuration;

    // ── Enemy Behaviour Flags ─────────────────────────────────────────
    [Header("Enemy Behaviour Flags")]
    [Tooltip("Must be killed first — player cannot interact with other lanes while alive (GDD §4.1)")]
    public bool isUnavoidable;
    [Tooltip("Bypasses hero immunity — attacks hero directly even while ally cards are alive (GDD §4.1)")]
    public bool isAggressive;
    [Tooltip("Can hit both front and back lanes")]
    public bool canAttackBackLane;
    [Tooltip("Survives overkill at 1 HP — never dies in a single hit (Gravekeeper, GDD §4.3)")]
    public bool survivesOverkill;
    [Tooltip("Moves to a new lane position every turn (Bloodthirsty Revenant, GDD §4.3)")]
    public bool changesLaneEachTurn;
    [Tooltip("Interval in turns for secondary status application (e.g. Skeleton Knight every 2 turns)")]
    public int  applyStatusEveryNTurns;

    // ── Spawner ───────────────────────────────────────────────────────
    [Header("Spawner")]
    [Tooltip("Cards deployed by Spawner enemies or Summoner ally on death (GDD §3.1, §4.1)")]
    public CardData[] spawnedCards;

    // ── Dungeon Pickup ────────────────────────────────────────────────
    [Header("Dungeon Pickup  (DungeonPickup category only)")]
    [Tooltip("What happens when this card is picked up in a dungeon row")]
    public PickupEffectType pickupEffectType;
    [Tooltip("Magnitude for Heal, Coins, SkillPoints, etc.")]
    public int              pickupEffectValue;

    // ── Utility Pile ──────────────────────────────────────────────────
    [Header("Utility Pile  (UtilityCard category only)")]
    [Tooltip("Which pile this card belongs to in the Dungeon Scroll loadout")]
    public UtilityPileType utilityPileType;

    // ── Hero Skills ───────────────────────────────────────────────────
    [Header("Hero Skills  (Hero category only)")]
    [Tooltip("Fixed Signature Skill — unique to this hero, cannot be changed (GDD §5.3)")]
    public SkillData signatureSkill;
    [Tooltip("Fixed secondary skill slot")]
    public SkillData secondarySkill;
    [Tooltip("Interchangeable skill — can be Passive or Active (GDD §5.3)")]
    public SkillData interchangeableSkill;
}
