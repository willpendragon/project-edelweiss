using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SinSystemDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI sinSystemText;
    [SerializeField] Image _alignmentIcon;
    [SerializeField] List<Sprite> _alignmentIcons = new List<Sprite>();
    [SerializeField] private CanvasGroup _sinfulElementIconHolder;

    public void DisplaySinfulMoves(string deityName, SpellAlignment spellAlignment)
    {
        string sinfulMoveName = spellAlignment.ToString();

        _sinfulElementIconHolder.alpha = 1;

        // Hard-coded logic, refactor using an SO for Spell Alignments.
        switch (spellAlignment)
        {
            case SpellAlignment.Lightning:
                sinSystemText.text = "Sinful Element";
                _alignmentIcon.sprite = _alignmentIcons[0];
                //sinSystemText.text = $"This Deity hates {sinfulMoveName}  <space=5><voffset=5><sprite=0></voffset>";
                break;

            case SpellAlignment.Ice:
                _alignmentIcon.sprite = _alignmentIcons[1];
                sinSystemText.text = "Sinful Element";
                //sinSystemText.text = $"This Deity hates {sinfulMoveName}  <space=5><voffset=5><sprite=1></voffset>";
                break;
        }
    }
}
