using UnityEngine;

/// <summary>
/// The 6 ally cards configured in the Deck Builder for room combat.
/// Order determines which cards are active on the field first.
/// </summary>
[CreateAssetMenu(fileName = "list_roomCombatDeck", menuName = "Cryptoria/Lists/RoomCombatDeck")]
public class RoomCombatDeckListSO : ScriptableListSO<CardData> { }
