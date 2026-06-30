using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Manages Deity UI display in battle contexts with screen space overlay.
/// Detects battle type and displays appropriate UI:
/// - No UI: Regular battles without Deity
/// - Minimal UI (Overlay): Deity present in regular battle (enmity only)
/// - Full UI (Overlay): BattleWithDeity (HP + enmity)
/// </summary>
public class DeityBattleUIController : MonoBehaviour
{
    [SerializeField] private BattleManager _battleManager;
    [SerializeField] private BattleTypeController _battleTypeController;

    [Header("Overlay Canvas Setup")]
    [SerializeField] private Canvas _overlayCanvas;

    [Header("Full UI (BattleWithDeity)")]
    [SerializeField] private CanvasGroup _fullUIContainer;
    [SerializeField] private Image _deityPortraitImage;
    [SerializeField] private TextMeshProUGUI _deityNameText;
    [SerializeField] private Slider _deityHealthSlider;
    [SerializeField] private TextMeshProUGUI _deityHealthText;
    [SerializeField] private Slider _deityEnmitySlider;
    [SerializeField] private TextMeshProUGUI _deityEnmityText;

    [Header("Minimal UI (Deity in Regular Battle)")]
    [SerializeField] private CanvasGroup _minimalUIContainer;
    [SerializeField] private Slider _minimalEnmitySlider;
    [SerializeField] private TextMeshProUGUI _minimalEnmityText;

    [Header("Settings")]
    [SerializeField] private float _uiFadeDuration = 0.3f;
    [SerializeField] private float _updateInterval = 0.1f;
    [SerializeField] private float _delayedInitializationDelay = 0.5f;

    public Deity CurrentDeity { get; private set; }
    private Unit _deityUnitComponent;
    private BattleTypeController.BattleType _currentBattleType;
    private bool _isInitialized = false;
    private Coroutine _updateCoroutine;

    private void OnEnable()
    {
        Deity.OnDeityNotificationUpdate += HandleDeityNotification;
        BattleTypeController.OnBattleTypeInitialized += OnBattleTypeInitialized;
    }

