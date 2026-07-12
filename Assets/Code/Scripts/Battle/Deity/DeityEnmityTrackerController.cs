using UnityEngine;
using TMPro;
using DG.Tweening;
using ProjectEdelweiss.Utils;

public class DeityEnmityTrackerController : MonoBehaviour
{
    public TextMeshProUGUI deityEnmityPointsCounter;
    public Deity deity;
    [SerializeField] RectTransform enmityIconRect;
    [SerializeField] Animator _enmityBarVFX;
    public void SetDeity(GameObject deityGO)
    {
        deity = deityGO.GetComponent<Deity>();
    }

    public void PlayEnmityIconFeedback()
    {
        // Kill any existing animations to prevent stacking
        DOTween.Kill(enmityIconRect);

        // Store original scale
        Vector3 originalScale = enmityIconRect.localScale;

        // Scale pop factor (1.6x of current size)
        float scaleFactor = 1.6f;
        Vector3 targetScale = originalScale * scaleFactor;

        // Use a proportion of the icon's height for jump power
        float iconHeight = enmityIconRect.rect.height * originalScale.y;
        float jumpPower = iconHeight * 0.5f; // 50% of height (adjust as needed)

        // Reset to original scale (optional if already set)
        enmityIconRect.localScale = originalScale;

        // Pop and bounce
        enmityIconRect.DOScale(targetScale, 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                enmityIconRect.DOScale(originalScale, 0.2f)
                    .SetEase(Ease.InOutSine);
            });

        // Twitchy vertical jump
        enmityIconRect.DOJumpAnchorPos(
            enmityIconRect.anchoredPosition,
            jumpPower,
            2, // number of jumps
            0.35f
        ).SetEase(Ease.OutQuad);
    }

    public void PlayFullBarFeedback()
    {
        //_enmityBarVFX.SetTrigger(GameTags.FULL_ENMITY_BAR);
    }

    public void StopFullBarFeedback()
    {
        //_enmityBarVFX.SetTrigger(GameTags.RESET_ENMITY_BAR);
    }
}