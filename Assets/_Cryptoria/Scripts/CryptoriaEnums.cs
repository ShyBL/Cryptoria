// ──────────────────────────────────────────────────────────────────────────────
//  CryptoriaEnums.cs
//  All domain enums for Cryptoria in one file.
//  Import this file; never duplicate enum definitions elsewhere.
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Top-level category for every card in the collection.
/// Determines which systems and fields are relevant on CardData.
/// </summary>
public enum CardCategory
{
    Ally,           // Used exclusively in room combat (Mage, Warrior, BlueMaze, etc.)
    Hero,           // The player's selected character — never an enemy (GDD §7.1)
    Enemy,          // All enemy cards — standard and named high-stat variants (GDD §4.3)
    DungeonPickup,  // Cards found in dungeon rows: coins, keys, potions, buffs
    UtilityCard     // Pre-packed loadout cards (Equipment / Buffs / Potions piles).
                    // Never appear in enemy rows — packed by the player before a run.
}

/// <summary>Ally card archetypes — room combat only (GDD §3.1).</summary>
public enum AllyArchetype
{
    None,
    Mage,       // x4 — buffs team, light combat
    Warrior,    // x4 — frontline tank
    BlueMaze,   // x2 — combo/exploit focused
    Cleric,     // x2 — pure healer
    Summoner,   // x2 — deploys cards on death
    Assassin    // x2 — glass cannon
}

/// <summary>
/// Enemy archetypes — can be combined as flags on a single enemy (GDD §4.1).
/// Example: Gargoyle = Elemental | Aggressive
/// </summary>
[System.Flags]
public enum EnemyArchetype
{
    None        = 0,
    Melee       = 1 << 0,  // Damages front lane only
    Ranged      = 1 << 1,  // Can hit front and back lanes
    Elemental   = 1 << 2,  // Deals elemental damage + applies status on every hit
    Unavoidable = 1 << 3,  // Must be killed first — player cannot pick other lanes
    Spawner     = 1 << 4,  // Summons cards into empty lanes (room combat)
    Support     = 1 << 5,  // Buffs/heals allied enemies each turn
    Aggressive  = 1 << 6   // Bypasses hero immunity — attacks hero directly each turn
}

/// <summary>
/// Elemental affinities — used as flags for dual-element cards (GDD §4.2, §6.1).
/// Each element maps to a specific status effect mechanic.
/// </summary>
[System.Flags]
public enum ElementType
{
    None        = 0,
    Ice         = 1 << 0,  // ❄️  Freeze       — slows target actions
    Fire        = 1 << 1,  // 🔥  Burn         — damages health + shield each round
    Poison      = 1 << 2,  // 🟢  Poison       — damages health directly (bypasses shield)
    Bleed       = 1 << 3,  // 🩸  Bleed        — damage multiplier for N turns
    Radiation   = 1 << 4,  // ☢️  RadiationDrain — chips shield only, bypasses health
    Corrosion   = 1 << 5,  // 🟤  Corrosion    — disables shield entirely for X turns
    CursePurple = 1 << 6,  // 🟣  Vulnerability — target receives more damage
    CurseGreen  = 1 << 7   // 🟢  Weakness     — target deals less damage
}

/// <summary>
/// The three utility pile types for Dungeon Scroll loadout (GDD §2.4).
/// Each pile is packed independently from the persistent collection.
/// </summary>
public enum UtilityPileType
{
    Equipment,  // Passive gear cards
    Buffs,      // Temporary stat/effect boosts
    Potions     // Consumable heals and recovery
}

/// <summary>
/// Effect type for DungeonPickup cards found in rows (GDD §2.4, Row Card Types).
/// UtilityCard category uses UtilityPileType instead of this enum.
/// </summary>
public enum PickupEffectType
{
    None,
    Heal,           // Heals hero immediately on pickup
    Coins,          // Added to persistent currency pool immediately
    SkillPoints,    // Added to persistent pool immediately
    Buff,           // Goes to top of Buffs utility pile
    AbilityBoost,   // Permanent or 2–3 row boost applied immediately
    Debuff,         // Debuff effect applied to hero (negative pickup)
    Collectable,    // Potion / damage modifier → top of matching utility pile
    Key,            // Generic room key
    DungeonKey,     // Unlocks deeper dungeon stages — cannot be lane-disabled (GDD §2.4)
    TreasureKey,    // Unlocks Treasure Room tile on island map — cannot be lane-disabled
    Treasure,       // High-value loot → persistent inventory
    RandomBuff      // Random buff, boost, debuff, or curse on pickup (Books)
}

/// <summary>
/// Tile types on the sub-island tile board (GDD §2.3).
/// Each type maps to a different encounter system.
/// </summary>
public enum TileType
{
    DungeonScroll,  // Solo hero run — required for island progression
    BoostRoom,      // Cave — skill boosts, potions, riddles, NPC offers
    TreasureRoom,   // Loot room — requires a Room Key to open
    SkillPointRoom, // NPC conversation — skill point rewards
    Bushes          // Random encounter — low stakes, high variance
}

/// <summary>Skill point tiers used at the Professor NPC (GDD §5.3).</summary>
public enum SkillPointTier
{
    Common,
    Rare,
    Epic
}

/// <summary>Run state for Dungeon Scroll — prevents input during async resolution.</summary>
public enum RunState
{
    Ongoing,
    Busy,   // Async combat/animation in progress — ignore new input
    Won,
    Lost
}
