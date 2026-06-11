using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private GameObject _tutorialButtonsContainer;
    
    [Header("Dynamic Generation Prefabs")]
    [SerializeField] private Button _tutorialButtonPrefab;
    [SerializeField] private TutorialSlide _tutorialSlidePrefab;
    [SerializeField] private Transform _slideContainer;
    
    [Header("Content Data")]
    [SerializeField] private TutorialContent[] _availableTutorials;

    private TutorialSlide _currentActiveSlide;

    void Start()
    {
        CloseTutorialMenu();

        GenerateTutorialMenu();
    }

    public void CloseTutorialMenu()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Escape))
    //     {
    //         OpenTutorialWindow();
    //     }
    // }

    private void GenerateTutorialMenu()
    {
        // Instantiate Tutorial buttons in the scrolling menu
        foreach (var tutorial in _availableTutorials)
        {
            Button btn = Instantiate(_tutorialButtonPrefab, _tutorialButtonsContainer.transform);
            
            // Assuming your button prefab has a TextMeshProUGUI child to set the title
            var textComponent = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = tutorial.tutorialName;
            }

            // Add a Listener to the button
            // Listener opens up a Tutorial Slide
            btn.onClick.AddListener(() => OpenTutorialSlide(tutorial));
        }
    }

    public void OpenTutorialWindow()
    {
        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    public void OpenTutorialSlide(TutorialContent content)
    {
        _tutorialButtonsContainer.SetActive(false);
        
        if (_currentActiveSlide != null)
        {
            Destroy(_currentActiveSlide.gameObject);
        }

        // Dynamically instantiate the slide UI and inject the content
        _currentActiveSlide = Instantiate(_tutorialSlidePrefab, _slideContainer);
        _currentActiveSlide.Setup(content, CloseTutorialSlide);
    }

    public void CloseTutorialSlide()
    {
        if (_currentActiveSlide != null)
        {
            Destroy(_currentActiveSlide.gameObject);
        }
        _tutorialButtonsContainer.SetActive(true);
    }
}