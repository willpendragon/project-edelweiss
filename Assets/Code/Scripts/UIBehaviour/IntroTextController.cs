using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;


public class IntroTextController : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public float typingSpeed = 0.05f;

    public string[] sentences = new string[]
    {
        "There was a time, when warriors could feel the mighty fist of God crushing the body and soul of the foolish humans getting in the way of His allies.",
        "Those days, are now long gone. The Silence of God turns men into beats on the battlfield, and torments the dreams of timid scholars, trying to find answers in their dusty attics.",
        "But there, atop the highest mountains, dividing two war-torn Kingdom, a mystic fortress awaits whose thirsty for divine knowledge and power.",
        "I can only dare to gather the many accounts and rumours, and tell the story of the fabled three Knight Saints, who tried to forge a new pact between Heaven and Earth."
    };

    private int index = 0;

    void Start()
    {
        TypeNextSentence();
    }

    void TypeNextSentence()
    {
        if (index < sentences.Length)
        {
            textComponent.text = "";
            textComponent.DOText(sentences[index], sentences[index].Length * typingSpeed)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    index++;
                    Invoke(nameof(TypeNextSentence), 1f); // Wait 1 second before typing the next sentence
                });
        }
        else
        {
            // Implement transition to the next scene or next action here
            Debug.Log("End of intro");
        }
    }
}