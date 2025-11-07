using UnityEngine;
using UnityEngine.EventSystems;

public class CloseMenu : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] CanvasGroup _deityListCanvas;
    [SerializeField] CanvasGroup _mainCanvas;

    public void OnPointerClick(PointerEventData eventData)
    {
        CloseWindow();
    }
    private void CloseWindow()
    {
        _deityListCanvas.alpha = 0;
        _deityListCanvas.blocksRaycasts = false;
        _mainCanvas.alpha = 1;
    }
}
