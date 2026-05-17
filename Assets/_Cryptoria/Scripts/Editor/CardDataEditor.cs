using UnityEditor;
using UnityEngine;

// ──────────────────────────────────────────────────────────────────────────────
//  CardDataEditor.cs
//  Custom inspector for CardData ScriptableObjects.
//
//  Fields are grouped and shown/hidden based on the selected CardCategory:
//
//    ALL categories   → Identity (name, description, artwork) + category picker
//    Hero             → Stats, Elements (primary only), Hero Skills
//    Ally             → Stats (with mana), AllyArchetype, Elements (primary only)
//    Enemy            → Stats, Elements (full dual-element), Status Effect,
//                       Enemy Behaviour Flags, Spawner
//    DungeonPickup    → Pickup Effect Type + Value
//    UtilityCard      → Utility Pile Type
//
//  File must live in an Editor/ folder to compile correctly.
// ──────────────────────────────────────────────────────────────────────────────

[CustomEditor(typeof(CardData))]
public class CardDataEditor : Editor
{
    // ── Serialized properties — cached in OnEnable ────────────────────
    // Identity
    private SerializedProperty _cardName;
    private SerializedProperty _description;
    private SerializedProperty _cardArtwork;

    // Category
    private SerializedProperty _cardCategory;
    private SerializedProperty _allyArchetype;
    private SerializedProperty _enemyArchetype;

    // Elements
    private SerializedProperty _primaryElement;
    private SerializedProperty _secondaryElement;
    private SerializedProperty _secondaryTriggerEveryNTurns;

    // Stats
    private SerializedProperty _cardLevel;
    private SerializedProperty _maxHealth;
    private SerializedProperty _damage;
    private SerializedProperty _shield;
    private SerializedProperty _manaCost;

    // Status Effect
    private SerializedProperty _statusMagnitude;
    private SerializedProperty _statusDuration;

    // Enemy Flags
    private SerializedProperty _isUnavoidable;
    private SerializedProperty _isAggressive;
    private SerializedProperty _canAttackBackLane;
    private SerializedProperty _survivesOverkill;
    private SerializedProperty _changesLaneEachTurn;
    private SerializedProperty _applyStatusEveryNTurns;

    // Spawner
    private SerializedProperty _spawnedCards;

    // Pickup
    private SerializedProperty _pickupEffectType;
    private SerializedProperty _pickupEffectValue;

    // Utility
    private SerializedProperty _utilityPileType;

    // Hero Skills
    private SerializedProperty _signatureSkill;
    private SerializedProperty _secondarySkill;
    private SerializedProperty _interchangeableSkill;

    // ── Foldout state ─────────────────────────────────────────────────
    private bool _statsFoldout    = true;
    private bool _elementsFoldout = true;
    private bool _flagsFoldout    = true;
    private bool _skillsFoldout   = true;

    // ── Styles ────────────────────────────────────────────────────────
    private GUIStyle _sectionStyle;
    private GUIStyle _categoryBadgeStyle;

    // ─────────────────────────────────────────────────────────────────
    //  OnEnable — cache all properties
    // ─────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        _cardName    = serializedObject.FindProperty("cardName");
        _description = serializedObject.FindProperty("description");
        _cardArtwork = serializedObject.FindProperty("cardArtwork");

        _cardCategory   = serializedObject.FindProperty("cardCategory");
        _allyArchetype  = serializedObject.FindProperty("allyArchetype");
        _enemyArchetype = serializedObject.FindProperty("enemyArchetype");

        _primaryElement              = serializedObject.FindProperty("primaryElement");
        _secondaryElement            = serializedObject.FindProperty("secondaryElement");
        _secondaryTriggerEveryNTurns = serializedObject.FindProperty("secondaryTriggerEveryNTurns");

        _cardLevel  = serializedObject.FindProperty("cardLevel");
        _maxHealth  = serializedObject.FindProperty("maxHealth");
        _damage     = serializedObject.FindProperty("damage");
        _shield     = serializedObject.FindProperty("shield");
        _manaCost   = serializedObject.FindProperty("manaCost");

