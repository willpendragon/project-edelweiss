using System.Collections;
using UnityEngine.Rendering;
using UnityEngine;

public class StageMoodController : MonoBehaviour
{
    public static StageMoodController Instance { get; private set; }
    [SerializeField] private Volume globalVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ActivateDarkness()
    {
        if (globalVolume == null)
        {
            Debug.LogError("Global Volume is not assigned.");
            return;
        }
        globalVolume.weight = 1f;
    }

    public void DeactivateDarkness()
    {
        if (globalVolume == null)
        {
            Debug.LogError("Global Volume is not assigned.");
            return;
        }
        globalVolume.weight = 0f;
    }

    public void StartResetDarkness(float delay)
    {
        StartCoroutine(ResetDarknessCoroutine(delay));
    }

    private IEnumerator ResetDarknessCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        DeactivateDarkness();
    }
}
