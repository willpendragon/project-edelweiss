using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SinTracker : MonoBehaviour
{
    public float redEnmity;
    public float blueEnmity;
    [SerializeField] TextMeshProUGUI enmityScoreRed;
    [SerializeField] TextMeshProUGUI enmityScoreBlue;
    
public void IncreaseEnmity(alignmentType deityAlignment)
{
    switch(deityAlignment)
    {
        case alignmentType.red:
            redEnmity += 1;
            UpdateEnmityScore();
            break;
        case alignmentType.blue:
            blueEnmity += 1;
            UpdateEnmityScore();
            break;
    }
}
public void UpdateEnmityScore()
{
    enmityScoreRed.text = redEnmity.ToString();
    enmityScoreBlue.text = blueEnmity.ToString();
}
}

