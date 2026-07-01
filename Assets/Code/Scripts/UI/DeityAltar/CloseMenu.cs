using UnityEngine;
using UnityEngine.EventSystems;

public class CloseMenu : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] CanvasGroup _deityListCanvas;
    [SerializeField] CanvasGroup _mainCanvas;
    [SerializeField] private CanvasGroup _tutorialMenuCanvas;

    public void OnPointerClick(PointerEventData eventData)
    {
        CloseWindow();
    }

    private void CloseWindow()
    {
        if (_deityListCanvas != null && _mainCanvas != null)
        {
            _deityListCanvas.alpha = 0;
            _deityListCanvas.blocksRaycasts = false;
            _mainCanvas.alpha = 1;
        }

        if (_tutorialMenuCanvas != null) // Quick logic addition for handling pause menu flow in Battle scene.
        {
            _tutorialMenuCanvas.alpha = 0;
            _tutorialMenuCanvas.blocksRaycasts = false;
        }
    }
}