using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    public Unit bossUnit;
    [SerializeField] GameObject bossHealthBarGO;
    public GameObject bossHealthBarInstance;
    public GameObject bossModel;

    private void Start()
    {
        AssignBossUnit();
        CreateBossHealthBar();
        PopulateDeityHealthBar();
        RotateBossUnitModel();
    }

    private void RotateBossUnitModel()
    {
        bossModel = GameObject.FindGameObjectWithTag("BossSimildeModel");
        bossModel.transform.rotation = Quaternion.Euler(0, 0, 0);
        Quaternion bossUnitRotation = Quaternion.Euler(0, -65, 0);
        bossModel.transform.rotation = bossUnitRotation;
    }

    public void AssignBossUnit()
    {
        GameObject[] enemyUnitsOnBattlefield = BattleManager.Instance.gameObject.GetComponent<TurnController>().enemyUnitsOnBattlefield;
        foreach (var enemy in enemyUnitsOnBattlefield)
        {
            // Look up for the Unit flagged as boss in the Enemy Party and assign it
            // to the bossUnit variable.

            if (enemy.GetComponent<Unit>()?.bossFlag == true)
            {
                bossUnit = enemy.GetComponent<Unit>();
            }
        }
    }
    public void ApplyBuff(DeityPrayerBuff.AffectedStat buffingStat, float amount)
    {
        if (bossUnit != null)
        {
            switch (buffingStat)
            {
                case DeityPrayerBuff.AffectedStat.AttackPower:
                    bossUnit.unitAttackPower += amount;
                    break;
                case DeityPrayerBuff.AffectedStat.MagicPower:
                    bossUnit.unitMagicPower += amount;
                    break;
                case DeityPrayerBuff.AffectedStat.ShieldPower:
                    bossUnit.unitShieldPoints += amount;
                    break;
            }
        }
    }
    void CreateBossHealthBar()
    {
        GameObject battleInterfaceCanvasGO = GameObject.FindGameObjectWithTag("BattleInterfaceCanvas");
        bossHealthBarInstance = Instantiate(bossHealthBarGO, battleInterfaceCanvasGO.transform);
        Debug.Log("Spawned Boss Health Bar");
    }

    void PopulateDeityHealthBar()
    {
        Slider bossHPSlider = bossHealthBarInstance.GetComponentInChildren<Slider>();

        bossHPSlider.maxValue = bossUnit.unitTemplate.unitMaxHealthPoints;
        bossHPSlider.value = bossUnit.GetComponent<Unit>().unitTemplate.unitHealthPoints;
        bossHPSlider.GetComponentInChildren<TextMeshProUGUI>().text = bossUnit.unitTemplate.unitMaxHealthPoints.ToString();
    }
    public void UpdateBossHealthBar(float bossHealthPoints)
    {
        if (bossHealthBarInstance != null)
        {
            bossHealthBarInstance.GetComponentInChildren<Slider>().value = bossHealthPoints;
        }
    }
}
