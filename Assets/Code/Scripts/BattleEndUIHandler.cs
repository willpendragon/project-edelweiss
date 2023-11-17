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
    public PlayableDirector battleCameraPlayableDirector;
    [SerializeField] float battleEndResultsScreenDelay;
    public void OnEnable()
    {
        BattleManager.OnBattleEndResultsScreen += DisplayBattleEndScreen;
    }
    public void OnDisable()
    {
        BattleManager.OnBattleEndResultsScreen -= DisplayBattleEndScreen;
    }
    public void DisplayBattleEndScreen(string battleEndMessage)
    {
        battleCameraPlayableDirector.Play();
        battleEndMessageText.text = battleEndMessage;
        StartCoroutine("DisplayBattleEndResultsScreen");
    }
    IEnumerator DisplayBattleEndResultsScreen()
    {
        yield return new WaitForSeconds(battleEndResultsScreenDelay);
        battleEndResultsScreen.transform.localScale = new Vector3(1, 1, 1);
    }
}