using UnityEngine;
using DG.Tweening;
using System.Linq;

public class IconDisplayHelper : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.1f;
    [SerializeField] private string iconSortingLayerName = "BattleIndicators";
    [SerializeField] private int iconSortingOrder = 5000;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not assigned on " + gameObject.name);
            return;
        }

        ApplyRenderPriority();
    }

    private void Start()
    {
        SetAlpha(0f); // Start invisible
    }

    public void ShowIcon()
    {
        if (spriteRenderer == null)
            return;

        // Kill any running tween to prevent conflicts
        ApplyRenderPriority();
        spriteRenderer.DOKill();
        spriteRenderer.DOFade(1f, fadeInDuration);
    }

    public void HideIcon()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.DOKill();
        spriteRenderer.DOFade(0f, fadeOutDuration)
            .OnComplete(() => Debug.Log("Thinking Icon Hidden"));
    }

    private void ApplyRenderPriority()
    {
        // Keep icon rendering above units and world-space unit UI.
        bool sortingLayerExists = SortingLayer.layers.Any(layer => layer.name == iconSortingLayerName);
        if (sortingLayerExists)
        {
            spriteRenderer.sortingLayerName = iconSortingLayerName;
        }
        else
        {
            Debug.LogWarning($"Sorting layer '{iconSortingLayerName}' not found for {gameObject.name}. Using current sorting layer.");
        }

        spriteRenderer.sortingOrder = iconSortingOrder;
    }

    private void SetAlpha(float alpha)
    {
        if (spriteRenderer == null)
            return;

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}