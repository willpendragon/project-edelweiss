using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using DG.Tweening;

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
    [SerializeField] CanvasGroup _battleUpgradesGroup;
    [SerializeField] CanvasGroup _mainCanvasGroup;
    [SerializeField] CanvasGroup _fadeCanvasGroup;
    [SerializeField] CursorController _cursor;

    [Header("UI Texts")]
    public TextMeshProUGUI battleEndMessageText;
    public TextMeshProUGUI battleEndExperiencePointsText;
    public TextMeshProUGUI battleEndWarFundsGainedText;
    public TextMeshProUGUI battleEndEnemiesKilledText;
    public TextMeshProUGUI battleEndCrystalObtainedText;
    public TextMeshProUGUI battleEndIngredients;


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
        _fadeCanvasGroup.DOFade(1, 0.5f);
        StartCoroutine("DisplayBattleEndResultsScreen");
    }
    IEnumerator DisplayBattleEndResultsScreen()
    {
        yield return new WaitForSeconds(battleEndResultsScreenDelay);
        _fadeCanvasGroup.DOFade(0, 0.5f);
        _battleEndScreenGroup.alpha = 1;
        _battleEndScreenGroup.blocksRaycasts = true;
        // Play camera animation.
        BattleManager.Instance.PlayCameraBattleEndAnimation();
    }
    private void DeactivateBattleUI()
    {
        _battleDetailsGroup.alpha = 0;
        _battleNotificationsGroup.alpha = 0;
        _battleUpgradesGroup.alpha = 0;
        _mainCanvasGroup.alpha = 0;
        DeactivateUnitSelectionIcon();
        DeactivateStatusIcons();
        // Deactivate the radial menu.
        _cursor.enabled = false;
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
        // NEW: Clear the list after destroying all icons
        GridManager.Instance.statusIcons.Clear();
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
        WipeGameSaveData();
        GameObject gameManagerInstance = GameObject.FindGameObjectWithTag("GameManager");
        Destroy(gameManagerInstance);
        SceneManager.LoadScene("start_screen");
    }
    // Duplicated method, for playtest demo only.
    private void WipeGameSaveData()
    {
        GameSaveData freshData = new GameSaveData();
        SaveStateManager.saveData = freshData;
        SaveStateManager.SaveGame(freshData);
        Debug.Log("Game data reset to blank slate.");
    }
}