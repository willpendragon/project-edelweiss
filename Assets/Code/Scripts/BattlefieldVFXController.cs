using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlefieldVFXController : MonoBehaviour
{
    public ParticleSystem deityFieldEffectVFX;
    public void OnEnable()
    {
        Deity.OnDeityFieldEffectActivation += PlayDeityFieldEffectVFX;
    }
    public void OnDisable()
    {
        Deity.OnDeityFieldEffectActivation -= PlayDeityFieldEffectVFX;
    }
    public void PlayDeityFieldEffectVFX()
    {
        deityFieldEffectVFX.Play();
    }
}
