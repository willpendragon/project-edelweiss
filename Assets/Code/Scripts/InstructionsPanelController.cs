using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InstructionsPanelController : MonoBehaviour
{
    public static InstructionsPanelController Instance { get; private set; }

    [SerializeField] TextMeshProUGUI leftMouseButtonText;
    [SerializeField] TextMeshProUGUI rightMouseButtonText;
    [SerializeField] TextMeshProUGUI actionModeText;
    [SerializeField] RectTransform instructionsPanelContainer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;
    }
    private void Start()
    {
        MinimizePanel();
    }

    public void UpdateInstructions(string leftMouseButtonInstructionsText, string rightMouseButtonInstructionsText)
    {
        EnlargePanel();
        UpdateLeftMouseButtonText(leftMouseButtonInstructionsText);
        UpdateRightMouseButtonText(rightMouseButtonInstructionsText);
    }

    private void UpdateLeftMouseButtonText(string leftMouseButtonInstructionsText)
    {
        leftMouseButtonText.text = leftMouseButtonInstructionsText;
    }

    private void UpdateRightMouseButtonText(string rightMouseButtonInstructionsText)
    {
        rightMouseButtonText.text = rightMouseButtonInstructionsText;
    }
    public void EnlargePanel()
    {
        instructionsPanelContainer.localScale = Vector3.one;
    }
    public void MinimizePanel()
    {
        instructionsPanelContainer.localScale = Vector3.zero;
    }
}
