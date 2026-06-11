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

    public void SetTileColor(float glowIntensity, Color glowColor)
    {
        if (glowingTileColumn == null)
            return;
        glowingTileColumn.material.SetFloat("_GlowIntensity", glowIntensity);
        glowingTileColumn.material.color = glowColor;
    }
    public void SetTileGlowIntensity(float glowIntensity)
    {
        if (glowingTileColumn == null)
            return;
        glowingTileColumn.material.SetFloat("_GlowIntensity", glowIntensity);
    }

    public void EnemyTileFeedback()
    {
        if (glowingTileColumn != null)
        {
            glowingTileColumn.material.color = Color.red;
            glowingTileColumn.material.SetFloat("_GlowIntensity", 1f);
        }
    }

    public void ResetEnemyTileFeedback()
    {
        if (glowingTileColumn != null)
        {
            glowingTileColumn.material.SetFloat("_GlowIntensity", 0f);
            SetTileToMoveRangeColor();
        }
    }

    public void ResetTileFadeHeightAnimation(TileController tileToReset)
    {
        glowingTileColumn.material.SetFloat("_GlowIntensity", 0f);
    }

    public Color RetrieveCurrentTileColor()
    {
        Color tileColor = glowingTileColumn.material.GetColor("_Color");
        return tileColor;
    }
}
