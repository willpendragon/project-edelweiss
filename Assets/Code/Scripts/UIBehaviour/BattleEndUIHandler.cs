using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;


public class BattleEndUIHandler : MonoBehaviour
{
    [Header("UI Behaviour")]
    [SerializeField] float battleEndResultsScreenDelay;

    [Header("UI Visuals")]
    public Image battleEndResultsScreen;
    public RectTransform battlefieldNotificationsContainer;
    public Button returnButton;

    [Header("UI Texts")]
    public TextMeshProUGUI battleEndMessageText;
    public TextMeshProUGUI battleEndExperiencePointsText;
    public TextMeshProUGUI battleEndWarFundsGainedText;
    public TextMeshProUGUI battleEndEnemiesKilledText;
    public TextMeshProUGUI battleEndCrystalObtainedText;

    private string saveFilePath;


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
        if (battleEndMessageText.text == "Defeat")
        {
            ChangeReturnButton();
        }
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
        DeactivateWaitIcons();
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

    private void DeactivateWaitIcons()
    {
        TurnController turnController = BattleManager.Instance?.GetComponent<TurnController>();
        GameObject[] playerUnitsOnBattlefield = turnController?.playerUnitsOnBattlefield;

        foreach (var playerUnit in playerUnitsOnBattlefield)
        {
            playerUnit.GetComponent<UnitIconsController>().HideWaitingIcon();
        }
    }

    private void ChangeReturnButton()
    {
        returnButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Start New Run";
        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(() => StartNewGameRun());
    }

    private void StartNewGameRun()
    {
        saveFilePath = Application.persistentDataPath + "/gameSaveData.json";
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Deleted Saved Game Data");
        }
        else
        {
            Debug.LogWarning("No Saved Game Data found.");
        }
        SceneManager.LoadScene("start_screen");
    }

}
