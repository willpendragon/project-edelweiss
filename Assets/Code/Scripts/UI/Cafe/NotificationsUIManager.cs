using UnityEngine;
using TMPro;

public class NotificationsUIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _notificationsText;
    public void DisplayTooltip(string message)
    {
        _notificationsText.text = message;
    }

    public void ResetToolTip()
    {
        _notificationsText.text = "";
    }
}
