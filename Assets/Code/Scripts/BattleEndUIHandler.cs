using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static TileController;

public class BattleEndUIHandler : MonoBehaviour
{
    [Header("UI Behaviour")]
    [SerializeField] float battleEndResultsScreenDelay;

    [Header("UI Visuals")]
    public Image battleEndResultsScreen;
    public RectTransform battlefieldNotificationsContainer;

    [Header("UI Texts")]
    public TextMeshProUGUI battleEndMessageText;
    public TextMeshProUGUI battleEndExperiencePointsText;
    public TextMeshProUGUI battleEndWarFundsGainedText;
    public TextMeshProUGUI battleEndEnemiesKilledText;

    //public PlayableDirector battleCameraPlayableDirector;
    private void OnEnable()
    {
        TurnController.OnBattleEnd += DisplayBattleEndScreen;
        PlaceCrystalPlayerAction.OnBattleEndCapturedDeity += DisplayBattleEndScreen;
    }
    private void OnDisable()
    {
        TurnController.OnBattleEnd -= DisplayBattleEndScreen;
        PlaceCrystalPlayerAction.OnBattleEndCapturedDeity -= DisplayBattleEndScreen;
    }
    //At the end of the Battle UI overlay appears (using Size scaling) with the results of the Battle.
    private void DisplayBattleEndScreen(string battleEndMessage)
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
    private void DeactivateBattleUI()
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

        BattleInterface.Instance.movesContainer.SetActive(false);
        DeactivateUnitSelectionCursor();
        DeactivateStatusIcons();
    }
    private void DeactivateUnitSelectionCursor()
    {
        GameObject[] selectionCursors = GameObject.FindGameObjectsWithTag("ActivePlayerCharacterSelectionIcon");
        foreach (var selectionCursor in selectionCursors)
        {
            Destroy(selectionCursor);
        }
    }
    private void DeactivateStatusIcons()
    {
        foreach (var statusIcon in GridManager.Instance.statusIcons)
        {
            Destroy(statusIcon);
        }
    }
}
