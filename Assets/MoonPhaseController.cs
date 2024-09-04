using System;
using UnityEngine;

public class MoonPhaseController : MonoBehaviour
{
    private void OnEnable()
    {
        //Subscribe to event from TurnTrackerCntroller
        TurnTrackerController.OnIncreaseTurnCounter += IncreaseLunarPhasesTurnCounter;
    }
    private void OnDisable()
    {
        //Subscribe to event from TurnTrackerCntroller
        TurnTrackerController.OnIncreaseTurnCounter -= IncreaseLunarPhasesTurnCounter;
    }
    public enum LunarPhases
    {
        PhaseZero,
        PhaseOne,
        PhaseTwo,
        PhaseThree,
    }

    private LunarPhases currentLunarPhase;
    [SerializeField] BossController bossController;
    [SerializeField] TurnTrackerController turnTrackerController;
    [SerializeField] float phaseOnebuffAmount;
    [SerializeField] float phaseTwobuffAmount;
    [SerializeField] float phaseThreebuffAmount;
    private int lunarPhasesTurnCounter;

    void Start()
    {
        currentLunarPhase = LunarPhases.PhaseZero;
    }

    // The Turn Tracker Controller sends a message with currentTurn as an argument and this class listens.

    private void IncreaseLunarPhasesTurnCounter()
    {
        if (CheckLunarPhasesCycle())
        {
            ResetLunarPhasesCycle();
        }
        else
        {
            lunarPhasesTurnCounter++;
        }
    }

    private void ResetLunarPhasesCycle()
    {
        lunarPhasesTurnCounter = 1;
    }

    private bool CheckLunarPhasesCycle()
    {
        int lunarPhasesCycleDuration = 7;
        if (lunarPhasesTurnCounter == lunarPhasesCycleDuration)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void SwitchLunarPhases(int lunarPhasesTurnCounter)
    {
        switch (lunarPhasesTurnCounter)
        {
            case 2:
                currentLunarPhase = LunarPhases.PhaseOne;
                Debug.Log(2);
                break;
            case 4:
                currentLunarPhase = LunarPhases.PhaseTwo;
                Debug.Log(4);
                break;
            case 6:
                currentLunarPhase = LunarPhases.PhaseThree;
                Debug.Log(6);
                break;
        }
    }

    // The Enemy Turn Controller calls this method.
    private void ActivateLunarPhaseBuff()
    {
        switch (currentLunarPhase)
        {
            // OK to use DeityPrayerBuff but will eventually need to rename it in a way
            // conforming to its multi-purpose nature

            case LunarPhases.PhaseZero:
                Debug.Log(LunarPhases.PhaseZero);
                break;
            case LunarPhases.PhaseOne:
                bossController.ApplyBuff(DeityPrayerBuff.AffectedStat.AttackPower, phaseOnebuffAmount);
                Debug.Log(LunarPhases.PhaseOne);
                break;
            case LunarPhases.PhaseTwo:
                bossController.ApplyBuff(DeityPrayerBuff.AffectedStat.MagicPower, phaseTwobuffAmount);
                Debug.Log(LunarPhases.PhaseTwo);
                break;
            case LunarPhases.PhaseThree:
                bossController.ApplyBuff(DeityPrayerBuff.AffectedStat.ShieldPower, phaseThreebuffAmount);
                Debug.Log(LunarPhases.PhaseThree);
                break;
        }
    }
}
