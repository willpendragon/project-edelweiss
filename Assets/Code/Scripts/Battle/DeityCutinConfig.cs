using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "NewDeityCutinConfig", menuName = "Battle/Deity Cutin Config")]
public class DeityCutinConfig : ScriptableObject
{
    [Header("Animation Timing")]
    [Tooltip("Duration for the cutin to slide in (seconds)")]
    [SerializeField] private float _slideInDuration = 0.3f;
    
    [Tooltip("Duration the cutin stays on screen (seconds)")]
    [SerializeField] private float _holdDuration = 0.8f;
    
    [Tooltip("Duration for the cutin to slide out (seconds)")]
    [SerializeField] private float _slideOutDuration = 0.3f;
    
    [Tooltip("Delay before the actual action executes after cutin completes (seconds)")]
    [SerializeField] private float _delayBeforeAction = 0.5f;

    [SerializeField] private float _bgFadeDelay = 0.4f;

    [Header("Animation Easing")]
    [Tooltip("Easing curve for slide in animation")]
    [SerializeField] private Ease _slideInEase = Ease.OutCubic;
    
    [Tooltip("Easing curve for slide out animation")]
    [SerializeField] private Ease _slideOutEase = Ease.InCubic;

    [Header("Visual Settings")]
    [Tooltip("The cutin prefab to display")]
    [SerializeField] private GameObject _cutinPrefab;
    
    [Tooltip("Whether to apply screen darkening effect during cutin")]
    [SerializeField] private bool _shouldDarkenScreen = true;

    // Read-only property getters
    public float SlideInDuration => _slideInDuration;
    public float HoldDuration => _holdDuration;
    public float SlideOutDuration => _slideOutDuration;
    public float DelayBeforeAction => _delayBeforeAction;
    public float BgFadeDelay => _bgFadeDelay;
    public Ease SlideInEase => _slideInEase;
    public Ease SlideOutEase => _slideOutEase;
    public GameObject CutinPrefab => _cutinPrefab;
    public bool ShouldDarkenScreen => _shouldDarkenScreen;

    // Calculated total duration
    public float TotalDuration => _slideInDuration + _holdDuration + _slideOutDuration + _delayBeforeAction;
}
