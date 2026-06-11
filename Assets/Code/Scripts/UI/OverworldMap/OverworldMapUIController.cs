using UnityEngine;
using TMPro;
using DG.Tweening;

public class OverworldMapUIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI warFundsCounter;
    [SerializeField] TextMeshProUGUI crystalsCounter;
    [SerializeField] GameObject locationInfoPanel;
    [SerializeField] CanvasGroup _arrows;
    public GameStatsManager gameStatsManager;
    void Start()
    {
        gameStatsManager.warFunds.ToString();
        DOVirtual.DelayedCall(0.1f, UpdateTributesCounter);
        warFundsCounter.text = $"War Funds <sprite=93> {gameStatsManager.warFunds}";
        locationInfoPanel?.SetActive(false);
    }

    private void UpdateTributesCounter()
    {
        crystalsCounter.text = $"Tributes <sprite=98> {gameStatsManager.captureCrystalsCount}";
    }

    public void SetArrowsVisibility(float alpha)
    {
        _arrows.alpha = alpha;
    }
}
