using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNotificationConfig", menuName = "Notifications/Config")]
public class NotificationConfig : ScriptableObject
{
    // Add also type to switch different types of Notification aesthetics.

    public string categoryName;
    public Sprite icon;
    public Color themeColor;
    public int priority;
    public float displayDuration = 3f;
}