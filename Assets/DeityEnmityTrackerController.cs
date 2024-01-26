using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DeityEnmityTrackerController : MonoBehaviour
{
    public void OnEnable()
    {
        Deity.OnDeitySpawn += SetDeity;
        SpellcastingController.OnCastedSpellTypeHatedbyDeity += UpdateDeityEnmityTracker;
    }
    public void OnDisable()
    {
        Deity.OnDeitySpawn -= SetDeity;
        SpellcastingController.OnCastedSpellTypeHatedbyDeity -= UpdateDeityEnmityTracker;
    }

    public TextMeshProUGUI deityEnmityTracker;
    public Deity deity;
    public void SetDeity(GameObject deityGO)
    {
        deity = deityGO.GetComponent<Deity>();
    }
    public void UpdateDeityEnmityTracker()
    {
        deityEnmityTracker.text = deity.enmity.ToString();
    }
}
