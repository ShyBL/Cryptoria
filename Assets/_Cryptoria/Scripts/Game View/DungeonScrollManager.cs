using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ──────────────────────────────────────────────────────────────────────────────
//  DungeonScrollManager.cs  (SO Architecture rebuild)
//
//  Responsibilities (GDD §2.4, §7.3):
//    - Spawn hero and enemy grid; initialise CardRuntimeState instances
//    - Resolve card interactions triggered by DropZone events
//    - Track run state (Ongoing / Busy / Won / Lost)
//    - Broadcast run events through GameChannelSO (no direct UI/audio references)
//    - Write pickup outcomes to PlayerProgressionSO
//    - Enforce lane-skip rules (GDD §2.4: 4 consecutive skips → lane disabled)
//    - On run win: hero fully restored (GDD §2.4)
//
//  NOT responsible for:
//    - Drag/drop physics         → CardDragHandler
//    - Visual animation          → EnemyCardView
//    - Displaying stats on cards → CardView (observes HeroRuntimeStateSO / variables)
//    - Persistent currency UI    → wired to IntVariableSO.OnValueChanged in inspector
// ──────────────────────────────────────────────────────────────────────────────

public class DungeonScrollManager : MonoBehaviour
{
    // ── Inspector — SO wiring ─────────────────────────────────────────
    [Header("SO Channels & State")]
    [SerializeField] private GameChannelSO       _channel;
    [SerializeField] private HeroRuntimeStateSO  _heroState;
    [SerializeField] private PlayerProgressionSO _progression;
    [SerializeField] private ActiveEnemyListSO   _activeEnemies;

    [Header("Prefabs")]
    [SerializeField] private GameObject _heroCardPrefab;
    [SerializeField] private GameObject _enemyCardPrefab;

    [Header("Scene References")]
    [SerializeField] private Transform _heroSpawnPoint;
    [SerializeField] private Transform _enemyGridParent;

    [Header("Data Assets")]
    [SerializeField] private CardData       _heroCardData;
    [SerializeField] private List<CardData> _enemyCardDataPool;

    [Header("Dungeon Layout")]
    [SerializeField, Tooltip("Number of enemy rows to spawn at the start of the run")]
    private int _numberOfRows = 4;
    [SerializeField, Tooltip("Number of cards per row — always 3 per GDD §2.4")]
    private int _cardsPerRow  = 3;

    // ── Private ───────────────────────────────────────────────────────
    private CardView                               _heroView;
    private List<EnemyCardView>                    _enemyViews    = new List<EnemyCardView>();
    private Dictionary<UtilityPileType, UtilityPile> _utilityPiles = new Dictionary<UtilityPileType, UtilityPile>();
    private Dictionary<int, int>                   _laneSkipCounts = new Dictionary<int, int>();
    private HashSet<int>                           _disabledLanes  = new HashSet<int>();
    private RunState                               _runState = RunState.Ongoing;

    // ─────────────────────────────────────────────────────────────────
    //  Unity lifecycle
    // ─────────────────────────────────────────────────────────────────

    private void Start()
    {
        _heroState.InitialiseForEncounter();

        _utilityPiles[UtilityPileType.Equipment] = new UtilityPile();
        _utilityPiles[UtilityPileType.Buffs]     = new UtilityPile();
        _utilityPiles[UtilityPileType.Potions]   = new UtilityPile();

        for (int col = 0; col < _cardsPerRow; col++)
            _laneSkipCounts[col] = 0;

        _activeEnemies.Clear();

        SpawnHero();
        for (int row = 0; row < _numberOfRows; row++)
            SpawnRow(row);

        _channel.Raise(GameTopic.DungeonScroll_RunStarted, _heroState);
    }

    // ─────────────────────────────────────────────────────────────────
    //  Spawn
    // ─────────────────────────────────────────────────────────────────

    private void SpawnHero()
    {
        GameObject heroGO = Instantiate(_heroCardPrefab, _heroSpawnPoint);
        if (!heroGO.TryGetComponent(out _heroView))
        {
            Debug.LogError("[DungeonScrollManager] Hero prefab is missing CardView component.", this);
            return;
        }
        _heroView.Bind(_heroCardData, _heroState.CurrentHealth);
    }

