using TMPro;
using UnityEngine;

public class CrystalHandler : MonoBehaviour
{
    // The base amount of Tributes going into the rewards pool.
    public int crystalsAmount = 3;
    public void TurnUnitIntoCrystal()
    {
        BattleManager.Instance.captureCrystalsRewardPool += crystalsAmount;
        // Play Tributes going to Rewards Pool animation here.
        var tributesCounter = FindAnyObjectByType<TributesCounter>();
        tributesCounter.UpdateTributesCounter();
    }
}
