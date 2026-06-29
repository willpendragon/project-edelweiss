using System;
using System.Collections;
using System.Collections.Generic;
using ProjectEdelweiss.Utils;
using UnityEngine;

public class RealTimeActionManager : MonoBehaviour
{
    [Header("Configuration")] [SerializeField]
    private ParryConfig config;

    private bool isWindowOpen = false;
    private bool hasAttemptedParry = false;
    private bool wasParrySuccessful = false;

    // UI
    [SerializeField] private GameObject _exclamationMark;

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

    public void OpenParryWindow(Unit targetPlayerUnit)
    {
        if (isWindowOpen) return;

        StartCoroutine(ParryWindowRoutine(targetPlayerUnit));
    }

    private IEnumerator ParryWindowRoutine(Unit targetPlayerUnit)
    {
        isWindowOpen = true;
        hasAttemptedParry = false;
        wasParrySuccessful = false;

        float timer = 0f;

        Debug.Log("Parry Window Opened! Press " + config.parryKey + " NOW!");
        // Instantiate "exclamation mark" on the character's head".
        Vector3 offset = new Vector3(1f, 5f, 0f);
       GameObject exclamationMarkObject = Instantiate(_exclamationMark, targetPlayerUnit.transform.position + offset,
            targetPlayerUnit.transform.rotation);
        // Wait for input or until the window expires
        while (timer < config.windowOfOpportunity)
        {
            if (Input.GetKeyDown(config.parryKey) && !hasAttemptedParry)
            {
                Destroy(exclamationMarkObject);
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
            Destroy(exclamationMarkObject);
            if (hasAttemptedParry)
                Debug.Log("<color=red>Parry failed: Pressed too early/late!</color>");
            else
                Debug.Log("<color=red>Parry failed: Missed the window!</color>");

            OnParryFailure?.Invoke();
        }

        isWindowOpen = false;
    }
}