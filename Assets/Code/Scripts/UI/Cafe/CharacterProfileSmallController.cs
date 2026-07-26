using TMPro;
using UnityEngine;

public class CharacterProfileSmallController : MonoBehaviour
{
    public Unit referenceUnit;
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _manaText;
    [SerializeField] private TextMeshProUGUI _faithPointsText;
    [SerializeField] private TextMeshProUGUI _attackPower;
    [SerializeField] private TextMeshProUGUI _defense;

    public void UpdateUIStats()
    {
        _hpText.text = $"HP {referenceUnit.unitHealthPoints.ToString()} / {referenceUnit.unitMaxHealthPoints.ToString()}";
        _manaText.text = $"MP {referenceUnit.unitManaPoints.ToString()} / {referenceUnit.unitMaxManaPoints.ToString()}";
        _faithPointsText.text = $"FAITH {referenceUnit.unitFaithPoints.ToString()} / {referenceUnit.gameObject.GetComponent<Unit>().unitTemplate.unitFaithPoints.ToString()}";

        // Update upgrade stats
        _attackPower.text = $"ATK {referenceUnit.unitAttackPower}";
        _defense.text = $"DEF {referenceUnit.unitShieldPoints}";
    }

    // Should use the same class to also display buff effects value and duration.
}
