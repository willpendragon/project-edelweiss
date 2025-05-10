using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialFlowController : MonoBehaviour
{
    [SerializeField] RectTransform tutorialPanel;
    [SerializeField] TextMeshProUGUI tutorialText;
    [SerializeField] string[] tutorialTexts;
    // Start is called before the first frame update

    // Add a class that inhibits the possibility to click for the Player
    private void OnEnable()
    {
        MovePlayerAction.OnUnitMovedToTile += CompleteFirstTutorial;
    }

    private void OnDisable()
    {
        MovePlayerAction.OnUnitMovedToTile -= CompleteFirstTutorial;
    }

    private void Start()
    {
        //Transform spellMenuContainer = GameObject.FindGameObjectWithTag("MovesPanel").transform;

        EnlargeTutorialPanel();
        if (tutorialText != null)
        {
            tutorialText.text = tutorialTexts[0];
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void CompleteFirstTutorial(TileController tile)
    {
        EnlargeTutorialPanel();
        tutorialText.text = tutorialTexts[1];
        Debug.Log("First Tutorial Completed");
    }
    public void MinimizeTutorialPanel()
    {
        tutorialPanel.localScale = Vector3.zero;
    }

    private void EnlargeTutorialPanel()
    {
        tutorialPanel.localScale = Vector3.one;
    }
}