using TMPro;
using UnityEngine;

public class FieldPrizeController : MonoBehaviour
{
    public ItemFieldPrize fieldPrizeTemplate;
    [SerializeField] private float _powerUpAmount;
    [SerializeField] private ItemFieldPrizeType _itemFieldPrizeType;
    [SerializeField] TextMeshProUGUI _fieldPrizeLabel;
    [SerializeField] private GameObject _prizeVisuals;

    public float PowerUpAmount => _powerUpAmount;
    public ItemFieldPrizeType ItemFieldPrizeType => _itemFieldPrizeType;
    public void SetupPrize()
    {
        _powerUpAmount = fieldPrizeTemplate.powerUpAmount;
        _itemFieldPrizeType = fieldPrizeTemplate.itemFieldPrizeType;

        if (fieldPrizeTemplate.itemFieldPrizeType == ItemFieldPrizeType.attackPowerUp ||
            fieldPrizeTemplate.itemFieldPrizeType == ItemFieldPrizeType.magicPowerUp)
        {
            SetTextLabel();
            SetPrizeColor();
        }
        else
        {
            _fieldPrizeLabel.text = ""; // Band-aid solution, prevents keys from overriding labels.
            // Set specific rotation/scale overrides for Key Objects (note: will not work on hard-coded keys, like miniboss etc).
            if (_prizeVisuals != null)
            {
                _prizeVisuals.transform.localEulerAngles = new Vector3(-143, 0, 0);
                _prizeVisuals.transform.localScale = new Vector3(0.74f, 0.74f, 0.74f);
                _prizeVisuals.transform.localPosition = new Vector3(0, 0.2f, 0); // Note this will add up to the already
                // existing Y offset applied to the GameObject when spawning. 
            }
            SetKeyColor();

        }
    }

    // Text Label is contained in the SO label.
    public void SetTextLabel()
    {
        // Should be abbreviated name of the effect, not full name.
        _fieldPrizeLabel.text = fieldPrizeTemplate.itemFieldPrizeLabel;
    }

    // Prize Color has to be specified in the SO label.
    public void SetPrizeColor()
    {
        Material mat = GetComponentInChildren<MeshRenderer>().material; // Create material instance.

        switch (fieldPrizeTemplate.itemFieldPrizeType)
        {
            case ItemFieldPrizeType.attackPowerUp:
                mat.SetColor("_MainColor", new Color(1f, 0f, 0f, 1f));   // Red (with alpha)
                mat.renderQueue = 2000;
                break;

            case ItemFieldPrizeType.magicPowerUp:
                mat.SetColor("_MainColor", new Color(1f, 0f, 1f, 1)); // Magenta (with alpha)
                mat.renderQueue = 2000;
                break;

            default:
                break;
        }
    }

    public void SetKeyColor()
    {
        Material mat = GetComponentInChildren<MeshRenderer>().material; // Create material instance.
        mat.SetColor("_MainColor", Color.yellow);
    }
}
