using UnityEngine;
using DG.Tweening; // Ensure DoTween is installed and imported

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private float transitionDuration = 0.5f; // Animation duration
    private bool isPaused = false; // Track menu state
    private RectTransform canvasTransform; // RectTransform reference
    private Tween currentTween; // To track the ongoing tween animation

    private void Start()
    {
        // Get the RectTransform of the canvas
        canvasTransform = GetComponent<RectTransform>();

        // Ensure the canvas starts completely hidden
        canvasTransform.localScale = Vector3.zero;
    }

    private void Update()
    {
        // Toggle pause menu on pressing ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    private void TogglePauseMenu()
    {
        // Ensure no animation is interrupted
        if (currentTween != null && currentTween.IsActive() && !currentTween.IsComplete())
            return;

        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        isPaused = true;

        // Scale up the canvas and delay freezing the game
        currentTween = canvasTransform.DOScale(Vector3.one, transitionDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // Freeze the game once the animation is complete
                Time.timeScale = 0f;
                currentTween = null; // Clear tween reference
            });
    }

    private void ResumeGame()
    {
        // Resume the game immediately
        Time.timeScale = 1f;

        isPaused = false;

        // Scale down the canvas
        currentTween = canvasTransform.DOScale(Vector3.zero, transitionDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                currentTween = null; // Clear tween reference
            });
    }
}
