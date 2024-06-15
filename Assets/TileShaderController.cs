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
        //else
        //{
        //    AnimateFadeHeight();
        //}
    }

    //Add parameters
    public void AnimateFadeHeight(float targetFadeHeight, float animationDuration, Color glowColor)
    {
        if (glowingTileColumn != null)
        {
            glowingTileColumn.material.color = glowColor;
            DOTween.To(() => glowingTileColumn.material.GetFloat("_FadeHeight"), x => glowingTileColumn.material.SetFloat("_FadeHeight", x), targetFadeHeight, animationDuration)
                .SetEase(animationEase); // Apply easing to the animation
        }
    }

    public void ResetTileFadeHeightAnimation(TileController tileToReset)
    {
        tileToReset.tileShaderController.AnimateFadeHeight(0f, 0.2f, Color.white);
    }
}
