using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DeityEnmityTrackerController : MonoBehaviour
{
    public void OnEnable()
    {
        SpellcastingController.OnCastedSpellTypeHatedbyDeity += UpdateDeityEnmityTracker;
    }
    public void OnDisable()
    {
        SpellcastingController.OnCastedSpellTypeHatedbyDeity -= UpdateDeityEnmityTracker;
    }
    public TextMeshProUGUI deityEnmityPointsCounter;
    public Deity deity;
    public void SetDeity(GameObject deityGO)
    {
        deity = deityGO.GetComponent<Deity>();
    }
    public void UpdateDeityEnmityTracker()
    {
        Debug.Log("Updating Enmity Tracker");
        deityEnmityPointsCounter.text = deity.enmity.ToString();
    }
}
