using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapNodeInfoPanelController : MonoBehaviour
{
    [SerializeField] GameObject mapNodeGameObject;
    [SerializeField] TextMeshProUGUI minPoolSizeText;
    [SerializeField] TextMeshProUGUI maxPoolSizeText;
    [SerializeField] GameObject mapNodeInfoPanelCanvas;
    public GameObject[] enemyPoolPredictionsGameObject;



    private void Start()
    {
        EnemySelection mapNodeEnemySelection = mapNodeGameObject.GetComponent<EnemySelection>();

        minPoolSizeText.text = mapNodeEnemySelection.levelData.minEnemyPoolSize.ToString();
        maxPoolSizeText.text = mapNodeEnemySelection.levelData.maxEnemyPoolSize.ToString();

        for (int i = 0; i < enemyPoolPredictionsGameObject.Length; i++)
        {

            if (i < mapNodeEnemySelection.EnemyTypeIds.Count)
            {
                // Assign enemy type text if within range
                enemyPoolPredictionsGameObject[i].GetComponentInChildren<TextMeshProUGUI>().text = mapNodeEnemySelection.EnemyTypeIds[i].ToString().ToUpper();
            }
            else
            {
                // Set empty or placeholder text for unused slots
                enemyPoolPredictionsGameObject[i].GetComponentInChildren<TextMeshProUGUI>().text = "Empty Slot";
            }
        }
    }

    private void SetEnemyIcon(string enemyId, Image enemyIcon)
    {
        // Pass the Icon and set it
        Image dummy1Image;
        switch (enemyId)
        {
            case "dummy1":
                dummy1Image = enemyIcon;
                break;
            case "dummy2":
                break;

        }
    }
}