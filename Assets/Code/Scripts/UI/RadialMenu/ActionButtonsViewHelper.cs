using UnityEngine;
using UnityEngine.UI;

public class ActionButtonsViewHelper : MonoBehaviour
{
    private const string ACTION_BUTTON_TAG = "PlayerUISpellButton";
    private const string ACTION_BUTTON_LAYER = "ActionButton";
    [SerializeField] Slider opportunityPointsSlider;
    public void GrayOutUnavailableButtons()
    {
        if (!OpportunityPointsAvailable())
        {
            GameObject[] actionButtons = GameObject.FindGameObjectsWithTag(ACTION_BUTTON_TAG);
            if (actionButtons == null || actionButtons.Length == 0)
            {
                return;
            }

            foreach (var actionButton in actionButtons)
            {
                if (actionButton.layer == LayerMask.NameToLayer(ACTION_BUTTON_LAYER))
                {
                    Button actionButtonUIComponent = actionButton.GetComponent<Button>();
                    actionButtonUIComponent.interactable = false;
                }
            }
        }
    }
    private bool OpportunityPointsAvailable()
    {
        if (opportunityPointsSlider.value > 0)
        {
            return true;
        }

        else if (opportunityPointsSlider.value <= 0)
        {
            return false;
        }

        else
        {
            return default;
        }
    }
}