using UnityEngine;
using DG.Tweening;

public class TileShaderController : MonoBehaviour
{
    public float targetFadeHeight = 2.75f; // Target fade height for the animation
    public float animationDuration = 2.0f; // Duration of the animation
    public MeshRenderer glowingTileColumn; // Reference to the glowing mesh
    public Ease animationEase = Ease.InOutQuad; // Easing type for the animation


    void Start()
    {
        if (glowingTileColumn == null)
        {
            Debug.LogError("Glowing Mesh not assigned.");
        }
    }

    public void AnimateFadeHeight(float targetFadeHeight, float animationDuration, Color glowColor)
    {
        if (glowingTileColumn != null)
        {
            glowingTileColumn.material.color = glowColor;
            DOTween.To(() => glowingTileColumn.material.GetFloat("_FadeHeight"), x => glowingTileColumn.material.SetFloat("_FadeHeight", x), targetFadeHeight, animationDuration)
                .SetEase(animationEase); // Apply easing to the animation
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
        tileToReset.tileShaderController.AnimateFadeHeight(0f, 0.2f, Color.white);
    }
}
