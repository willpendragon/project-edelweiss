using TMPro;
using UnityEngine;

public class FieldPrizeController : MonoBehaviour
{
    public ItemFieldPrize fieldPrizeTemplate;
    [SerializeField] private float _powerUpAmount;
    [SerializeField] private ItemFieldPrizeType _itemFieldPrizeType;
    [SerializeField] TextMeshProUGUI _fieldPrizeLabel;
    [SerializeField] MeshRenderer _prizeMesh; // Mesh should be retrieved from SO.

    public float PowerUpAmount => _powerUpAmount;
    public ItemFieldPrizeType ItemFieldPrizeType => _itemFieldPrizeType;
    public void SetupPrize()
    {
        _powerUpAmount = fieldPrizeTemplate.powerUpAmount;
        _itemFieldPrizeType = fieldPrizeTemplate.itemFieldPrizeType;
        if (fieldPrizeTemplate.itemFieldPrizeType != ItemFieldPrizeType.PuzzleLevelKey)
        {
            SetTextLabel();
            SetPrizeColor();
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
        if (_prizeMesh == null)
        {
            Debug.LogWarning("No Prize MeshRenderer has been found.");
            return;
        }

        Material mat = _prizeMesh.material; // Create material instance.

        switch (fieldPrizeTemplate.itemFieldPrizeType)
        {
            case ItemFieldPrizeType.attackPowerUp:
                mat.SetColor("_BaseColor", new Color(1f, 0f, 0f, 1f));   // Red (with alpha)
                break;

            case ItemFieldPrizeType.magicPowerUp:
                mat.SetColor("_BaseColor", new Color(1f, 0f, 1f, 1)); // Magenta (with alpha)
                break;

            default:
                break;
        }
    }
}
