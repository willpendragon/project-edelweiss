using UnityEngine;

public class ComboController : MonoBehaviour
{
    public static ComboController Instance { get; private set; }

    public int comboCounter;
    private string wantedEnemy;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        wantedEnemy = "Stunner Godling";
    }
    public void IncreaseComboCounter(Unit defeatedEnemy)
    {
        Debug.Log("Check Defeated Enemy name");
        if (CheckEnemy(defeatedEnemy))
        {
            comboCounter++;
            defeatedEnemy.gameObject.GetComponentInChildren<BattleFeedbackController>().PlayComboIncreaseVFX();
        }
        else
        {
            if (comboCounter > 0)
            {
                comboCounter--;
            }
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
