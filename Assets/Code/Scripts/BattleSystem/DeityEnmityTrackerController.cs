using UnityEngine;
using TMPro;

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
