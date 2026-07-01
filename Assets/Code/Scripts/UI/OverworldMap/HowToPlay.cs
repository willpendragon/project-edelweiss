using DG.Tweening;
using ProjectEdelweiss.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class HowToPlay : MonoBehaviour, IPointerClickHandler // This class also handles the map pause menu
{
    [SerializeField] private CanvasGroup _mapCanvas;
    [SerializeField] private CanvasGroup _tutorialCanvas;
    [SerializeField] private CanvasGroup _pauseMenuCanvas;
    [SerializeField] private OverworldMapUIController _mapMenuController;

    void Start()
    {
        _mapMenuController = FindAnyObjectByType<OverworldMapUIController>();
    }

    public void OpenMenu()
    {
        if (_mapCanvas != null)
        {
            _mapCanvas.alpha = 0.8f;
        }

        if (_pauseMenuCanvas != null)
        {
            _pauseMenuCanvas.alpha = 1f;
            _pauseMenuCanvas.interactable = true;
            _pauseMenuCanvas.blocksRaycasts = true;
            Time.timeScale = 0f;
        }

        Scene currentScene = SceneManager.GetActiveScene();

        if (_tutorialCanvas != null &&
            currentScene.name == GameTags.OVERWORLD_MAP) // Quick Fix, this logic will happen only in map scene
        {
            _tutorialCanvas.alpha = 1f;
            _tutorialCanvas.interactable = true;
            _tutorialCanvas.blocksRaycasts = true;
            Time.timeScale = 0f;
        }
        // // Hide navigation arrows
        // _mapMenuController.SetArrowsVisibility(0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ResumeGame();
    }

    public void ResumeGame()
    {
        if (_mapCanvas != null)
        {
            _mapCanvas.alpha = 1f;
        }

        if (_tutorialCanvas != null)
        {
            _tutorialCanvas.alpha = 0f;
            _tutorialCanvas.interactable = false;
            _tutorialCanvas.blocksRaycasts = false;
        }

        if (_pauseMenuCanvas != null)
        {
            _pauseMenuCanvas.alpha = 0f;
            _pauseMenuCanvas.interactable = false;
            _pauseMenuCanvas.blocksRaycasts = false;
        }


        // Resume the game immediately
        Time.timeScale = 1f;

        // // Show navigation arrows
        // _mapMenuController.SetArrowsVisibility(1f);
    }
}