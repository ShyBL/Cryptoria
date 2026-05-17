using UnityEngine;

/// <summary>
/// Runtime list of active enemy card states — used by both combat managers.
/// </summary>
[CreateAssetMenu(fileName = "list_activeEnemies", menuName = "Cryptoria/Lists/ActiveEnemies")]
public class ActiveEnemyListSO : ScriptableListSO<CardRuntimeState> { }