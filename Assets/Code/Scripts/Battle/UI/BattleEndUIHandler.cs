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
    public RectTransform battlefieldNotificationsContainer;
    public Button returnButton;
    [SerializeField] GameObject _battleInterfaceCanvasObject;
    [SerializeField] GameObject _battleDetailsPanelObject;
    [SerializeField] CanvasGroup _battleDetailsGroup;
    [SerializeField] CanvasGroup _battleNotificationsGroup;
    [SerializeField] CanvasGroup _battleEndScreenGroup;

    [Header("UI Texts")]
    public TextMeshProUGUI battleEndMessageText;
    public TextMeshProUGUI battleEndExperiencePointsText;
    public TextMeshProUGUI battleEndWarFundsGainedText;
    public TextMeshProUGUI battleEndEnemiesKilledText;
    public TextMeshProUGUI battleEndCrystalObtainedText;

    private string saveFilePath;

    private void OnEnable()
    {
        BattleFlowController.OnBattleEnd += DisplayBattleEndScreen;
        TurnController.OnBattleEnd += DisplayBattleEndScreen;
        PlaceCrystalPlayerAction.OnBattleEndCapturedDeity += DisplayBattleEndScreen;
    }
    private void OnDisable()
    {
        BattleFlowController.OnBattleEnd -= DisplayBattleEndScreen;
        TurnController.OnBattleEnd -= DisplayBattleEndScreen;
        PlaceCrystalPlayerAction.OnBattleEndCapturedDeity -= DisplayBattleEndScreen;
    }

    private void Start()
    {
        _battleEndScreenGroup.alpha = 0;
        _battleEndScreenGroup.blocksRaycasts = false;
    }
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
        // Change to Canvas and use CanvasGroup
        _battleEndScreenGroup.alpha = 1;
        _battleEndScreenGroup.blocksRaycasts = true;
    }
    private void DeactivateBattleUI()
    {
        _battleDetailsGroup.alpha = 0;
        _battleNotificationsGroup.alpha = 0;
        DeactivateUnitSelectionIcon();
        DeactivateStatusIcons();

        //DeactivateUnitProfile();
        //DeactivateWaitIcons();
        //DeactivateBattleDetailsPanel();
        //Transform childTransform;
        //// Check if the child index exists
        //if (_battleInterfaceCanvasObject.transform.childCount > 1 && (childTransform = _battleInterfaceCanvasObject.transform.GetChild(1)) != null)
        //{
        //    childTransform.gameObject.SetActive(false);
        //}
    }

    private void DeactivateBattleDetailsPanel()
    {
        _battleDetailsPanelObject.SetActive(false);
    }

    private void DeactivateUnitProfile()
    {
        var existingInfoPanel = GameObject.FindGameObjectWithTag("ActiveCharacterUnitProfile");
        Destroy(existingInfoPanel);
    }

    private void DeactivateUnitSelectionIcon()
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
        GameObject gameManagerInstance = GameObject.FindGameObjectWithTag("GameManager");
        Destroy(gameManagerInstance);
        SceneManager.LoadScene("start_screen");
    }
}
