using UnityEngine;
using DG.Tweening;

public class MaskFeedbackHelper : MonoBehaviour
{
    [SerializeField] SpriteRenderer maskSprite;
    [SerializeField] Animator maskSpriteAnimator;

    public void DeactivateMask()
    {
        PlayMaskDeactivationFeedback();
    }

    private void PlayMaskDeactivationFeedback()
    {
        if (maskSprite == null)
        {
            Debug.LogWarning("Mask Sprite is not assigned!");
            return;
        }
        maskSpriteAnimator?.SetTrigger("MaskRemoved");
    }
}