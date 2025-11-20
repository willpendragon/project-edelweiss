using UnityEngine;
using DG.Tweening;

public class IconDisplayHelper : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.1f;

    private void Awake()
    {
        if (spriteRenderer == null)
            Debug.LogError("SpriteRenderer not assigned on " + gameObject.name);
    }

    private void Start()
    {
        SetAlpha(0f); // Start invisible
    }

    public void ShowIcon()
    {
        // Kill any running tween to prevent conflicts
        spriteRenderer.DOKill();
        spriteRenderer.DOFade(1f, fadeInDuration);
    }

    public void HideIcon()
    {
        spriteRenderer.DOKill();
        spriteRenderer.DOFade(0f, fadeOutDuration)
            .OnComplete(() => Debug.Log("Thinking Icon Hidden"));
    }

    private void SetAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
