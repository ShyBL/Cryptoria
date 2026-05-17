using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// ──────────────────────────────────────────────────────────────────────────────
//  DropZone.cs  (SO Architecture rebuild)
//
//  Responsibility: receive a card drop; fire the CardDropEvent; expose the
//  WasJustDroppedOn flag so DungeonScrollManager can identify which enemy
//  slot was targeted within the same event frame.
//
//  No game logic, no combat, no stat references.
// ──────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public class CardDropEvent : UnityEvent<CardDragHandler> { }

public class DropZone : MonoBehaviour, IDropHandler
{
    /// <summary>Wired in Inspector or by DungeonScrollManager.AddListener().</summary>
    public CardDropEvent OnCardDropped;

    /// <summary>
    /// Set to true when a drop occurs. DungeonScrollManager reads this immediately
    /// after OnCardDropped fires, then calls ClearDropFlag() to reset it.
    /// </summary>
    public bool WasJustDroppedOn { get; private set; }

    public void OnDrop(PointerEventData eventData)
    {
        CardDragHandler droppedCard = null;
        if (eventData.pointerDrag != null)
            eventData.pointerDrag.TryGetComponent(out droppedCard);

        if (droppedCard != null)
        {
            WasJustDroppedOn = true;
            OnCardDropped.Invoke(droppedCard);
        }
    }

    /// <summary>Called by DungeonScrollManager after reading WasJustDroppedOn.</summary>
    public void ClearDropFlag() => WasJustDroppedOn = false;
}
