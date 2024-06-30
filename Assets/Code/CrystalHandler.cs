using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalHandler : MonoBehaviour
{
    public void TurnUnitIntoCrystal()
    {
        BattleManager.Instance.captureCrystalsRewardPool++;
    }
}
