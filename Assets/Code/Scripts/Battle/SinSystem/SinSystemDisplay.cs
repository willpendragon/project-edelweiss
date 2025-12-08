using TMPro;
using UnityEngine;

public class SinSystemDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI sinSystemText;

    public void DisplaySinfulMoves(string deityName, SpellAlignment spellAlignment)
    {
        string sinfulMoveName = spellAlignment.ToString();

        // Hard-coded logic, refactor using an SO for Spell Alignments.
        switch (spellAlignment)
        {
            case SpellAlignment.Lightning:
                sinSystemText.text = $"{deityName} hates {sinfulMoveName} <space=5><voffset=5><sprite=0></voffset>";
                break;

            case SpellAlignment.Ice:
                sinSystemText.text = $"{deityName} hates {sinfulMoveName} <space=5><voffset=5><sprite=1></voffset>";
                break;
        }
    }
}
