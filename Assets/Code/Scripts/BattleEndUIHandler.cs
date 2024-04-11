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
        GameObject battleInterfaceCanvas = GameObject.FindGameObjectWithTag("BattleInterfaceCanvas");
        if (battleInterfaceCanvas != null) // Check if the GameObject was found
        {
            Transform childTransform;
            // Check if the child index exists
            if (battleInterfaceCanvas.transform.childCount > 1 && (childTransform = battleInterfaceCanvas.transform.GetChild(1)) != null)
            {
                childTransform.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("The specified child of BattleInterfaceCanvas does not exist!");
            }
        }
        else
        {
            Debug.LogWarning("BattleInterfaceCanvas not found!");
        }

    }
}