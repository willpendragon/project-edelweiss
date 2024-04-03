using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitUIController : MonoBehaviour
{
    public Text unitDamageCounter;
    public Animator unitDamageCounterAnimator;
    public void SetDamageCounter(float receivedDamage)
    {
        unitDamageCounter.text = receivedDamage.ToString();
        unitDamageCounterAnimator.SetTrigger("activate_damage_counter");
        unitDamageCounterAnimator.SetTrigger("reset_damage_counter");
        StartCoroutine("ResetDamageCounter");
    }

    IEnumerator ResetDamageCounter()
    {
        yield return new WaitForSeconds(0.5f);
        unitDamageCounter.text = "";
    }
}
