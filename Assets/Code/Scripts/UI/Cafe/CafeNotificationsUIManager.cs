using UnityEngine;
using TMPro;

public class CafeNotificationsUIManager : NotificationsUIManager
{
    [SerializeField] TextMeshProUGUI _notificationsText;

    // Notifications fire when the user moves the cursor over the Café Menu button.
    public void DisplayTooltip(string message)
    {
        _notificationsText.text = message;
    }

    public void ResetToolTip()
    {
        _notificationsText.text = "";
    }
}
