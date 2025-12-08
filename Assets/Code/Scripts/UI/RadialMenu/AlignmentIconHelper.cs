using ProjectEdelweiss.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlignmentIconHelper : MonoBehaviour
{
    [SerializeField] private Image _alignmentIcon;

    [SerializeField] List<Sprite> _alignmentIcons = new List<Sprite>();

    public void DisplayAlignmentIcon()
    {
        // Note: The Icon should be retrieved from an Alignment SO (attached to the Spell).
        // Hard-coded logic for Playtest demo.
        // Get ActivePlayerUnit
        var activePlayerUnit = GameObject.FindGameObjectWithTag(GameTags.ActivePlayerUnit);
        SpellAlignment spellAlignment = activePlayerUnit.GetComponent<Unit>().unitTemplate.spellsList[0].alignment;
        
        Color imageColor = _alignmentIcon.color;
        imageColor.a = 1f;
        _alignmentIcon.color = imageColor;

        switch (spellAlignment)
        {
            case SpellAlignment.Ice:
                _alignmentIcon.sprite = _alignmentIcons[0];
                break;
            case SpellAlignment.Lightning:
                _alignmentIcon.sprite = _alignmentIcons[1];
                return;
        }
    }
}
