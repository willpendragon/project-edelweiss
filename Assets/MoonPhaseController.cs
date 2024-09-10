using UnityEngine;

public class MoonPhaseController : MonoBehaviour
{
    private void OnEnable()
    {
        //Subscribe to event from TurnTrackerCntroller
        TurnTrackerController.OnIncreaseTurnCounter += IncreaseLunarPhasesTurnCounter;
        TurnController.OnEnemyTurnSwap += ActivateLunarPhaseBuff;

    }
    private void OnDisable()
    {
        //Subscribe to event from TurnTrackerCntroller
        TurnTrackerController.OnIncreaseTurnCounter -= IncreaseLunarPhasesTurnCounter;
        TurnController.OnEnemyTurnSwap -= ActivateLunarPhaseBuff;
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

    private void Start()
    {
        currentLunarPhase = LunarPhases.PhaseZero;
    }

    // The Turn Tracker Controller sends a message, this class listens and fires this method.

    private void IncreaseLunarPhasesTurnCounter()
    {
        if (CheckLunarPhasesCycle())
        {
            ResetLunarPhasesCycle();
        }
        else
        {
            lunarPhasesTurnCounter++;
            // Lunar Phases counter increases. 
            SwitchLunarPhases(lunarPhasesTurnCounter);
            // Calling the method to Switch the Lunar Phases using the current Counter as a parameter.
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
                Debug.Log("Case " + 2 + "Switching to Lunar Phase One");
                break;
            case 4:
                currentLunarPhase = LunarPhases.PhaseTwo;
                Debug.Log("Case " + 4 + "Switching to Lunar Phase Two");
                break;
            case 6:
                currentLunarPhase = LunarPhases.PhaseThree;
                Debug.Log("Case " + 6 + "Switching to Lunar Phase Three");
                break;
        }
    }
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
