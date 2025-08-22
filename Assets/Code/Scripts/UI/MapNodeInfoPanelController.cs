using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapNodeInfoPanelController : MonoBehaviour
{
    [SerializeField] GameObject mapNodeGameObject;
    [SerializeField] TextMeshProUGUI minPoolSizeText;
    [SerializeField] TextMeshProUGUI maxPoolSizeText;
    [SerializeField] GameObject mapNodeInfoPanelCanvas;
    [SerializeField] GameObject[] enemyPoolPredictionsGameObject;
    [SerializeField] Sprite enemyIcon1;
    [SerializeField] Sprite enemyIcon2;
    [SerializeField] Sprite enemyIcon3;
    [SerializeField] Sprite defaultIcon;

    private void Start()
    {
        EnemySelection mapNodeEnemySelection = mapNodeGameObject.GetComponent<EnemySelection>();

        minPoolSizeText.text = mapNodeEnemySelection.enemyParty.minEnemyPoolSize.ToString();
        maxPoolSizeText.text = mapNodeEnemySelection.enemyParty.maxEnemyPoolSize.ToString();

        for (int i = 0; i < enemyPoolPredictionsGameObject.Length; i++)
        {

            if (i < mapNodeEnemySelection.enemyParty.enemyWeights.Count)
            {
                // Assign enemy type text if within range
                enemyPoolPredictionsGameObject[i].GetComponentInChildren<TextMeshProUGUI>().text = mapNodeEnemySelection.enemyParty.enemyWeights[i].weight.ToString().ToUpper();
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
                return enemyIcon1;
            case "dummy2":
                return enemyIcon2;
            case "dummy3":
                return enemyIcon3;
            default:
                return defaultIcon;
        }
    }
}