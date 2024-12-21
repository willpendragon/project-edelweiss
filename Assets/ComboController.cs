using Language.Lua;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboController : MonoBehaviour
{
    public static ComboController Instance { get; private set; }

    public int comboCounter;
    private string wantedEnemy;
    // Start is called before the first frame update

    private void Start()
    {
        wantedEnemy = "Stunner Godling";
    }
    public void IncreaseComboCounter(Unit defeatedEnemy)
    {
        if (CheckEnemy(defeatedEnemy))
        {
            comboCounter++;
        }
    }

    bool CheckEnemy(Unit defeatedEnemy)
    {
        if (defeatedEnemy.unitTemplate.unitName == wantedEnemy)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


}
