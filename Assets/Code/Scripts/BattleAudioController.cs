using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class BattleAudioController : MonoBehaviour
{
    [SerializeField] AudioSource regularBattleAudioSource;
    [SerializeField] AudioSource deityBattleAudioSource;
    [SerializeField] AudioSource battleVictoryAudioSource;
    [SerializeField] AudioSource battleDefeatAudioSource;
    [SerializeField] AudioSource cancelMoveSelectionAudioSource;
    private void OnEnable()
    {
        BattleFlowController.OnBattleEnd += BattleEndTheme;
    }
    private void OnDisable()
    {
        BattleFlowController.OnBattleEnd -= BattleEndTheme;
    }
    void Start()
    {
        switch (BattleTypeController.Instance.currentBattleType)
        {
            case BattleTypeController.BattleType.RegularBattle:
                regularBattleAudioSource?.Play();
                break;

            case BattleTypeController.BattleType.BattleWithDeity:
                deityBattleAudioSource?.Play();
                break;
        }
    }

    public void BattleEndTheme(string battleEndResult)
    {
        switch (battleEndResult)
        {
            case "Victory":
                regularBattleAudioSource?.Stop();
                deityBattleAudioSource?.Stop();
                StartCoroutine("PlayVictoryTheme");
                break;
            case "Defeat":
                regularBattleAudioSource?.Stop();
                deityBattleAudioSource?.Stop();
                StartCoroutine("PlayDefeatTheme");
                break;
        }
    }
    IEnumerator PlayVictoryTheme()
    {
        yield return new WaitForSeconds(0.5f);
        battleVictoryAudioSource?.Play();
    }
    IEnumerator PlayDefeatTheme()
    {
        yield return new WaitForSeconds(0.5f);
        battleDefeatAudioSource?.Play();
    }

    public void PlayCancelMoveSelectionSound()
    {
        cancelMoveSelectionAudioSource?.Play();
    }
}