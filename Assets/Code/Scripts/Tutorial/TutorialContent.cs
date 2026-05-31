using UnityEngine;

[CreateAssetMenu(fileName = "NewTutorialContent", menuName = "Tutorial/Tutorial Content")]
public class TutorialContent : ScriptableObject
{
    [System.Serializable]
    public struct Page
    {
        [TextArea(3, 5)]
        public string text;
        public Sprite image;
    }

    public string tutorialName;
    public Page[] pages;
}