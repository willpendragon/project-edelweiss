using UnityEngine;
using TMPro;

public class FaithControllerDialogueUIHelper : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI notificationsText;
    public void DisplayTextNotification(string textNotification)
    {
        if (notificationsText != null)
        {
            notificationsText.text = textNotification;
        }
    }
}
