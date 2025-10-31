using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapNodeInfoPanelController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] GameObject mapNodeGameObject;
    [SerializeField] TextMeshProUGUI poolSize;
    [SerializeField] GameObject mapNodeInfoPanelCanvas;
    [SerializeField] GameObject[] enemyPoolPredictionsGameObject;
    [SerializeField] Sprite defaultIcon;
    [SerializeField] List<UnitTemplate> _enemyUnits; // Remember to add all of the possible Enemy SOs in the Inspector.
    [SerializeField] MapNodeController _mapNodeController;

    private void Start()
    {
        EnemySelection mapNodeEnemySelection = mapNodeGameObject.GetComponent<EnemySelection>();

        poolSize.text = $"The Enemy Party in this battle will contain a number of foes between {mapNodeEnemySelection.enemyParty.minEnemyPoolSize} and {mapNodeEnemySelection.enemyParty.maxEnemyPoolSize}.";

        for (int i = 0; i < enemyPoolPredictionsGameObject.Length; i++)
        {

            if (i < mapNodeEnemySelection.enemyParty.enemyWeights.Count)
            {
                // Display Enemy Weights
                var enemyPredictionsText = enemyPoolPredictionsGameObject[i].GetComponentInChildren<TextMeshProUGUI>();
                enemyPredictionsText.text = $"Encounter Chance: {mapNodeEnemySelection.enemyParty.enemyWeights[i].weight}";

                // Retrieve Enemy Type
                string enemyType = mapNodeEnemySelection.enemyParty.enemyWeights[i].enemyType.ToString();
                enemyPoolPredictionsGameObject[i].GetComponent<Image>().sprite = SetEnemyIcon(enemyType);
            }
            else
            {
                // Set empty or placeholder text for unused slots
                enemyPoolPredictionsGameObject[i].GetComponentInChildren<TextMeshProUGUI>().text = "-";
            }
        }
    }

    private Sprite SetEnemyIcon(string enemyId)
    {
        // Pass the Icon and set it
        switch (enemyId)
        {
            case "dummy1":
                return _enemyUnits[0].unitPortrait;
            case "dummy2":
                return _enemyUnits[1].unitPortrait;
            case "dummy3":
                return _enemyUnits[2].unitPortrait;
            case "Wildermann":
                return _enemyUnits[3].unitPortrait;
            default:
                return defaultIcon;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ResumeGame();
    }

    public void ResumeGame()
    {
        _mapNodeController.CloseLocationEnterPanel();
    }
}