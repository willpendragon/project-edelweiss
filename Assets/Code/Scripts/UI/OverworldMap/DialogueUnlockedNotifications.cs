using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueUnlockedNotifications : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _title;
    [SerializeField] RectTransform _popupTransform;
    [SerializeField] CanvasGroup _canvasGroup;

    private void OnEnable()
    {
        ConversationManager.OnDialogueUnlocked += ShowNotification;
    }
    private void OnDisable()
    {
        ConversationManager.OnDialogueUnlocked -= ShowNotification;
    }

    private void ShowNotification(string title)
    {
        _canvasGroup.alpha = 1;
        _popupTransform.localScale = Vector3.zero;
        _popupTransform.DOScale(Vector3.one, 0.3f)
            .SetEase(Ease.OutBack);
        _title.text = title;
        Invoke("HidePopup", 2f);
    }

    public void HidePopup()
    {
        _popupTransform.DOScale(Vector3.zero, 0.25f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                _canvasGroup.alpha = 0;
            });
    }
}
