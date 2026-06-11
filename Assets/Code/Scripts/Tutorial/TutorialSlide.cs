using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class TutorialSlide : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _contentText;
    [SerializeField] private Image _contentImage;
    [SerializeField] private TextMeshProUGUI _pageNumberText;
    
    [Header("Controls")]
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _prevButton;
    [SerializeField] private Button _closeButton;

    private TutorialContent _currentContent;
    private int _currentPageIndex;

    public void Setup(TutorialContent content, System.Action onClose)
    {
        _currentContent = content;
        _currentPageIndex = 0;
        
        if (_titleText != null) 
            _titleText.text = content.tutorialName;
        
        // Setup button listeners
        _nextButton.onClick.RemoveAllListeners();
        _nextButton.onClick.AddListener(NextPage);
        
        _prevButton.onClick.RemoveAllListeners();
        _prevButton.onClick.AddListener(PrevPage);
        
        _closeButton.onClick.RemoveAllListeners();
        _closeButton.onClick.AddListener(() => onClose?.Invoke());
        
        UpdatePageDisplay();
    }

    private void NextPage()
    {
        if (_currentPageIndex < _currentContent.pages.Length - 1)
        {
            _currentPageIndex++;
            UpdatePageDisplay();
        }
    }

    private void PrevPage()
    {
        if (_currentPageIndex > 0)
        {
            _currentPageIndex--;
            UpdatePageDisplay();
        }
    }

    private void UpdatePageDisplay()
    {
        if (_currentContent.pages == null || _currentContent.pages.Length == 0) return;

        var page = _currentContent.pages[_currentPageIndex];
        
        if (_contentText != null) 
            _contentText.text = page.text;
            
        if (_contentImage != null)
        {
            _contentImage.sprite = page.image;
            _contentImage.gameObject.SetActive(page.image != null);
        }
        
        if (_pageNumberText != null)
            _pageNumberText.text = $"{_currentPageIndex + 1} / {_currentContent.pages.Length}";
        
        // Disable next/prev buttons if we are at the edges
        if (_prevButton != null) _prevButton.interactable = _currentPageIndex > 0;
        if (_nextButton != null) _nextButton.interactable = _currentPageIndex < _currentContent.pages.Length - 1;
    }
}