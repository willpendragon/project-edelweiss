using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeityEntryController : MonoBehaviour
{
    [SerializeField] Image _deityPortrait;
    [SerializeField] TextMeshProUGUI _deityName;
    [SerializeField] TextMeshProUGUI _deityDescription;

    public void FillEntryDetails(Deity deity)
    {
        var deityUnit = deity.gameObject.GetComponent<Unit>();
        _deityPortrait.sprite = deityUnit.unitTemplate.unitPortrait;
        _deityName.text = deityUnit.unitTemplate.unitName;
        _deityDescription.text = "Deity Description (need to be added to template)";
    }
}
