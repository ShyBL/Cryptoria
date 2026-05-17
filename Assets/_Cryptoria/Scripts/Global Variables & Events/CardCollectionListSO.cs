using UnityEngine;

/// <summary>
/// The player's persistent card collection — survives all sessions.
/// resetOn = None so authored data is never cleared.
/// </summary>
[CreateAssetMenu(fileName = "list_playerCollection", menuName = "Cryptoria/Lists/PlayerCollection")]
public class CardCollectionListSO : ScriptableListSO<CardData> { }