using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealTimeActionManager : MonoBehaviour
{
    [Header("Configuration")] [SerializeField]
    private ParryConfig config;

    private bool isWindowOpen = false;
    private bool hasAttemptedParry = false;
    private bool wasParrySuccessful = false;

    public event Action OnParrySuccess;
    public event Action OnParryFailure;

    private static RealTimeActionManager _instance;

    public static RealTimeActionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<RealTimeActionManager>();
            }

            return _instance;
        }
    }

    public void OpenParryWindow()
    {
        if (isWindowOpen) return;

        StartCoroutine(ParryWindowRoutine());
    }

    private IEnumerator ParryWindowRoutine()
    {
        isWindowOpen = true;
        hasAttemptedParry = false;
        wasParrySuccessful = false;

        float timer = 0f;

        Debug.Log("Parry Window Opened! Press " + config.parryKey + " NOW!");

        // Wait for input or until the window expires
        while (timer < config.windowOfOpportunity)
        {
            if (Input.GetKeyDown(config.parryKey) && !hasAttemptedParry)
            {
                hasAttemptedParry = true;
                wasParrySuccessful = true;

                Debug.Log("<color=green>Attack parried successfully!</color>");
                OnParrySuccess?.Invoke();

                // Break out early so we don't keep polling
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // If the timer ran out and we didn't succeed
        if (!wasParrySuccessful)
        {
            if (hasAttemptedParry)
                Debug.Log("<color=red>Parry failed: Pressed too early/late!</color>");
            else
                Debug.Log("<color=red>Parry failed: Missed the window!</color>");

            OnParryFailure?.Invoke();
        }

        isWindowOpen = false;
    }
}