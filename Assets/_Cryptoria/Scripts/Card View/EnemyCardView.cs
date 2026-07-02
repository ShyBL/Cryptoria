using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Holds one enemy's CardRuntimeState and drives its visual display and animations.
/// DungeonScrollManager calls Bind() on spawn, then reads RuntimeState for combat.
/// </summary>
public class EnemyCardView : MonoBehaviour
{
    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI _nameLabel;
    [SerializeField] private TextMeshProUGUI _levelLabel;
    [SerializeField] private TextMeshProUGUI _healthLabel;
    [SerializeField] private TextMeshProUGUI _damageLabel;

    [Header("Visuals")]
    [SerializeField] private Image           _cardArtwork;
    [SerializeField] private GameObject      _cardMesh;
    [SerializeField] private ParticleSystem  _destroyParticle;
    public UnityEvent OnDestroy;
    
    // ── Runtime state exposed to managers ────────────────────────────
    public CardRuntimeState RuntimeState { get; private set; }

    // ── Initialisation ────────────────────────────────────────────────

    /// <summary>
    /// Called by DungeonScrollManager on spawn. Creates the runtime state and refreshes display.
    /// </summary>
    public void Bind(CardRuntimeState state, int rowIndex, int colIndex)
    {
        RuntimeState             = state;
        RuntimeState.RowIndex    = rowIndex;
        RuntimeState.CurrentLane = colIndex;
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (RuntimeState == null) return;
        CardData d = RuntimeState.Data;

        if (_nameLabel)   _nameLabel.text   = d.cardName;
        if (_levelLabel)  _levelLabel.text  = $"Lvl {d.cardLevel}";
        if (_healthLabel) _healthLabel.text = $"{RuntimeState.CurrentHealth} / {d.maxHealth}";
        if (_damageLabel) _damageLabel.text = $"ATK {d.damage}";

        if (_cardArtwork != null && d.cardArtwork != null)
            _cardArtwork.sprite = d.cardArtwork;
    }

    /// <summary>Called by DungeonScrollManager after damage is applied.</summary>
    public void RefreshHealthDisplay()
    {
        if (RuntimeState == null || _healthLabel == null) return;
        _healthLabel.text = $"{RuntimeState.CurrentHealth} / {RuntimeState.Data.maxHealth}";
    }

    // ── Animations ────────────────────────────────────────────────────

    /// <summary>
    /// Plays the destruction particle and waits 0.5 s.
    /// Started by DungeonScrollManager via StartCoroutine.
    /// </summary>
    public IEnumerator PlayDestroyAnimation()
    {
        //if (_cardMesh != null)        _cardMesh.SetActive(true);
        //if (_destroyParticle != null) _destroyParticle.Play();
        OnDestroy.Invoke();
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Slides the card down and fades it out over 0.3 s using a manual lerp.
    /// Started by DungeonScrollManager via StartCoroutine.
    /// Uses RectTransform.anchoredPosition — correct for Canvas-parented UI objects.
    /// </summary>
    public IEnumerator PlayDiscardAnimation()
    {
        RectTransform rt = GetComponent<RectTransform>();
        CanvasGroup   cg = GetComponent<CanvasGroup>();

        if (rt == null && cg == null) { yield return new WaitForSeconds(0.3f); yield break; }

        float duration  = 0.3f;
        float elapsed   = 0f;
        float startY    = rt != null ? rt.anchoredPosition.y : 0f;
        float startAlpha = cg != null ? cg.alpha : 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.Clamp01(elapsed / duration);

            if (rt != null)
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, Mathf.Lerp(startY, startY - 100f, t));

            if (cg != null)
                cg.alpha = Mathf.Lerp(startAlpha, 0f, t);

            yield return null;
        }
    }
}