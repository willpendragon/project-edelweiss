using System;
using UnityEngine;

[System.Serializable]
public class CutsceneSlide
{
    [Tooltip("The background image for this part of the cutscene.")]
    public Sprite image;
    
    [TextArea(3, 5)]
    [Tooltip("The dialogue or description text appearing below.")]
    public string text;
    
    [Tooltip("How long the slide stays fully visible before fading out.")]
    public float duration = 3f;
    
    [Tooltip("How long the fade-in and fade-out transitions take.")]
    public float fadeDuration = 1f;
}

[CreateAssetMenu(fileName = "NewCutscene", menuName = "RPG/Cutscene Data")]
public class CutsceneData : ScriptableObject
{
    public CutsceneSlide[] slides;
}