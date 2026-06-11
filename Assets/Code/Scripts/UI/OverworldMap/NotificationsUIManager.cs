using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class NotificationsUIManager : MonoBehaviour
{
    public static NotificationsUIManager Instance;
    private float _notificationDelay = 1.5f;
    private GameObject _messageBox;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TextMeshProUGUI _footerNotifications;
    private List<string> _eventNotifications = new List<string>();
    private float _marqueeResetDelay = 1.5f;


    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void AddMessageToQueue(string message)
    {
        _eventNotifications.Add(message);
    }

    public void Start()
    {
        Invoke("DisplayNotifications", 1f);
    }

    public void DisplayNotifications()
    {
        StartCoroutine(ProcessNotifications());
    }

    private IEnumerator ProcessNotifications()
    {
        foreach (var stringMessage in _eventNotifications)
        {
            yield return new WaitForSeconds(_notificationDelay);
            ShowNotification(stringMessage);
        }
    }

    private void ShowNotification(string message)
    {
        GameObject newMessageBox = Instantiate(_messageBox, _canvas.transform);
        // Logic to set the text inside your message box would go here
        AssignImage(newMessageBox);
        Destroy(newMessageBox, _notificationDelay);
    }

    private void AssignImage(GameObject newMessageBox)
    {
        throw new NotImplementedException();
    }

    public void DisplayFooterNotification(string message)
    {
        string previousMessage = _footerNotifications.text;
        _footerNotifications.text = message;
        DOVirtual.DelayedCall(_marqueeResetDelay, () => _footerNotifications.text = previousMessage);
    }
}