    private void OnDisable()
    {
        Deity.OnDeityNotificationUpdate -= HandleDeityNotification;
        BattleTypeController.OnBattleTypeInitialized -= OnBattleTypeInitialized;

        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
        }
    }

    private void Start()
    {
        // Get references if not set in inspector
        if (_battleManager == null)
            _battleManager = FindAnyObjectByType<BattleManager>();

        if (_battleTypeController == null)
            _battleTypeController = BattleTypeController.Instance;
    }

    /// <summary>
    /// Called when BattleTypeController has determined the battle type.
    /// Uses delayed initialization to ensure Deity has been spawned.
    /// </summary>
    private void OnBattleTypeInitialized()
    {
        // Delay initialization to allow DeitySpawner to spawn the Deity
        StartCoroutine(DelayedInitialize());
    }

    /// <summary>
    /// Waits for Deity to be spawned, then initializes the UI.
    /// </summary>
    private IEnumerator DelayedInitialize()
    {
        yield return new WaitForSeconds(_delayedInitializationDelay);
        InitializeDeityUI();
    }

    /// <summary>
    /// Initializes the Deity UI based on the current battle type.
    /// </summary>
    private void InitializeDeityUI()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;
        _currentBattleType = _battleTypeController.currentBattleType;

        // Check if Deity is present in the battle
        if (_battleManager == null || _battleManager.deity == null)
        {
            Debug.Log("DeityBattleUIController: No Deity present in battle.");
            HideAllDeityUI();
            return;
        }

        CurrentDeity = _battleManager.deity;
        _deityUnitComponent = CurrentDeity.GetComponentInChildren<Unit>();

        if (_deityUnitComponent == null)
        {
            Debug.LogWarning("DeityBattleUIController: Deity found but missing Unit component.");
            HideAllDeityUI();
            return;
        }

        Debug.Log($"DeityBattleUIController: Initializing Deity UI for {CurrentDeity.name} in {_currentBattleType} battle.");

        // Set up UI based on battle type
        switch (_currentBattleType)
        {
            case BattleTypeController.BattleType.BattleWithDeity:
                SetupFullUI();
                break;

            case BattleTypeController.BattleType.RegularBattle:
                // Deity is present in a regular battle
                SetupMinimalUI();
                break;

            default:
                HideAllDeityUI();
                break;
        }
    }

    /// <summary>
    /// Sets up full UI with HP, enmity, portrait, and name.
    /// Used in BattleWithDeity encounters.
    /// </summary>
    private void SetupFullUI()
    {
        if (_fullUIContainer == null)
        {
            Debug.LogWarning("DeityBattleUIController: Full UI container not assigned.", gameObject);
            return;
        }

        // Hide minimal UI
        FadeOutUI(_minimalUIContainer);

        // Show full UI
        FadeInUI(_fullUIContainer);

        // Initialize UI values
        UpdateFullUIValues();

        // Start update coroutine
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
        }
        _updateCoroutine = StartCoroutine(ContinuousUpdateFullUI());
    }

    /// <summary>
    /// Sets up minimal UI with only enmity meter.
    /// Used when a Deity is present in a regular battle.
    /// </summary>
    private void SetupMinimalUI()
    {
        if (_minimalUIContainer == null)
        {
            Debug.LogWarning("DeityBattleUIController: Minimal UI container not assigned.", gameObject);
            return;
        }

        // Hide full UI
        FadeOutUI(_fullUIContainer);

        // Show minimal UI
        FadeInUI(_minimalUIContainer);

        // Initialize enmity slider
        if (_minimalEnmitySlider != null)
        {
            _minimalEnmitySlider.maxValue = CurrentDeity._maxEnmity;
            _minimalEnmitySlider.value = CurrentDeity.enmity;
        }

        // Start update coroutine
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
        }
        _updateCoroutine = StartCoroutine(ContinuousUpdateMinimalUI());
    }

    /// <summary>
    /// Updates all full UI elements with current values.
    /// </summary>
    private void UpdateFullUIValues()
    {
        if (_deityUnitComponent == null || CurrentDeity == null)
            return;

        // Update portrait
        if (_deityPortraitImage != null && CurrentDeity.deityPortrait != null)
        {
            _deityPortraitImage.sprite = CurrentDeity.deityPortrait;
        }

        // Update name
        if (_deityNameText != null)
        {
            _deityNameText.text = _deityUnitComponent.unitTemplate.unitName;
        }

        // Update health
        if (_deityHealthSlider != null)
        {
            _deityHealthSlider.maxValue = _deityUnitComponent.unitMaxHealthPoints;
            _deityHealthSlider.value = _deityUnitComponent.unitHealthPoints;
        }

        if (_deityHealthText != null)
        {
            _deityHealthText.text = $"{_deityUnitComponent.unitHealthPoints:F0} / {_deityUnitComponent.unitMaxHealthPoints:F0}";
        }

        // Update enmity
        if (_deityEnmitySlider != null)
        {
            _deityEnmitySlider.maxValue = CurrentDeity._maxEnmity;
            _deityEnmitySlider.value = CurrentDeity.enmity;
        }

        if (_deityEnmityText != null)
        {
            _deityEnmityText.text = $"{CurrentDeity.enmity:F0} / {CurrentDeity._maxEnmity:F0}";
        }
    }

    /// <summary>
    /// Updates minimal UI enmity values.
    /// </summary>
    private void UpdateMinimalUIValues()
    {
        if (CurrentDeity == null)
            return;

        if (_minimalEnmitySlider != null)
        {
            _minimalEnmitySlider.value = CurrentDeity.enmity;
            _minimalEnmitySlider.maxValue = CurrentDeity._maxEnmity;
            _minimalEnmityText.text = $"{CurrentDeity.enmity:F0} / {CurrentDeity._maxEnmity:F0}";
            _deityHealthSlider.gameObject.SetActive(false); // Hide health slider in minimal UI
            _deityHealthText.gameObject.SetActive(false); // Hide health text in minimal UI
        }

        if (_minimalEnmityText != null)
        {
            _minimalEnmityText.text = $"{CurrentDeity.enmity:F0} / {CurrentDeity._maxEnmity:F0}";
        }

        _deityNameText.text = CurrentDeity.gameObject.GetComponent<Unit>().unitTemplate.unitName;
    }

    /// <summary>
    /// Continuously updates full UI values during BattleWithDeity.
    /// </summary>
    private IEnumerator ContinuousUpdateFullUI()
    {
        while (_currentBattleType == BattleTypeController.BattleType.BattleWithDeity &&
               _deityUnitComponent != null)
        {
            UpdateFullUIValues();
            yield return new WaitForSeconds(_updateInterval);
        }
    }

    /// <summary>
    /// Continuously updates minimal UI values when Deity is in regular battle.
    /// </summary>
    private IEnumerator ContinuousUpdateMinimalUI()
    {
        while (_currentBattleType == BattleTypeController.BattleType.RegularBattle &&
               CurrentDeity != null)
        {
            UpdateMinimalUIValues();
            yield return new WaitForSeconds(_updateInterval);
        }
    }

    /// <summary>
    /// Handles notifications from the Deity and updates UI feedback if needed.
    /// </summary>
    private void HandleDeityNotification(string notificationText)
    {
        // Can be extended for visual feedback like brief highlights
        // Example: Show damage numbers or status effects
    }

    /// <summary>
    /// Fades in a UI container with smooth transition.
    /// </summary>
    private void FadeInUI(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        StartCoroutine(FadeCoroutine(canvasGroup, 0, 1));
    }

    /// <summary>
    /// Fades out a UI container with smooth transition.
    /// </summary>
    private void FadeOutUI(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        StartCoroutine(FadeCoroutine(canvasGroup, 1, 0));
    }

    /// <summary>
    /// Coroutine for smoothly fading UI elements.
    /// </summary>
    private IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        while (elapsedTime < _uiFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / _uiFadeDuration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    /// <summary>
    /// Hides all Deity UI elements and cleans up coroutines.
    /// </summary>
    private void HideAllDeityUI()
    {
        if (_fullUIContainer != null)
        {
            _fullUIContainer.alpha = 0;
            _fullUIContainer.blocksRaycasts = false;
        }

        if (_minimalUIContainer != null)
        {
            _minimalUIContainer.alpha = 0;
            _minimalUIContainer.blocksRaycasts = false;
        }

        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
        }
    }

    /// <summary>
    /// Cleans up when battle ends. Call this from BattleFlowController or similar.
    /// </summary>
    public void OnBattleEnd()
    {
        HideAllDeityUI();
        CurrentDeity = null;
        _deityUnitComponent = null;
        _isInitialized = false;
    }
}