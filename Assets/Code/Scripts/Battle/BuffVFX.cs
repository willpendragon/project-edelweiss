using UnityEngine;

public class BuffVFX : MonoBehaviour
{
    [SerializeField] MeshRenderer _buffMesh;
    public void TriggerVFX()
    {
        // Shows the protective Summon Buff - prototype basic logic
        if (_buffMesh == null)
            return;
        _buffMesh.material.SetFloat("_GlowIntensity", 1f);
        _buffMesh.material.SetFloat("_FadeHeight", 5f);
    }
}