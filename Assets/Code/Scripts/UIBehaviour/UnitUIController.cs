using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitUIController : MonoBehaviour
{
    public TextMeshProUGUI unitDamageCounter;
    public Animator unitDamageCounterAnimator;

    public TextMeshProUGUI unitBuffCounter;
    public Animator unitBuffCounterAnimator;

    public void OnEnable()
    {
        SummoningBuffController.OnAppliedDeityBuffMessage += SetBuffCounter;
    }

    public void OnDisable()
    {
        SummoningBuffController.OnAppliedDeityBuffMessage -= SetBuffCounter;
    }

    public void SetDamageCounter(float receivedDamage)
    {
        if (unitDamageCounter.text != null && unitDamageCounterAnimator != null)
        {
            unitDamageCounter.text = receivedDamage.ToString();
            unitDamageCounterAnimator.SetTrigger("activate_damage_counter");
            unitDamageCounterAnimator.SetTrigger("reset_damage_counter");
            StartCoroutine("ResetDamageCounter");
        }
    }

    IEnumerator ResetDamageCounter()
    {
        float damageCounterResetTime = 0.5f;
        yield return new WaitForSeconds(damageCounterResetTime);
        unitDamageCounter.text = "";
    }


    public void SetBuffCounter(string receivedBuff)
    {
        if (this.gameObject.GetComponent<Unit>().linkedDeity != null && unitBuffCounter != null && unitBuffCounterAnimator != null)
        {
            unitBuffCounter.text = receivedBuff;
            unitBuffCounterAnimator.SetTrigger("activate_buff_counter");
            unitBuffCounterAnimator.SetTrigger("reset_buff_counter");
            StartCoroutine("ResetBuffCounter");
        }
    }
    IEnumerator ResetBuffCounter()
    {
        float buffCounterResetTime = 0.5f;
        yield return new WaitForSeconds(buffCounterResetTime);
        unitBuffCounter.text = "";
    }


}
