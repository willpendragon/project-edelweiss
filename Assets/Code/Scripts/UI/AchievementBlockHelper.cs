using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AchievementBlockHelper : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI achievementNameText;
    [SerializeField] TextMeshProUGUI achievementDescriptionText;
    [SerializeField] TextMeshProUGUI spawnableDeityNameText;
    [SerializeField] TextMeshProUGUI requirementNumberText;
    [SerializeField] TextMeshProUGUI achievementProgressText;
    [SerializeField] Slider _slider;
    [SerializeField] const string COMPLETE = "Complete";

    public void PopulateTexts(string achievementName, string achievementDescription, string spawnableDeityName, string achievementRequirement, string achievementProgress)
    {
        achievementNameText.text = achievementName;
        achievementDescriptionText.text = achievementDescription;
        spawnableDeityNameText.text = spawnableDeityName;

        achievementProgressText.text = $"{achievementProgress} / {achievementRequirement}";
        UpdateSlider(achievementRequirement, achievementProgress);
        SetCompletionText(achievementRequirement, achievementProgress);
    }

    private void SetCompletionText(string achievementRequirement, string achievementProgress)
    {
        int requirementInt = int.Parse(achievementRequirement);
        int progressInt = int.Parse(achievementProgress);
        if (requirementInt <= progressInt)
        {
            achievementProgressText.text = COMPLETE;
        }
    }

    public void UpdateSlider(string achievementRequirement, string achievementProgress)
    {
        int requirementInt = int.Parse(achievementRequirement);
        int progressInt = int.Parse(achievementProgress);
        _slider.minValue = 0;
        _slider.maxValue = requirementInt;
        _slider.value = progressInt;
    }
}
