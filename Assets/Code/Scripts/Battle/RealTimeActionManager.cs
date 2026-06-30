using System;
using System.Collections;
using System.Collections.Generic;
using ProjectEdelweiss.Utils;
using UnityEngine;

public class RealTimeActionManager : MonoBehaviour
{
    public enum ParryState { Idle, Windup, WindowOpen }

    [Header("Configuration")] 
    [SerializeField] private ParryConfig config;

    private ParryState currentState = ParryState.Idle;
    private bool isBotched = false;
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
            if (_instance == null) _instance = FindObjectOfType<RealTimeActionManager>();
            return _instance;
        }
    }

    private void Update()
    {
        // LA TRAPPOLA ANTI-MASHING
        // Se preme il pulsante mentre il nemico sta caricando l'attacco, si brucia l'occasione!
        if (Input.GetKeyDown(config.parryKey))
        {
            if (currentState == ParryState.Windup)
            {
                isBotched = true;
            }
        }
    }

    /// <summary>
    /// Va chiamato all'inizio dell'animazione di attacco del nemico.
    /// </summary>
    public void StartWindup()
    {
        currentState = ParryState.Windup;
        isBotched = false; // Resetta lo stato di fallimento per il nuovo attacco
    }

    public void OpenParryWindow(Unit targetPlayerUnit)
    {
        if (currentState == ParryState.WindowOpen) return;
        StartCoroutine(ParryWindowRoutine(targetPlayerUnit));
    }

    private IEnumerator ParryWindowRoutine(Unit targetPlayerUnit)
    {
        currentState = ParryState.WindowOpen;
        hasAttemptedParry = false;
        wasParrySuccessful = false;
        float timer = 0f;

        Debug.Log("Parry Window Opened! Press " + config.parryKey + " NOW!");
        Vector3 offset = new Vector3(1f, 5f, 0f);
        GameObject exclamationMarkObject = Instantiate(_exclamationMark, targetPlayerUnit.transform.position + offset, targetPlayerUnit.transform.rotation);
        
        while (timer < config.windowOfOpportunity)
        {
            if (Input.GetKeyDown(config.parryKey) && !hasAttemptedParry)
            {
                Destroy(exclamationMarkObject);
                hasAttemptedParry = true;

                // CONTROLLA SE AVEVA SPAMMATO PRIMA CHE SI APRISSE LA FINESTRA
                if (isBotched)
                {
                    wasParrySuccessful = false;
                    Debug.Log("<color=red>Parry failed: You mashed or pressed too early!</color>");
                    break;
                }
                else
                {
                    wasParrySuccessful = true;
                    Debug.Log("<color=green>Attack parried successfully!</color>");
                    OnParrySuccess?.Invoke();
                    break;
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Risoluzione fallimento (tempo scaduto o mashing)
        if (!wasParrySuccessful)
        {
            if (exclamationMarkObject != null) Destroy(exclamationMarkObject);

            if (isBotched && hasAttemptedParry)
                Debug.Log("<color=red>Parry failed: Botched from mashing!</color>");
            else if (hasAttemptedParry)
                Debug.Log("<color=red>Parry failed: Pressed too late!</color>");
            else
                Debug.Log("<color=red>Parry failed: Missed the window!</color>");

            OnParryFailure?.Invoke();
        }

        currentState = ParryState.Idle; // Torna a riposo per il prossimo attacco
    }
}