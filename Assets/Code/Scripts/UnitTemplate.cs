using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Template", menuName = "UnitTemplate")]
public class UnitTemplate : ScriptableObject
{
    public string unitName;
    public SpriteRenderer unitSpriteRenderer;
    public List<Spell> spellsList;
}