        _statusMagnitude = serializedObject.FindProperty("statusMagnitude");
        _statusDuration  = serializedObject.FindProperty("statusDuration");

        _isUnavoidable       = serializedObject.FindProperty("isUnavoidable");
        _isAggressive        = serializedObject.FindProperty("isAggressive");
        _canAttackBackLane   = serializedObject.FindProperty("canAttackBackLane");
        _survivesOverkill    = serializedObject.FindProperty("survivesOverkill");
        _changesLaneEachTurn = serializedObject.FindProperty("changesLaneEachTurn");
        _applyStatusEveryNTurns = serializedObject.FindProperty("applyStatusEveryNTurns");

        _spawnedCards = serializedObject.FindProperty("spawnedCards");

        _pickupEffectType  = serializedObject.FindProperty("pickupEffectType");
        _pickupEffectValue = serializedObject.FindProperty("pickupEffectValue");

        _utilityPileType = serializedObject.FindProperty("utilityPileType");

        _signatureSkill      = serializedObject.FindProperty("signatureSkill");
        _secondarySkill      = serializedObject.FindProperty("secondarySkill");
        _interchangeableSkill = serializedObject.FindProperty("interchangeableSkill");
    }

    // ─────────────────────────────────────────────────────────────────
    //  OnInspectorGUI
    // ─────────────────────────────────────────────────────────────────

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        BuildStyles();

        CardCategory category = (CardCategory)_cardCategory.enumValueIndex;

        DrawIdentitySection(category);
        EditorGUILayout.Space(4);
        DrawCategorySection(category);
        EditorGUILayout.Space(8);

        switch (category)
        {
            case CardCategory.Hero:         DrawHeroFields();         break;
            case CardCategory.Ally:         DrawAllyFields();         break;
            case CardCategory.Enemy:        DrawEnemyFields();        break;
            case CardCategory.DungeonPickup: DrawDungeonPickupFields(); break;
            case CardCategory.UtilityCard:  DrawUtilityCardFields();  break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    // ─────────────────────────────────────────────────────────────────
    //  Identity — shown for every category
    // ─────────────────────────────────────────────────────────────────

    private void DrawIdentitySection(CardCategory category)
    {
        DrawSectionHeader("Identity", CategoryColor(category));

        EditorGUILayout.PropertyField(_cardName,    new GUIContent("Card Name"));
        EditorGUILayout.PropertyField(_description, new GUIContent("Description"));
        EditorGUILayout.PropertyField(_cardArtwork, new GUIContent("Artwork"));
    }

    // ─────────────────────────────────────────────────────────────────
    //  Category picker + archetype
    // ─────────────────────────────────────────────────────────────────

    private void DrawCategorySection(CardCategory category)
    {
        DrawSectionHeader("Category", CategoryColor(category));

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_cardCategory, new GUIContent("Card Category"));
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties(); // Force immediate refresh

        EditorGUILayout.Space(2);

        switch (category)
        {
            case CardCategory.Ally:
                EditorGUILayout.PropertyField(_allyArchetype, new GUIContent("Ally Archetype"));
                break;
            case CardCategory.Enemy:
                EditorGUILayout.PropertyField(_enemyArchetype, new GUIContent("Enemy Archetype"));
                break;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  Hero fields
    // ─────────────────────────────────────────────────────────────────

    private void DrawHeroFields()
    {
        // Stats — hero has no mana cost (heroes don't cost mana to play)
        _statsFoldout = DrawFoldout(_statsFoldout, "Stats", new Color(0.4f, 0.7f, 1f));
        if (_statsFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_cardLevel, new GUIContent("Card Level"));
            EditorGUILayout.PropertyField(_maxHealth, new GUIContent("Max Health"));
            EditorGUILayout.PropertyField(_damage,    new GUIContent("Damage"));
            EditorGUILayout.PropertyField(_shield,    new GUIContent("Shield"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(4);

        // Elements — primary only (heroes have one fixed affinity, GDD §7.2)
        _elementsFoldout = DrawFoldout(_elementsFoldout, "Elemental Affinity", new Color(0.4f, 0.7f, 1f));
        if (_elementsFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_primaryElement, new GUIContent("Primary Element"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(4);

        // Skills — all three slots (GDD §5.3)
        _skillsFoldout = DrawFoldout(_skillsFoldout, "Skills", new Color(0.4f, 0.7f, 1f));
        if (_skillsFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_signatureSkill,       new GUIContent("Signature Skill (Fixed)"));
            EditorGUILayout.PropertyField(_secondarySkill,       new GUIContent("Secondary Skill (Fixed)"));
            EditorGUILayout.PropertyField(_interchangeableSkill, new GUIContent("Interchangeable Skill"));
            EditorGUI.indentLevel--;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  Ally fields
    // ─────────────────────────────────────────────────────────────────

    private void DrawAllyFields()
    {
        // Stats — allies have mana cost (GDD §2.5)
        _statsFoldout = DrawFoldout(_statsFoldout, "Stats", new Color(0.4f, 0.9f, 0.5f));
        if (_statsFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_cardLevel, new GUIContent("Card Level"));
            EditorGUILayout.PropertyField(_maxHealth, new GUIContent("Max Health"));
            EditorGUILayout.PropertyField(_damage,    new GUIContent("Damage"));
            EditorGUILayout.PropertyField(_shield,    new GUIContent("Shield"));
            EditorGUILayout.PropertyField(_manaCost,  new GUIContent("Mana Cost"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(4);

        // Elements — primary only for allies
        _elementsFoldout = DrawFoldout(_elementsFoldout, "Elemental Affinity", new Color(0.4f, 0.9f, 0.5f));
        if (_elementsFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_primaryElement, new GUIContent("Primary Element"));
            EditorGUI.indentLevel--;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  Enemy fields
    // ─────────────────────────────────────────────────────────────────

    private void DrawEnemyFields()
    {
        // Stats
        _statsFoldout = DrawFoldout(_statsFoldout, "Stats", new Color(1f, 0.4f, 0.4f));
        if (_statsFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_cardLevel, new GUIContent("Card Level"));
            EditorGUILayout.PropertyField(_maxHealth, new GUIContent("Max Health"));
            EditorGUILayout.PropertyField(_damage,    new GUIContent("Damage"));
            EditorGUILayout.PropertyField(_shield,    new GUIContent("Shield"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(4);

        // Elements — full dual-element support for enemies (GDD §6.1)
        _elementsFoldout = DrawFoldout(_elementsFoldout, "Elements", new Color(1f, 0.4f, 0.4f));
        if (_elementsFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_primaryElement,   new GUIContent("Primary Element"));
            EditorGUILayout.PropertyField(_secondaryElement, new GUIContent("Secondary Element"));

            bool hasDual = (ElementType)_secondaryElement.intValue != ElementType.None;
            if (hasDual)
            {
                EditorGUILayout.PropertyField(_secondaryTriggerEveryNTurns,
                    new GUIContent("Secondary Fires Every N Turns",
                    "0 = never. Primary fires every hit; secondary on this interval."));
            }

            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(_statusMagnitude, new GUIContent("Status Magnitude"));
            EditorGUILayout.PropertyField(_statusDuration,  new GUIContent("Status Duration (turns)"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(4);

        // Behaviour Flags
        _flagsFoldout = DrawFoldout(_flagsFoldout, "Behaviour Flags", new Color(1f, 0.4f, 0.4f));
        if (_flagsFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_isUnavoidable,
                new GUIContent("Unavoidable", "Must be killed first — blocks all other lane choices."));
            EditorGUILayout.PropertyField(_isAggressive,
                new GUIContent("Aggressive", "Attacks hero directly each turn, bypassing ally immunity."));
            EditorGUILayout.PropertyField(_canAttackBackLane,
                new GUIContent("Attacks Back Lane", "Can hit both front and back lanes."));
            EditorGUILayout.PropertyField(_survivesOverkill,
                new GUIContent("Survives Overkill", "Survives at 1 HP if killed in one turn (Gravekeeper)."));
            EditorGUILayout.PropertyField(_changesLaneEachTurn,
                new GUIContent("Changes Lane Each Turn", "Moves to a new lane position every turn (Revenant)."));

            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(_applyStatusEveryNTurns,
                new GUIContent("Apply Status Every N Turns", "0 = on every hit. Skeleton Knight = 2."));

            // Spawner sub-section — only shown when Spawner archetype flag is set
            bool isSpawner = ((EnemyArchetype)_enemyArchetype.intValue & EnemyArchetype.Spawner) != 0;
            if (isSpawner)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Spawner", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_spawnedCards, new GUIContent("Spawned Cards"), true);
            }

            EditorGUI.indentLevel--;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  DungeonPickup fields
    // ─────────────────────────────────────────────────────────────────

    private void DrawDungeonPickupFields()
    {
        DrawSectionHeader("Pickup", new Color(1f, 0.8f, 0.2f));

        EditorGUILayout.PropertyField(_pickupEffectType,
            new GUIContent("Effect Type", "What happens when the player picks this card up in a dungeon row."));

        PickupEffectType effectType = (PickupEffectType)_pickupEffectType.enumValueIndex;

        // Only show value field for effect types that use a numeric magnitude
        bool needsValue = effectType == PickupEffectType.Heal
                       || effectType == PickupEffectType.Coins
                       || effectType == PickupEffectType.SkillPoints;

        if (needsValue)
        {
            EditorGUILayout.PropertyField(_pickupEffectValue,
                new GUIContent("Effect Value", "Amount healed / coins granted / skill points added."));
        }

        // Buff/Collectable cards also need a utility pile assignment
        bool needsPile = effectType == PickupEffectType.Buff
                      || effectType == PickupEffectType.Collectable;
        if (needsPile)
        {
            EditorGUILayout.PropertyField(_utilityPileType,
                new GUIContent("Routes To Pile", "Which utility pile this card goes to on pickup."));
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  UtilityCard fields
    // ─────────────────────────────────────────────────────────────────

    private void DrawUtilityCardFields()
    {
        DrawSectionHeader("Utility Pile", new Color(0.8f, 0.5f, 1f));

        EditorGUILayout.PropertyField(_utilityPileType,
            new GUIContent("Pile Type", "Which pile this card belongs to in the Dungeon Scroll loadout."));

        // Utility cards have stats so the player knows what they do
        EditorGUILayout.Space(4);
        _statsFoldout = DrawFoldout(_statsFoldout, "Effect Stats", new Color(0.8f, 0.5f, 1f));
        if (_statsFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_cardLevel,       new GUIContent("Card Level"));
            EditorGUILayout.PropertyField(_statusMagnitude, new GUIContent("Effect Magnitude"));
            EditorGUILayout.PropertyField(_statusDuration,  new GUIContent("Effect Duration (turns)"));
            EditorGUI.indentLevel--;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  Drawing helpers
    // ─────────────────────────────────────────────────────────────────

    private void DrawSectionHeader(string label, Color color)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, 22f);
        EditorGUI.DrawRect(rect, color * 0.35f);

        rect.x    += 6;
        rect.width -= 6;
        GUI.Label(rect, label.ToUpper(), _sectionStyle);
    }

    private bool DrawFoldout(bool state, string label, Color color)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, 20f);
        EditorGUI.DrawRect(rect, color * 0.2f);

        rect.x    += 4;
        rect.width -= 4;
        return EditorGUI.Foldout(rect, state, label, true, EditorStyles.foldoutHeader);
    }

    private void BuildStyles()
    {
        if (_sectionStyle != null) return;

        _sectionStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 11,
            alignment = TextAnchor.MiddleLeft,
            normal    = { textColor = Color.white }
        };
    }

    private Color CategoryColor(CardCategory category)
    {
        switch (category)
        {
            case CardCategory.Hero:          return new Color(0.4f, 0.7f, 1f);    // Blue
            case CardCategory.Ally:          return new Color(0.4f, 0.9f, 0.5f);  // Green
            case CardCategory.Enemy:         return new Color(1f,   0.4f, 0.4f);  // Red
            case CardCategory.DungeonPickup: return new Color(1f,   0.8f, 0.2f);  // Gold
            case CardCategory.UtilityCard:   return new Color(0.8f, 0.5f, 1f);    // Purple
            default:                         return Color.grey;
        }
    }
}