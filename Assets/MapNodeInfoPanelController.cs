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

    private void Start()
    {
        EnemySelection mapNodeEnemySelection = mapNodeGameObject.GetComponent<EnemySelection>();

        minPoolSizeText.text = mapNodeEnemySelection.levelData.minEnemyPoolSize.ToString();
        maxPoolSizeText.text = mapNodeEnemySelection.levelData.maxEnemyPoolSize.ToString();

        for (int i = 0; i < enemyPoolPredictionsGameObject.Length; i++)
        {

            if (i < mapNodeEnemySelection.levelData.enemyWeights.Count)
            {
                // Assign enemy type text if within range
                enemyPoolPredictionsGameObject[i].GetComponentInChildren<TextMeshProUGUI>().text = mapNodeEnemySelection.levelData.enemyWeights[i].weight.ToString().ToUpper();
                string enemyType = mapNodeEnemySelection.levelData.enemyWeights[i].enemyType.ToString();
                enemyPoolPredictionsGameObject[i].GetComponent<Image>().sprite = SetEnemyIcon(enemyType);
            }
            else
            {
                // Set empty or placeholder text for unused slots
                enemyPoolPredictionsGameObject[i].GetComponentInChildren<TextMeshProUGUI>().text = "Empty Slot";
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
                return null;
        }
    }
}