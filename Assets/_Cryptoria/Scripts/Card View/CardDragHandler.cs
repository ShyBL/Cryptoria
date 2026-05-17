using UnityEngine;
using UnityEngine.EventSystems;

// ──────────────────────────────────────────────────────────────────────────────
//  CardDragHandler.cs  (SO Architecture rebuild)
//
//  Responsibility: UI drag-and-drop input only.
//  - Moves the card RectTransform during drag
//  - Makes the card semi-transparent and raycast-transparent while dragging
//  - On release: delegates to the hit DropZone; snaps back if none found
//
//  No CardData, no combat logic, no manager references.
//  The manager wires up via DropZone.OnCardDropped (UnityEvent).
// ──────────────────────────────────────────────────────────────────────────────

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // ── Cached components ─────────────────────────────────────────────
    private RectTransform _rectTransform;
    private CanvasGroup   _canvasGroup;
    private Canvas        _canvas;

    // ── State ─────────────────────────────────────────────────────────
    private Vector2 _originalPosition;

    // ─────────────────────────────────────────────────────────────────
    //  Unity lifecycle
    // ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        TryGetComponent(out _rectTransform);
        TryGetComponent(out _canvasGroup);
        _canvas = GetComponentInParent<Canvas>();

        if (_canvas == null)
            Debug.LogError("[CardDragHandler] No parent Canvas found.", this);
    }

    // ─────────────────────────────────────────────────────────────────
    //  Drag handlers
    // ─────────────────────────────────────────────────────────────────

    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalPosition           = _rectTransform.anchoredPosition;
        _canvasGroup.alpha           = 0.6f;
        _canvasGroup.blocksRaycasts  = false; // Let the pointer hit DropZones beneath the card
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_canvas == null) return;
        _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    /// <summary>
    /// On release: if a DropZone was hit delegate to it; otherwise snap back.
    /// DropZone sets WasJustDroppedOn and fires OnCardDropped → DungeonScrollManager.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.alpha          = 1f;
        _canvasGroup.blocksRaycasts = true;

        GameObject hit      = eventData.pointerCurrentRaycast.gameObject;
        DropZone   dropZone = null;

        if (hit != null)
            hit.TryGetComponent(out dropZone);

        if (dropZone != null)
            dropZone.OnDrop(eventData);
        else
        {
            _rectTransform.anchoredPosition = _originalPosition;
            Debug.Log("[CardDragHandler] No valid DropZone — snapping back.");
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  Utility
    // ─────────────────────────────────────────────────────────────────

    /// <summary>Reparent this card and reset its anchored position to the new parent's pivot.</summary>
    public void SetParentAndPosition(Transform parent)
    {
        _rectTransform.SetParent(parent);
        _rectTransform.anchoredPosition = Vector2.zero;
    }
}
