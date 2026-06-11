using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class HowToPlay : MonoBehaviour, IPointerClickHandler // This class also handles the map pause menu
{
    [SerializeField] private CanvasGroup _mapCanvas;
    [SerializeField] private CanvasGroup _tutorialCanvas;
    [SerializeField] private OverworldMapUIController _mapMenuController;

    void Start()
    {
        _mapMenuController = FindAnyObjectByType<OverworldMapUIController>();
    }

    public void OpenMenu()
    {
        _mapCanvas.alpha = 0.8f;

        _tutorialCanvas.alpha = 1f;
        _tutorialCanvas.interactable = true;
        _tutorialCanvas.blocksRaycasts = true;
        Time.timeScale = 0f;

        // Hide navigation arrows
        _mapMenuController.SetArrowsVisibility(0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ResumeGame();
    }

    public void ResumeGame()
    {
        _mapCanvas.alpha = 1f;
        _tutorialCanvas.alpha = 0f;
        _tutorialCanvas.interactable = false;
        _tutorialCanvas.blocksRaycasts = false;

        // Resume the game immediately
        Time.timeScale = 1f;

        // Show navigation arrows
        _mapMenuController.SetArrowsVisibility(1f);
    }
}
