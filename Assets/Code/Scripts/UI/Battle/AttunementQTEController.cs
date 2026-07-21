using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;

public class AttunementQTEController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas qteCanvas;
    [SerializeField] private Slider qteSlider;
    [SerializeField] private RectTransform sliderHandle;
    [SerializeField] private Image perfectZone;
    [SerializeField] private Image normalZone;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI resultText;
    
    [Header("Configuration")]
    [SerializeField] private AttunementSettings settings;
    [SerializeField] private KeyCode inputKey = KeyCode.Space;
    
    // QTE state
    private bool _qteActive = false;
    private float _currentSliderValue = 0f;
    private float _sliderDirection = 1f; // 1 for right, -1 for left
    private float _inputTimestamp = 0f;
    private System.Action<float> _onQTEComplete;

    private void Awake()
    {
        if (qteCanvas != null)
            qteCanvas.enabled = false;
    }

    public void StartQTE(System.Action<float> onComplete)
    {
        if (settings == null)
        {
            Debug.LogError("AttunementQTEController: AttunementSettings not assigned!");
            onComplete?.Invoke(1f); // Auto-fail if no settings
            return;
        }

        _onQTEComplete = onComplete;
        StartCoroutine(RunQTE());
    }

    private IEnumerator RunQTE()
    {
        // Initialize QTE state
        _qteActive = true;
        _currentSliderValue = 0f;
        _sliderDirection = 1f;
        _inputTimestamp = Time.time;
        
        // Show UI
        if (qteCanvas != null) qteCanvas.enabled = true;
        if (instructionText != null) instructionText.text = $"Press {inputKey} to capture!";
        if (resultText != null) resultText.gameObject.SetActive(false);
        
        // Configure slider
        if (qteSlider != null)
        {
            qteSlider.minValue = 0f;
            qteSlider.maxValue = 1f;
            qteSlider.value = 0f;
        }

        // Visualize zones based on settings
        UpdateZoneVisuals();

        // Animate slider back and forth
        float elapsedTime = 0f;
        bool playerInputReceived = false;
        float finalDistanceFromCenter = 1f; // Default to miss

        while (elapsedTime < settings.inputTimeWindow && !playerInputReceived)
        {
            // Update slider position
            _currentSliderValue += _sliderDirection * settings.sliderSpeed * Time.deltaTime;
            
            // Bounce at edges
            if (_currentSliderValue >= 1f)
            {
                _currentSliderValue = 1f;
                _sliderDirection = -1f;
            }
            else if (_currentSliderValue <= 0f)
            {
                _currentSliderValue = 0f;
                _sliderDirection = 1f;
            }

            if (qteSlider != null)
                qteSlider.value = _currentSliderValue;

            // Check for player input
            if (Input.GetKeyDown(inputKey))
            {
                playerInputReceived = true;
                finalDistanceFromCenter = Mathf.Abs(_currentSliderValue - 0.5f) * 2f; // Normalize to 0-1
                break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // If no input received, treat as complete miss
        if (!playerInputReceived)
        {
            finalDistanceFromCenter = 1f;
        }

        // Show result feedback
        _qteActive = false;
        yield return ShowResult(finalDistanceFromCenter);

        // Hide UI
        if (qteCanvas != null) qteCanvas.enabled = false;

        // Callback with result
        _onQTEComplete?.Invoke(finalDistanceFromCenter);
    }

    private IEnumerator ShowResult(float distanceFromCenter)
    {
        string resultMessage = "";
        Color resultColor = Color.white;

        if (distanceFromCenter <= settings.perfectThreshold)
        {
            resultMessage = "PERFECT!";
            resultColor = Color.green;
        }
        else if (distanceFromCenter <= settings.normalThreshold)
        {
            resultMessage = "Good";
            resultColor = Color.yellow;
        }
        else
        {
            resultMessage = "Miss...";
            resultColor = Color.red;
        }

        if (resultText != null)
        {
            resultText.text = resultMessage;
            resultText.color = resultColor;
            resultText.gameObject.SetActive(true);
            
            // Animate result text
            resultText.transform.localScale = Vector3.zero;
            resultText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        yield return new WaitForSeconds(1f);
    }

    private void UpdateZoneVisuals()
    {
        if (settings == null) return;

        // Update perfect zone size (centered)
        if (perfectZone != null)
        {
            RectTransform perfectRT = perfectZone.GetComponent<RectTransform>();
            if (perfectRT != null)
            {
                float perfectWidth = settings.perfectThreshold * 2f; // Full width relative to slider
                perfectRT.anchorMin = new Vector2(0.5f - perfectWidth / 2f, 0f);
                perfectRT.anchorMax = new Vector2(0.5f + perfectWidth / 2f, 1f);
            }
        }

        // Update normal zone size (centered)
        if (normalZone != null)
        {
            RectTransform normalRT = normalZone.GetComponent<RectTransform>();
            if (normalRT != null)
            {
                float normalWidth = settings.normalThreshold * 2f;
                normalRT.anchorMin = new Vector2(0.5f - normalWidth / 2f, 0f);
                normalRT.anchorMax = new Vector2(0.5f + normalWidth / 2f, 1f);
            }
        }
    }

    /// <summary>
    /// Returns the singleton instance. Creates one if it doesn't exist.
    /// </summary>
    public static AttunementQTEController Instance
    {
        get
        {
            var existing = FindAnyObjectByType<AttunementQTEController>();
            if (existing != null) return existing;

            // Create a new instance
            var go = new GameObject("AttunementQTEController");
            return go.AddComponent<AttunementQTEController>();
        }
    }

    /// <summary>
    /// Sets the AttunementSettings for this controller.
    /// </summary>
    public void SetSettings(AttunementSettings newSettings)
    {
        settings = newSettings;
    }
}
