using System.Collections.Generic;

// ──────────────────────────────────────────────────────────────────────────────
//  UtilityPile.cs
//  Capped LIFO pile for one Dungeon Scroll utility pile slot.
//  One instance per UtilityPileType (Equipment, Buffs, Potions).
//
//  GDD §2.4:
//    - Cards always played from the top (most recently acquired plays first)
//    - Max 3 cards per pile (total utility cards not to exceed 6 across all 3 piles)
//    - When full and a new card is pushed, the oldest (bottom) card is evicted
//    - All unused pile cards discarded at run end — not persistent
//
//  Uses List<T> not Stack<T> to allow O(1) bottom-eviction without the
//  reversal bug introduced by Stack→List→Stack round-trips.
// ──────────────────────────────────────────────────────────────────────────────

public class UtilityPile
{
    private const int MAX_CARDS = 3;

    // Index 0 = bottom (oldest). Index Count-1 = top (most recent, plays first).
    private readonly List<CardRuntimeState> _cards = new List<CardRuntimeState>();

    // ── Write ─────────────────────────────────────────────────────────

    /// <summary>
    /// Push a card onto the top of the pile.
    /// If already at MAX_CARDS, evicts the oldest (bottom) card first.
    /// </summary>
    public void Push(CardRuntimeState card)
    {
        if (_cards.Count >= MAX_CARDS)
            _cards.RemoveAt(0); // Evict oldest

        _cards.Add(card);       // New card goes to top (end of list)
    }

    // ── Read / Play ───────────────────────────────────────────────────

    /// <summary>
    /// Pop and return the top card (most recently added).
    /// Returns null if the pile is empty.
    /// </summary>
    public CardRuntimeState Pop()
    {
        if (_cards.Count == 0) return null;
        CardRuntimeState top = _cards[_cards.Count - 1];
        _cards.RemoveAt(_cards.Count - 1);
        return top;
    }

    /// <summary>Peek at the top card without removing it. Returns null if empty.</summary>
    public CardRuntimeState Peek() =>
        _cards.Count > 0 ? _cards[_cards.Count - 1] : null;

    // ── State ─────────────────────────────────────────────────────────

    public int  Count   => _cards.Count;
    public bool IsEmpty => _cards.Count == 0;
    public bool IsFull  => _cards.Count >= MAX_CARDS;

    /// <summary>
    /// Discard all cards — called at run end (GDD §2.4: pile cards do not persist).
    /// </summary>
    public void Clear() => _cards.Clear();
}
