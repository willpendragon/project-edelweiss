using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Template", menuName = "UnitTemplate")]
public class UnitTemplate : ScriptableObject
{
    public string unitName;
    public Sprite unitPortrait;
    public List<Spell> spellsList;
    public int unitHealthPoints;
    public int unitMaxHealthPoints;
    public int unitManaPoints;
    public int unitOpportunityPoints;
    public int unitShieldPoints;
    public float unitCoinsReward;
    public float unitExperiencePointsReward;

}
