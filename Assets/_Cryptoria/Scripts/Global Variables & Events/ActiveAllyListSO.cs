using UnityEngine;

/// <summary>
/// Runtime list of active ally card states in room combat.
/// Used by RoomCombatManager to track which allies are on the field.
/// resetOn = OnSingleSceneLoad (clears between encounters).
/// </summary>
[CreateAssetMenu(fileName = "list_activeAllies", menuName = "Cryptoria/Lists/ActiveAllies")]
public class ActiveAllyListSO : ScriptableListSO<CardRuntimeState> { }