    private void SpawnRow(int rowIndex)
    {
        if (_enemyCardDataPool == null || _enemyCardDataPool.Count == 0)
        {
            Debug.LogError("[DungeonScrollManager] Enemy card data pool is empty.", this);
            return;
        }

        for (int col = 0; col < _cardsPerRow; col++)
        {
            GameObject enemyGO = Instantiate(_enemyCardPrefab, _enemyGridParent);

            if (!enemyGO.TryGetComponent(out EnemyCardView view))
            {
                Debug.LogError("[DungeonScrollManager] Enemy prefab is missing EnemyCardView.", this);
                Destroy(enemyGO);
                continue;
            }

            CardData data = _enemyCardDataPool[Random.Range(0, _enemyCardDataPool.Count)];
            CardRuntimeState state = new CardRuntimeState(data)
            {
                RowIndex    = rowIndex,
                CurrentLane = col
            };

            view.Bind(state, rowIndex, col);
            _activeEnemies.Add(state);
            _enemyViews.Add(view);

            // Wire DropZone → this manager
            if (enemyGO.TryGetComponent(out DropZone dz))
                dz.OnCardDropped.AddListener(OnHeroDroppedOnEnemy);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  Drop entry point
    // ─────────────────────────────────────────────────────────────────

    private void OnHeroDroppedOnEnemy(CardDragHandler heroCard)
    {
        if (_runState != RunState.Ongoing) return;

        // Identify the enemy view that was just dropped on
        EnemyCardView target = _enemyViews.FirstOrDefault(v =>
        {
            if (v.TryGetComponent(out DropZone dz))
                return dz.WasJustDroppedOn && v.RuntimeState.RowIndex == 0;
            return false;
        });

        // Clear all drop flags
        foreach (EnemyCardView v in _enemyViews)
        {
            if (v.TryGetComponent(out DropZone dz))
                dz.ClearDropFlag();
        }

        if (target == null)
        {
            Debug.Log("[DungeonScrollManager] No valid front-row target found.");
            return;
        }

        UpdateLaneSkipCounts(target.RuntimeState.CurrentLane);
        StartCoroutine(ResolveInteraction(target));
    }

    // ─────────────────────────────────────────────────────────────────
    //  Lane skip tracking (GDD §2.4)
    // ─────────────────────────────────────────────────────────────────

    private void UpdateLaneSkipCounts(int chosenCol)
    {
        for (int col = 0; col < _cardsPerRow; col++)
        {
            if (col == chosenCol)
            {
                _laneSkipCounts[col] = 0;
            }
            else
            {
                _laneSkipCounts[col]++;
                if (_laneSkipCounts[col] >= 4 && !_disabledLanes.Contains(col))
                {
                    _disabledLanes.Add(col);
                    _channel.Raise(GameTopic.DungeonScroll_LaneDisabled, col);
                    Debug.Log($"[DungeonScrollManager] Lane {col} disabled (4 consecutive skips).");
                    // NOTE: Key items in that lane cannot be disabled (GDD §2.4)
                    // — enforcement needed in OnHeroDroppedOnEnemy target selection
                }
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  Interaction routing
    // ─────────────────────────────────────────────────────────────────

    private IEnumerator ResolveInteraction(EnemyCardView target)
    {
        _runState = RunState.Busy;

        CardData data = target.RuntimeState.Data;

        if (data.cardCategory == CardCategory.DungeonPickup)
            ApplyPickup(data);
        else
            yield return StartCoroutine(ResolveCombat(target));

        if (_runState == RunState.Lost) yield break;

        yield return StartCoroutine(FinalizeRow(target));

        if (_runState != RunState.Won)
            _runState = RunState.Ongoing;
    }

    // ─────────────────────────────────────────────────────────────────
    //  Combat
    // ─────────────────────────────────────────────────────────────────

    private IEnumerator ResolveCombat(EnemyCardView target)
    {
        CardRuntimeState enemyState = target.RuntimeState;

        // Simultaneous damage exchange (GDD §2.4)
        _heroState.TakeDamage(enemyState.Data.damage);
        enemyState.TakeDamage(_heroState.HeroDefinition.damage);

        // Gravekeeper rule (GDD §4.3) — survivesOverkill: enemy survives at 1 HP
        if (enemyState.Data.survivesOverkill && enemyState.CurrentHealth <= 0)
        {
            enemyState.CurrentHealth = 1;
            Debug.Log($"[DungeonScrollManager] {enemyState.Data.cardName} survives overkill at 1 HP.");
        }

        // Broadcast stat changes — UI observes IntVariableSO directly
        _channel.Raise(GameTopic.Hero_HealthChanged, _heroState.CurrentHealth);
        _channel.Raise(GameTopic.Hero_ShieldChanged, _heroState.CurrentShield);

        target.RefreshHealthDisplay();

        Debug.Log($"[DungeonScrollManager] Hero dealt {_heroState.HeroDefinition.damage} dmg | " +
                  $"Enemy HP: {enemyState.CurrentHealth}/{enemyState.Data.maxHealth} | " +
                  $"Enemy dealt {enemyState.Data.damage} dmg | " +
                  $"Hero HP: {_heroState.CurrentHealth}/{_heroState.MaxHealth}");

        if (_heroState.IsDead)
        {
            _runState = RunState.Lost;
            _channel.Raise(GameTopic.DungeonScroll_RunLost, _heroState);
            Debug.Log("[DungeonScrollManager] Hero died — run over.");
            // TODO: trigger game over UI via channel listener
        }

        yield return null;
    }

    // ─────────────────────────────────────────────────────────────────
    //  Row finalization
    // ─────────────────────────────────────────────────────────────────

    private IEnumerator FinalizeRow(EnemyCardView chosen)
    {
        List<EnemyCardView> frontRow = _enemyViews
            .Where(v => v.RuntimeState.RowIndex == 0)
            .ToList();

        // Kick off all animations simultaneously then wait for the longest (destroy = 0.5 s)
        foreach (EnemyCardView v in frontRow)
        {
            if (v == chosen) StartCoroutine(v.PlayDestroyAnimation());
            else             StartCoroutine(v.PlayDiscardAnimation());
        }

        yield return new WaitForSeconds(0.5f);

        foreach (EnemyCardView v in frontRow)
        {
            _activeEnemies.Remove(v.RuntimeState);
            _enemyViews.Remove(v);
            Destroy(v.gameObject);
        }

        // Re-index remaining rows toward the front
        foreach (EnemyCardView v in _enemyViews)
            v.RuntimeState.RowIndex--;

        _channel.Raise(GameTopic.DungeonScroll_RowCleared, null);
        CheckWinCondition();
    }

    // ─────────────────────────────────────────────────────────────────
    //  Win condition
    // ─────────────────────────────────────────────────────────────────

    private void CheckWinCondition()
    {
        if (_activeEnemies.Count == 0 && _runState == RunState.Busy)
        {
            _runState = RunState.Won;
            _heroState.RestoreAfterRun();                              // GDD §2.4
            _channel.Raise(GameTopic.DungeonScroll_RunWon, _heroState);
            Debug.Log("[DungeonScrollManager] Run complete — hero restored.");
            // TODO: trigger run-complete UI via channel listener
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  Pickup routing (GDD §2.4)
    // ─────────────────────────────────────────────────────────────────

    private void ApplyPickup(CardData data)
    {
        switch (data.pickupEffectType)
        {
            case PickupEffectType.Heal:
                int healed = Mathf.Min(data.pickupEffectValue,
                    _heroState.MaxHealth - _heroState.CurrentHealth);
                _heroState.CurrentHealth += healed;
                _channel.Raise(GameTopic.Hero_HealthChanged, _heroState.CurrentHealth);
                Debug.Log($"[DungeonScrollManager] Pickup: Healed {healed} HP.");
                break;

            case PickupEffectType.Coins:
                _progression.AddCoins(data.pickupEffectValue);
                _channel.Raise(GameTopic.Economy_CoinsChanged, _progression.Coins);
                break;

            case PickupEffectType.SkillPoints:
                _progression.AddSkillPoints(data.pickupEffectValue, SkillPointTier.Common);
                _channel.Raise(GameTopic.Economy_SkillPointsChanged, null);
                break;

            case PickupEffectType.Buff:
            case PickupEffectType.Collectable:
                _utilityPiles[data.utilityPileType].Push(new CardRuntimeState(data));
                _channel.Raise(GameTopic.UtilityPile_CardPushed, data);
                break;

            case PickupEffectType.AbilityBoost:
                // TODO: apply permanent or 2–3 row boost to hero runtime state
                Debug.Log($"[DungeonScrollManager] Pickup: AbilityBoost {data.cardName} (not yet implemented).");
                break;

            case PickupEffectType.DungeonKey:
            case PickupEffectType.TreasureKey:
            case PickupEffectType.Key:
                // TODO: add to hero key inventory via PlayerProgressionSO or dedicated KeyInventorySO
                Debug.Log($"[DungeonScrollManager] Pickup: Key type {data.pickupEffectType} acquired (not yet implemented).");
                break;

            default:
                Debug.LogWarning($"[DungeonScrollManager] Unhandled pickup type: {data.pickupEffectType}");
                break;
        }

        _channel.Raise(GameTopic.DungeonScroll_PickupApplied, data);
    }
}
