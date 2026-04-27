using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class NotificationRequest
{
    public NotificationConfig config;
    public string title;
    public string description;

    public NotificationRequest(NotificationConfig config, string title, string description, string category)
    {
        this.config = config;
        this.title = title;
        this.description = description;
    }
}

public class EventsUIManager : MonoBehaviour
{
    [SerializeField] private Image _eventImage;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _eventText;
    [SerializeField] private TextMeshProUGUI _categoryText;

    // The queue processes the notifications in an orderly manner.
    private Queue<NotificationRequest> _queue = new Queue<NotificationRequest>();
    private bool _isShowing = false;

    public void AddNotification(NotificationConfig config, string title, string description, string category)
    {
        _queue.Enqueue(new NotificationRequest(config, title, description, category));

        if (!_isShowing)
        {
            ProcessQueue();
        }
    }

    private void ProcessQueue()
    {
        if (_queue.Count == 0)
        {
            _isShowing = false;
            return;
        }

        _isShowing = true;
        NotificationRequest current = _queue.Dequeue();

        _eventImage.sprite = current.config.icon;
        _eventText.text = current.title;
        _categoryText.text = current.config.categoryName;

        Sequence s = DOTween.Sequence();

        _canvasGroup.blocksRaycasts = true;
        s.Append(_canvasGroup.DOFade(1f, 0.5f))
         .AppendInterval(2.5f)
         .Append(_canvasGroup.DOFade(0f, 0.5f))
         .OnComplete(() =>
         {
             _canvasGroup.blocksRaycasts = false;
             DOVirtual.DelayedCall(0.2f, () => ProcessQueue());
         });
    }
}
