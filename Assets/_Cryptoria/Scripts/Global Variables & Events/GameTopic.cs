/// <summary>
/// All channel topics for the Cryptoria SO event bus.
/// Add new values here as systems expand — never remove existing ones mid-project.
///
/// Naming convention: [System]_[Event]
/// </summary>
public enum GameTopic
{
    None,

    // ── Hero ──────────────────────────────────────────────────────────
    Hero_HealthChanged,
    Hero_ShieldChanged,
    Hero_Died,
    Hero_RunRestored,           // Hero fully healed after surviving a Dungeon Scroll run

    // ── Dungeon Scroll ────────────────────────────────────────────────
    DungeonScroll_RunStarted,
    DungeonScroll_RunWon,
    DungeonScroll_RunLost,
    DungeonScroll_RowCleared,
    DungeonScroll_CardDropped,  // Hero card dragged onto an enemy slot
    DungeonScroll_LaneDisabled, // A column has been skipped 4x consecutively (GDD §2.4)
    DungeonScroll_PickupApplied,

    // ── Room Combat ───────────────────────────────────────────────────
    RoomCombat_Started,
    RoomCombat_TurnStarted,
    RoomCombat_TurnEnded,
    RoomCombat_Won,
    RoomCombat_Lost,
    RoomCombat_AllyDied,
    RoomCombat_EnemyDied,
    RoomCombat_HeroExposed,     // All ally cards dead — hero now vulnerable

    // ── Status Effects ────────────────────────────────────────────────
    Status_Applied,
    Status_Expired,
    Status_Ticked,              // One tick of per-turn damage applied

    // ── Economy / Currencies ─────────────────────────────────────────
    Economy_CoinsChanged,
    Economy_SkillPointsChanged,
    Economy_ShardsChanged,
    Economy_FragmentsChanged,

    // ── Utility Piles ─────────────────────────────────────────────────
    UtilityPile_CardPushed,
    UtilityPile_CardPlayed,     // Player actively used a pile card
    UtilityPile_CardEvicted,    // Oldest card removed due to cap overflow

    // ── World Map ────────────────────────────────────────────────────
    WorldMap_ClusterUnlocked,
    WorldMap_SubIslandRevealed,

    // ── Tile Board ───────────────────────────────────────────────────
    TileBoard_TileEntered,
    TileBoard_TileCleared,

    // ── UI ────────────────────────────────────────────────────────────
    UI_DeckBuilderOpened,
    UI_DeckBuilderClosed,
    UI_LoadoutIncomplete        // Player tried to enter a tile without a configured loadout
}
