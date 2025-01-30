using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AchievementBlockHelper : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI achievementNameText;
    [SerializeField] TextMeshProUGUI achievementDescriptionText;
    [SerializeField] TextMeshProUGUI spawnableDeityNameText;
    [SerializeField] TextMeshProUGUI requiredKillsNumberText;

    public void PopulateTexts(string achievementName, string achievementDescription, string spawnableDeityName, string achievementRequirement)
    {
        achievementNameText.text = achievementName;
        achievementDescriptionText.text = achievementDescription;
        spawnableDeityNameText.text = spawnableDeityName;
        requiredKillsNumberText.text = achievementRequirement;
    }
}
