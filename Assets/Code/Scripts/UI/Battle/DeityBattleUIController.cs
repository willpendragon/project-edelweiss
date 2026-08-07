using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DeityBattleUIController : MonoBehaviour
{
    [SerializeField] private BattleManager _battleManager;
    [SerializeField] private BattleTypeController _battleTypeController;
    [SerializeField] private DeitySpawner _deitySpawner;

    [Header("Overlay Canvas Setup")]
    [SerializeField] private Canvas _overlayCanvas;

    [Header("Deity UI Common Elements")]
    [SerializeField] private Image _deityPortraitImage;
    [SerializeField] private TextMeshProUGUI _deityNameText;
    [SerializeField] private CanvasGroup _uIContainer;
    [SerializeField] private Slider _deityEnmitySlider;
    [SerializeField] private TextMeshProUGUI _deityEnmityText;
    [SerializeField] private Slider _deityHealthSlider;
    [SerializeField] private TextMeshProUGUI _deityHealthText;

    [Header("Deity UI Unbound Battle Elements")]

    [Header("Settings")]
    [SerializeField] private float _uiFadeDuration = 0.3f;
    [SerializeField] private float _updateInterval = 0.1f;
    [SerializeField] private float _delayedInitializationDelay = 0.5f;

    [SerializeField] public Deity CurrentDeity { get; private set; }
    private Unit _deityUnitComponent;
    private BattleTypeController.BattleType _currentBattleType;
    private bool _isInitialized = false;
    private Coroutine _updateCoroutine;
    [SerializeField] private bool _unboundDeityPresent;

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
        if (_battleManager == null)
            _battleManager = FindAnyObjectByType<BattleManager>();

        if (_battleTypeController == null)
            _battleTypeController = BattleTypeController.Instance;

        if (_deitySpawner == null)
        {
            _deitySpawner = GameObject.FindAnyObjectByType<DeitySpawner>();
        }
    }

    private void OnBattleTypeInitialized()
    {
        // Delay initialization to allow DeitySpawner to spawn the Deity.
        StartCoroutine(DelayedInitialize());
    }
    private IEnumerator DelayedInitialize()
    {
        yield return new WaitForSeconds(_delayedInitializationDelay);
        InitializeDeityUI();
    }
    private void InitializeDeityUI()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;
        _currentBattleType = _battleTypeController.currentBattleType;

        // Check if the Deity is present as an Unbound Deity (this is the case when the Battle is against the Deity itself, so it doesn't populate the BattleManager).

        if (_deitySpawner != null && _deitySpawner.currentUnboundDeity != null)
        {
            _unboundDeityPresent = true;
        }

        // Check if Deity is present in the battle. If not, hide dedicated UI.
        if (_battleManager == null || _battleManager.deity == null && !_unboundDeityPresent)
        {
            Debug.Log("DeityBattleUIController: No Deity present in battle.");
            HideAllDeityUI();
            return;
        }

        // Set up UI and retrives Deity based on battle type. At the moment, Deity spawn in two different ways (hence the need for the switch).
        switch (_currentBattleType)
        {
            case BattleTypeController.BattleType.BattleWithDeity:
                CurrentDeity = _deitySpawner.currentUnboundDeity;
                _deityUnitComponent = CurrentDeity.GetComponentInChildren<Unit>();

                if (_deityUnitComponent == null)
                {
                    Debug.LogWarning("DeityBattleUIController: Deity found but missing Unit component.");
                    HideAllDeityUI();
                    return;
                }

                SetupUI();
                Debug.Log($"DeityBattleUIController: Initialized Deity UI for {CurrentDeity.name} in {_currentBattleType} battle.");
                break;

            case BattleTypeController.BattleType.RegularBattle:
                CurrentDeity = _battleManager.deity;
                _deityUnitComponent = CurrentDeity.GetComponentInChildren<Unit>();

                if (_deityUnitComponent == null)
                {
                    Debug.LogWarning("DeityBattleUIController: Deity found but missing Unit component.");
                    HideAllDeityUI();
                    return;
                }

                // Deity is present in a regular battle
                SetupUI();
                Debug.Log($"DeityBattleUIController: Initialized Deity UI for {CurrentDeity.name} in {_currentBattleType} battle.");
                break;

            case BattleTypeController.BattleType.PuzzleBattle:
                // Quick fiX: Puzzle battles shouldn't contain deities, but in current config a boss battle is flagged as a puzzle battle.
                // This case allows deity parameters to be displayed in boss fights that are incorrectly classified as puzzle battles.
                CurrentDeity = _battleManager.deity;
                _deityUnitComponent = CurrentDeity.GetComponentInChildren<Unit>();

                if (_deityUnitComponent == null)
                {
                    Debug.LogWarning("DeityBattleUIController: Deity found but missing Unit component.");
                    HideAllDeityUI();
                    return;
                }

                // Deity is present in a puzzle battle (treated as boss battle)
                SetupUI();
                Debug.Log($"DeityBattleUIController: Initialized Deity UI for {CurrentDeity.name} in {_currentBattleType} battle.");
                break;

            default:
                HideAllDeityUI();
                break;
        }
    }


    private void SetupUI()
    {
        if (_uIContainer == null)
        {
            Debug.LogWarning("DeityBattleUIController: UI container not assigned.", gameObject);
            return;
        }

        // Show UI
        FadeInUI(_uIContainer);

        // Enmity should always be visible when deity UI is active.
        SetEnmityUIVisible(true);

        // Health should only be visible in direct deity battles.
        SetHealthUIVisible(ShouldShowHealthUI());

        // Initialize enmity slider
        if (_deityEnmitySlider != null)
        {
            _deityEnmitySlider.maxValue = CurrentDeity._maxEnmity;
            _deityEnmitySlider.value = CurrentDeity.enmity;
        }

        if (ShouldShowHealthUI())
        {
            // Initialize Deity HP slider.
            if (_deityHealthSlider != null)
            {
                _deityHealthSlider.maxValue = _deityUnitComponent.unitMaxHealthPoints;
                _deityHealthSlider.value = _deityUnitComponent.unitHealthPoints;
            }

            if (_deityHealthText != null)
            {
                _deityHealthText.text = $"{_deityUnitComponent.unitHealthPoints:F0} / {_deityUnitComponent.unitMaxHealthPoints:F0}";
            }
        }

        // Update all UI values immediately (portrait, enmity text, health text)
        UpdateUIValues();

        // Start update coroutine
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
        }
        _updateCoroutine = StartCoroutine(ContinuousUpdateUI());
    }

    public void UpdateUIValues()
    {
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

        // Update enmity.
        if (_deityEnmitySlider != null)
        {
            _deityEnmitySlider.maxValue = CurrentDeity._maxEnmity;
            _deityEnmitySlider.value = CurrentDeity.enmity;
        }

        if (_deityEnmityText != null)
        {
            //_deityEnmityText.text = $"{CurrentDeity.enmity:F0} / {CurrentDeity._maxEnmity:F0}";
            // Show decimal values in UI.
            _deityEnmityText.text = $"{CurrentDeity.enmity:F1} / {CurrentDeity._maxEnmity:F0}";
        }

        // Update health (this logic is applicable only during fights against the Deity, or where the Player can attack them directly!).
        if (ShouldShowHealthUI() && _deityHealthSlider != null)
        {
            _deityHealthSlider.maxValue = _deityUnitComponent.unitMaxHealthPoints;
            _deityHealthSlider.value = _deityUnitComponent.unitHealthPoints;
        }

        if (ShouldShowHealthUI() && _deityHealthText != null)
        {
            _deityHealthText.text = $"{_deityUnitComponent.unitHealthPoints:F0} / {_deityUnitComponent.unitMaxHealthPoints:F0}";
        }

    }

    private bool ShouldShowHealthUI()
    {
        // Quick fix: Show health in PuzzleBattle because boss battles are currently flagged as puzzle battles in the config.
        return _currentBattleType == BattleTypeController.BattleType.BattleWithDeity || 
               _currentBattleType == BattleTypeController.BattleType.PuzzleBattle;
    }

    private void SetHealthUIVisible(bool isVisible)
    {
        if (_deityHealthSlider != null)
        {
            _deityHealthSlider.gameObject.SetActive(isVisible);
        }

        if (_deityHealthText != null)
        {
            _deityHealthText.gameObject.SetActive(isVisible);
        }
    }

    private void SetEnmityUIVisible(bool isVisible)
    {
        if (_deityEnmitySlider != null)
        {
            _deityEnmitySlider.gameObject.SetActive(isVisible);
        }

        if (_deityEnmityText != null)
        {
            _deityEnmityText.gameObject.SetActive(isVisible);
        }
    }

    private IEnumerator ContinuousUpdateUI()
    {
        while ((_currentBattleType == BattleTypeController.BattleType.RegularBattle ||
                _currentBattleType == BattleTypeController.BattleType.BattleWithDeity) &&
               CurrentDeity != null)
        {
            UpdateUIValues();
            yield return new WaitForSeconds(_updateInterval);
        }
    }
    private void HandleDeityNotification(string notificationText)
    {
        // Hook into this to display notifications.
    }

    private void FadeInUI(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        StartCoroutine(FadeCoroutine(canvasGroup, 0, 1));
    }

    private void FadeOutUI(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        StartCoroutine(FadeCoroutine(canvasGroup, 1, 0));
    }

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

    private void HideAllDeityUI()
    {
        if (_uIContainer != null)
        {
            _uIContainer.alpha = 0;
            _uIContainer.blocksRaycasts = false;
            _uIContainer.interactable = false;
        }

        SetHealthUIVisible(false);
        SetEnmityUIVisible(false);

        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
        }
    }

    public void OnBattleEnd()
    {
        HideAllDeityUI();
        CurrentDeity = null;
        _deityUnitComponent = null;
        _isInitialized = false;
    }
}