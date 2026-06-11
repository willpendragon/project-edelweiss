using TMPro;
using UnityEngine;

public class ItemCounterUIHelper : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textCounter;
    [SerializeField] private CanvasGroup _canvasGroup;

    public void DisplayTributesCounter()
    {
        _canvasGroup.alpha = 1.0f;
        GameSaveData saveData = SaveStateManager.saveData;
        _textCounter.text = $"x {saveData.resourceData.captureCrystalsCount}";
    }
}
