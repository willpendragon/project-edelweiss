using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovesTracker : MonoBehaviour
{
    public void OnEnable()
    {
        AOESpellPlayerAction.OnUsedSingleTargetSpell += TrackUsedSingleTargetSpell;
    }

    public void OnDisable()
    {
        AOESpellPlayerAction.OnUsedSingleTargetSpell -= TrackUsedSingleTargetSpell;
    }

    public void TrackUsedSingleTargetSpell()
    {
        GameStatsManager gameStatsManager = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStatsManager>();
        gameStatsManager.timesSingleTargetSpellWasUsed++;
        Debug.Log("Adding times Single Target Spell was Used");
    }
}
