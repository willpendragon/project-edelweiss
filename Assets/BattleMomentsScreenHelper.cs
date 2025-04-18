using System.Collections;
using TMPro;
using UnityEngine;

public class BattleMomentsScreenHelper : MonoBehaviour
{
    [SerializeField] GameObject battleMomentsScreen;
    [SerializeField] float battleMomentsScreenDeactivationTime;
    private void OnEnable()
    {
        TurnController.OnEnemyTurn += ActivateBattleMomentsScreen;
        EnemyTurnManager.OnPlayerTurn += ActivateBattleMomentsScreen;
        EnemyTurnManager.OnDeityTurn += ActivateBattleMomentsScreen;
    }

    private void OnDisable()
    {
        EnemyTurnManager.OnPlayerTurn -= ActivateBattleMomentsScreen;
        EnemyTurnManager.OnDeityTurn -= ActivateBattleMomentsScreen;
        TurnController.OnEnemyTurn -= ActivateBattleMomentsScreen;
    }
    public void ActivateBattleMomentsScreen(string battleMomentText)
    {
        battleMomentsScreen.SetActive(true);
        battleMomentsScreen.GetComponentInChildren<TextMeshProUGUI>().text = battleMomentText;
        StartCoroutine("DeactivateBattleMomentsScreen");
    }
    IEnumerator DeactivateBattleMomentsScreen()
    {
        yield return new WaitForSeconds(battleMomentsScreenDeactivationTime);
        battleMomentsScreen.SetActive(false);
    }
}