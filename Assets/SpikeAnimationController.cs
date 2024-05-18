using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeAnimationController : MonoBehaviour
{
    public void OnEnable()
    {
        TrapController.OnTrapAction += AnimateSpikes;
    }
    public void OnDisable()
    {
        TrapController.OnTrapAction -= AnimateSpikes;
    }

    public Animator spikeAnimator;

    public void AnimateSpikes()
    {
        if (spikeAnimator != null)
        {
            spikeAnimator.SetTrigger("ActivateTrap");
            Debug.Log("Playing Spike Animation");
        }
    }

}
