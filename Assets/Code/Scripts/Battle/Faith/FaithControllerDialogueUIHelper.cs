using UnityEngine;
using TMPro;
using DG.Tweening;

public class FaithControllerDialogueUIHelper : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI notificationsText;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float popScale = 1.2f;
    [SerializeField] private float popUpDuration = 0.15f;
    [SerializeField] private float popDownDuration = 0.1f;

    private Tween currentTween;

    private void Awake()
    {
        if (notificationsText != null)
        {
            notificationsText.alpha = 0f;
            notificationsText.transform.localScale = Vector3.one;
        }
    }
    public void DisplayTextNotification(string textNotification)
    {
        if (notificationsText == null) return;

        currentTween?.Kill();

        notificationsText.text = textNotification;
        notificationsText.alpha = 0f;
        notificationsText.transform.localScale = Vector3.zero; // start from nothing

        currentTween = DOTween.Sequence()
            // Appear: fade in + scale up smoothly from zero
            .Append(notificationsText.DOFade(1f, 0.3f))
            .Join(notificationsText.transform.DOScale(1.15f, 0.4f).SetEase(Ease.OutBack))

            // Slight "squash" back to normal for satisfying bounce
            .Append(notificationsText.transform.DOScale(1f, 0.2f).SetEase(Ease.InOutSine))

            // Hold at full size
            .AppendInterval(displayDuration)

            // Fade out while shrinking
            .Append(notificationsText.DOFade(0f, 0.4f))
            .Join(notificationsText.transform.DOScale(0.8f, 0.4f).SetEase(Ease.InBack))

            .SetEase(Ease.InOutQuad);
    }
}