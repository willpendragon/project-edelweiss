using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AchievementBlockHelper : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI achievementNameText;
    [SerializeField] TextMeshProUGUI achievementDescriptionText;
    [SerializeField] TextMeshProUGUI spawnableDeityNameText;
    [SerializeField] TextMeshProUGUI requirementNumberText;
    [SerializeField] TextMeshProUGUI achievementProgressText;

    public void PopulateTexts(string achievementName, string achievementDescription, string spawnableDeityName, string achievementRequirement, string achievementProgress)
    {
        achievementNameText.text = achievementName;
        achievementDescriptionText.text = achievementDescription;
        spawnableDeityNameText.text = spawnableDeityName;
        requirementNumberText.text = achievementRequirement;
        achievementProgressText.text = achievementProgress;
    }
}
