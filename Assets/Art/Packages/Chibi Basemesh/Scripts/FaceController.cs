using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[RequireComponent(typeof(Animator))]
public class FaceController : MonoBehaviour
{
    [Header("Render")]
    public Renderer _Render;
    public string textureName = "_MainTex";
    [Header("Face Sheet")]
    public FaceSheet sheet;
    [Range(0, 16)] public int frame = 0;

    private Vector2 m_Offset;
    private Vector2 m_TextureSize = Vector2.zero;

    public Vector2 Offset { get { return m_Offset; } }
    public Vector2 TextureSize { get { return m_TextureSize; } }

    public void UpdateSheet()
    {
        if (sheet && _Render)
        {
            CalcTextureSize();
            ApplyOffset();
        }
    }

    private void CalcTextureSize()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            _Render.sharedMaterial.SetTexture(textureName, sheet.texture);
            // Set the tile size of the texture (in UV units), based on the rows and columns
            m_TextureSize = new Vector2(1f / sheet.columns, 1f / sheet.rows);
            _Render.sharedMaterial.SetTextureScale(textureName, m_TextureSize);
            return;
        }
#endif
        _Render.material.SetTexture(textureName, sheet.texture);
        // Set the tile size of the texture (in UV units), based on the rows and columns
        m_TextureSize = new Vector2(1f / sheet.columns, 1f / sheet.rows);
        _Render.material.SetTextureScale(textureName, m_TextureSize);
    }

    private void ApplyOffset()
    {
        float fixorder = ((float)sheet.rows - 1f) / (float)sheet.rows;
        if (fixorder <= 0.5f)
        {
            fixorder = m_TextureSize.y;
        }
        // Split into x and y indexes. calculate the new offsets
        m_Offset = new Vector2((float)frame / sheet.columns - (frame / sheet.columns),
                                fixorder - ((frame / sheet.columns) / (float)sheet.rows));
        // Reset the y offset, if needed
        if (m_Offset.y == 1)
            m_Offset.y = 0.0f;
        // If we have scaled the texture, we need to reposition the texture to the center of the object
        m_Offset.x += ((1f / sheet.columns) - m_TextureSize.x) / 2.0f;
        m_Offset.y += ((1f / sheet.rows) - m_TextureSize.y) / 2.0f;
        // Update the material
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            _Render.sharedMaterial.SetTextureOffset(textureName, m_Offset);
            return;
        }
#endif
        _Render.material.SetTextureOffset(textureName, m_Offset);
    }
}