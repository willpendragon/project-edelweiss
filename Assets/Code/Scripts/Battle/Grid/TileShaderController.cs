using UnityEngine;
using DG.Tweening;

public class TileShaderController : MonoBehaviour
{
    public float targetFadeHeight = 2.75f; // Target fade height for the animation
    public float animationDuration = 2.0f; // Duration of the animation
    public MeshRenderer glowingTileColumn; // Reference to the glowing mesh
    public Ease animationEase = Ease.InOutQuad; // Easing type for the animation
    private Tween fadeHeightTween;
    private const string TILE_MOVEMENT_RANGE_COLOR = "#7984DC";

    void Start()
    {
        if (glowingTileColumn == null)
        {
            Debug.LogError("Glowing Mesh not assigned.");
        }
        glowingTileColumn.material.SetFloat("_GlowIntensity", 0f);
        SetTileToMoveRangeColor();
    }
    public void SetTileToMoveRangeColor()
    {
        Color myColor;
        if (ColorUtility.TryParseHtmlString(TILE_MOVEMENT_RANGE_COLOR, out myColor))
        {
            glowingTileColumn.material.color = myColor;
        }
    }

    public void AnimateFadeHeight(float targetFadeHeight, float animationDuration, Color glowColor)
    {
        if (glowingTileColumn != null)
        {
            glowingTileColumn.material.SetFloat("_GlowIntensity", 1f);
        }
    }
    public void SetTileColor(float glowIntensity, Color glowColor)
    {
        if (glowingTileColumn == null)
            return;
        glowingTileColumn.material.SetFloat("_GlowIntensity", glowIntensity);
        glowingTileColumn.material.color = glowColor;
    }

    public void ResetTileGlowIntensity()
    {
        if (glowingTileColumn == null)
            return;
        glowingTileColumn.material.SetFloat("_GlowIntensity", 0f);
    }

    public void EnemyTileFeedback(float targetFadeHeight, float animationDuration, Color glowColor)
    {
        if (glowingTileColumn != null)
        {
            glowingTileColumn.material.color = glowColor;
            glowingTileColumn.material.SetFloat("_GlowIntensity", 1f);
        }
    }

    public void ResetEnemyTileFeedback(float targetFadeHeight, float animationDuration, Color glowColor)
    {
        if (glowingTileColumn != null)
        {
            glowingTileColumn.material.SetFloat("_GlowIntensity", 0f);
            SetTileToMoveRangeColor();
        }
    }

    public void AnimateFadeHeightPulse(float minFadeHeight, float maxFadeHeight, float halfCycleDuration, Color glowColor)
    {
        if (glowingTileColumn != null)
        {
            // Set the initial glow color
            glowingTileColumn.material.color = glowColor;

            // Kill any existing tween
            fadeHeightTween?.Kill();

            // Start pulsating between min and max
            fadeHeightTween = DOTween.To(
                    () => glowingTileColumn.material.GetFloat("_FadeHeight"),
                    x => glowingTileColumn.material.SetFloat("_FadeHeight", x),
                    maxFadeHeight,
                    halfCycleDuration
                )
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void AnimateFadeHeightError(float targetFadeHeight, float animationDuration, Color glowColor)
    {
        if (glowingTileColumn != null)
        {
            // Store the initial value to revert back to it later
            float initialFadeHeight = glowingTileColumn.material.GetFloat("_FadeHeight");

            // Set the glow color
            glowingTileColumn.material.color = glowColor;

            // Create the initial animation to the target fade height
            DOTween.To(() => glowingTileColumn.material.GetFloat("_FadeHeight"),
                       x => glowingTileColumn.material.SetFloat("_FadeHeight", x),
                       targetFadeHeight,
                       animationDuration)
                .SetEase(animationEase) // Apply easing to the animation
                .OnComplete(() => // When the first animation completes, revert to the initial value
                {
                    DOTween.To(() => glowingTileColumn.material.GetFloat("_FadeHeight"),
                               x => glowingTileColumn.material.SetFloat("_FadeHeight", x),
                               initialFadeHeight,
                               animationDuration)
                        .SetEase(animationEase); // Apply easing for the revert animation as well
                });
        }
    }

    public void ResetTileFadeHeightAnimation(TileController tileToReset)
    {
        glowingTileColumn.material.SetFloat("_GlowIntensity", 0f);
    }
    public void StopFadeHeightPulse()
    {
        fadeHeightTween?.Kill();
        fadeHeightTween = null;
        AnimateFadeHeight(0, 0, Color.white);
    }
}
