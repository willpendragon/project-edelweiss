using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalHandler : MonoBehaviour
{
    public int crystalsAmount = 3;
    public void TurnUnitIntoCrystal()
    {
        BattleManager.Instance.captureCrystalsRewardPool += crystalsAmount;
    }
}
