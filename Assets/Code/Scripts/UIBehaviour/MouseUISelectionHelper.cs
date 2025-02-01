using UnityEngine;

public class MouseUISelectionHelper : MonoBehaviour
{
    [SerializeField] Animator mouseClickSelectionIconAnimator;
    private void Start()
    {
        ShowMouseIconLeftClick();
    }
    public void ShowMouseIconLeftClick()
    {
        if (mouseClickSelectionIconAnimator != null)
        {
            mouseClickSelectionIconAnimator.SetTrigger("ShowLMBIcon");
        }
    }
    public void HideMouseIconLeftClick()
    {
        if (mouseClickSelectionIconAnimator != null)
        {
            mouseClickSelectionIconAnimator.SetTrigger("HideLMBIcon");
        }
    }

    public void ShowMouseIconRightClick()
    {
        if (mouseClickSelectionIconAnimator != null)
        {
            mouseClickSelectionIconAnimator.SetTrigger("ShowRMBIcon");
        }
    }

    public void HideMouseIconRightClick()
    {
        if (mouseClickSelectionIconAnimator != null)
        {
            mouseClickSelectionIconAnimator.SetTrigger("HideRMBIcon");
        }
    }
}
