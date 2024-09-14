using UnityEngine;
using UnityEngine.UI;

public class MoonPhaseController : MonoBehaviour
{
    public delegate void MoonPhaseSwitch(string moonPhaseName);
    public static event MoonPhaseSwitch OnMoonPhaseSwitch;

    public delegate void MoonPhaseBuffActivation(string moonPhaseBuffName);
    public static event MoonPhaseBuffActivation OnMoonPhaseBuffActivation;

    [SerializeField] Image attackPowerBuffIcon;
    [SerializeField] Image magicPowerBuffIcon;
    [SerializeField] Image shieldPowerBuffIcon;

    private void OnEnable()
    {
        //Subscribe to event from TurnTrackerCntroller
        TurnTrackerController.OnIncreaseTurnCounter += IncreaseMoonPhasesTurnCounter;
        TurnController.OnEnemyTurnSwap += ActivateLunarPhaseBuff;
    }
    private void OnDisable()
    {
        //Subscribe to event from TurnTrackerCntroller
        TurnTrackerController.OnIncreaseTurnCounter -= IncreaseMoonPhasesTurnCounter;
        TurnController.OnEnemyTurnSwap -= ActivateLunarPhaseBuff;
    }
    public enum MoonPhases
    {
        PhaseZero,
        PhaseOne,
        PhaseTwo,
        PhaseThree,
    }

    public MoonPhases currentMoonPhase;
    [SerializeField] BossController bossController;
    [SerializeField] TurnTrackerController turnTrackerController;
    [SerializeField] float phaseOnebuffAmount;
    [SerializeField] float phaseTwobuffAmount;
    [SerializeField] float phaseThreebuffAmount;
    private int lunarPhasesTurnCounter;

    private void Start()
    {
        currentMoonPhase = MoonPhases.PhaseZero;

        attackPowerBuffIcon.color = Color.black;
        magicPowerBuffIcon.color = Color.black;
        shieldPowerBuffIcon.color = Color.black;
    }

    // The Turn Tracker Controller sends a message, this class listens and fires this method.

    private void IncreaseMoonPhasesTurnCounter()
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
                currentMoonPhase = MoonPhases.PhaseOne;
                OnMoonPhaseSwitch("Switching to Moon Phase One");
                Debug.Log("Case " + 2 + "Switching to Moon Phase One");
                break;
            case 4:
                currentMoonPhase = MoonPhases.PhaseTwo;
                OnMoonPhaseSwitch("Switching to Lunar Phase Two");
                Debug.Log("Case " + 4 + "Switching to Moon Phase Two");
                break;
            case 6:
                currentMoonPhase = MoonPhases.PhaseThree;
                OnMoonPhaseSwitch("Switching to Moon Phase Three");
                Debug.Log("Case " + 6 + "Switching to Moon Phase Three");
                break;
        }
    }
    private void ActivateLunarPhaseBuff()
    {
        switch (currentMoonPhase)
        {

            // OK to use DeityPrayerBuff but will eventually need to rename it in a way
            // conforming to its multi-purpose nature

            case MoonPhases.PhaseZero:
                Debug.Log(MoonPhases.PhaseZero);
                break;
            case MoonPhases.PhaseOne:
                bossController.ApplyBuff(DeityPrayerBuff.AffectedStat.AttackPower, phaseOnebuffAmount);
                attackPowerBuffIcon.color = Color.white;
                magicPowerBuffIcon.color = Color.black;
                shieldPowerBuffIcon.color = Color.black;
                OnMoonPhaseBuffActivation("Boss Received Attack Power Buff");
                Debug.Log(MoonPhases.PhaseOne);
                break;
            case MoonPhases.PhaseTwo:
                bossController.ApplyBuff(DeityPrayerBuff.AffectedStat.MagicPower, phaseTwobuffAmount);
                attackPowerBuffIcon.color = Color.black;
                magicPowerBuffIcon.color = Color.white;
                shieldPowerBuffIcon.color = Color.black;
                OnMoonPhaseBuffActivation("Boss Received Magic Power Buff");
                Debug.Log(MoonPhases.PhaseTwo);
                break;
            case MoonPhases.PhaseThree:
                bossController.ApplyBuff(DeityPrayerBuff.AffectedStat.ShieldPower, phaseThreebuffAmount);
                attackPowerBuffIcon.color = Color.black;
                magicPowerBuffIcon.color = Color.black;
                shieldPowerBuffIcon.color = Color.white;
                OnMoonPhaseBuffActivation("Boss Received Shield Power Buff");
                Debug.Log(MoonPhases.PhaseThree);
                break;
        }
    }
}
