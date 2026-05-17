using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ──────────────────────────────────────────────────────────────────────────────
//  CardView.cs  +  EnemyCardView.cs
//
//  Visual components for card prefabs.
//  Responsibility: display CardData / CardRuntimeState values; play animations.
//  Zero game logic lives here — all combat decisions are in the managers.
//
//  SO Architecture role: observe IntVariableSO / HeroRuntimeStateSO by wiring
//  to their OnValueChanged events where needed. EnemyCardView holds a direct
//  reference to its CardRuntimeState (created by DungeonScrollManager on spawn).
// ──────────────────────────────────────────────────────────────────────────────

// ── CardView — hero card and generic card display ─────────────────────────────

/// <summary>
/// Displays CardData on any card prefab and exposes UpdateHealthDisplay
/// for manager-driven health updates.
/// Hero card uses this component — DungeonScrollManager calls Bind() on spawn.
/// </summary>
public class CardView : MonoBehaviour
{
    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI _nameLabel;
    [SerializeField] private TextMeshProUGUI _levelLabel;
    [SerializeField] private TextMeshProUGUI _healthLabel;
    [SerializeField] private TextMeshProUGUI _damageLabel;
    [SerializeField] private TextMeshProUGUI _shieldLabel;
    [SerializeField] private TextMeshProUGUI _descriptionLabel;

    [Header("Visuals")]
    [SerializeField] private Image _cardArtwork;
    [SerializeField] private Image _primaryElementIcon;
    [SerializeField] private Image _secondaryElementIcon;

    private CardData _data;
    private int      _maxHealth;

    // ── Initialisation ────────────────────────────────────────────────

    /// <summary>
    /// Called by DungeonScrollManager after spawning the hero prefab.
    /// </summary>
    public void Bind(CardData data, int currentHealth)
    {
        _data      = data;
        _maxHealth = data.maxHealth;
        RefreshAll(currentHealth);
    }

    private void RefreshAll(int currentHealth)
    {
        if (_data == null) return;

        if (_nameLabel)        _nameLabel.text        = _data.cardName;
        if (_levelLabel)       _levelLabel.text       = $"Lvl {_data.cardLevel}";
        if (_healthLabel)      _healthLabel.text      = $"{currentHealth} / {_maxHealth}";
        if (_damageLabel)      _damageLabel.text      = $"ATK {_data.damage}";
        if (_shieldLabel)      _shieldLabel.text      = $"DEF {_data.shield}";
        if (_descriptionLabel) _descriptionLabel.text = _data.description;

        if (_cardArtwork != null && _data.cardArtwork != null)
            _cardArtwork.sprite = _data.cardArtwork;

        SetElementIcon(_primaryElementIcon,   _data.primaryElement);
        SetElementIcon(_secondaryElementIcon, _data.secondaryElement);
    }

    /// <summary>
    /// Called by the manager after each damage calculation to update the health label.
    /// This is display only — the true value lives in HeroRuntimeStateSO.
    /// </summary>
    public void UpdateHealthDisplay(int current, int max)
    {
        if (_healthLabel) _healthLabel.text = $"{current} / {max}";
    }

    private void SetElementIcon(Image icon, ElementType element)
    {
        if (icon == null) return;
        icon.gameObject.SetActive(element != ElementType.None);
        // Sprite mapping wired in Inspector per element icon set
    }
}

// ── EnemyCardView — enemy slot in Dungeon Scroll or Room Combat ───────────────