using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEditor.VersionControl;


public class NotificationsUIManager : MonoBehaviour
{
    private float _notificationDelay = 1.5f;
    private GameObject _messageBox;
    [SerializeField] private Canvas _canvas;
    private List<string> _eventNotifications = new List<string>();

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
}
