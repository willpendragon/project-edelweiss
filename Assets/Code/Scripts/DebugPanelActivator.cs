using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPanelActivator : MonoBehaviour
{
    public enum PanelStatus
    {
        open,
        closed
    }
    public GameObject debugPanel;
    public PanelStatus currentPanelStatus;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && currentPanelStatus == PanelStatus.closed)
        {
            debugPanel.SetActive(true);
            currentPanelStatus = PanelStatus.open;
        }
        else if (Input.GetKeyDown(KeyCode.Q) && currentPanelStatus == PanelStatus.open)
        {
            debugPanel.SetActive(false);
            currentPanelStatus = PanelStatus.closed;
        }        
    }
}
