using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static TileController;
using UnityEngine.Playables;

public class BattleEndUIHandler : MonoBehaviour
{
    public Image battleEndResultsScreen;
    public TextMeshProUGUI battleEndMessageText;
    public TextMeshProUGUI battleEndExperiencePointsText;
    public TextMeshProUGUI battleEndWarFundsGainedText;
    public TextMeshProUGUI battleEndEnemiesKilledText;

    public RectTransform battlefieldNotificationsContainer;

    public PlayableDirector battleCameraPlayableDirector;
    [SerializeField] float battleEndResultsScreenDelay;
    public void OnEnable()
    {
        TurnController.OnBattleEnd += DisplayBattleEndScreen;
        PlaceCrystalPlayerAction.OnBattleEndCapturedDeity += DisplayBattleEndScreen;
    }
    public void OnDisable()
    {
        TurnController.OnBattleEnd -= DisplayBattleEndScreen;
        PlaceCrystalPlayerAction.OnBattleEndCapturedDeity -= DisplayBattleEndScreen;
    }
    //At the end of the Battle UI overlay appears (using Size scaling) with the results of the Battle.
    public void DisplayBattleEndScreen(string battleEndMessage)
    {
        battleEndMessageText.text = battleEndMessage;
        DeactivateBattleUI();
        StartCoroutine("DisplayBattleEndResultsScreen");
    }
    IEnumerator DisplayBattleEndResultsScreen()
    {
        yield return new WaitForSeconds(battleEndResultsScreenDelay);
        battleEndResultsScreen.transform.localScale = new Vector3(1, 1, 1);
    }

    public void DeactivateBattleUI()
    {
        battlefieldNotificationsContainer.transform.localScale = new Vector3(0, 0, 0);
        GameObject.FindGameObjectWithTag("MovesPanel").gameObject.SetActive(false);
        GameObject.FindGameObjectWithTag("BattleInterfaceCanvas").transform.GetChild(0).gameObject.SetActive(false);

    }
}