using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(FaceSheet))]
public class FaceSheetEditor : Editor
{
    private static readonly string[] _Exclude = new string[] { "m_Script", "texture" };
    public override void OnInspectorGUI()
    {
        FaceSheet m_FaceSheet = (FaceSheet)target;
        serializedObject.Update();
        if (m_FaceSheet.texture != null)
        {
            m_FaceSheet.texture = (Texture)EditorGUILayout.ObjectField("Texture", m_FaceSheet.texture, typeof(Texture), allowSceneObjects: false);
            DrawPropertiesExcluding(serializedObject, _Exclude);
        }
        else
        {
            m_FaceSheet.texture = (Texture)EditorGUILayout.ObjectField("Texture", m_FaceSheet.texture, typeof(Texture), allowSceneObjects: false);
            EditorGUILayout.HelpBox("Texture Required", MessageType.Warning);
        }
        serializedObject.ApplyModifiedProperties();
    }
    public override bool HasPreviewGUI()
    {
        return true;
    }
    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        FaceSheet m_FaceSheet = (FaceSheet)target;
        if (Event.current.type == EventType.Repaint)
        {
            if (m_FaceSheet.texture)
            {
                GUI.DrawTexture(r, m_FaceSheet.texture, ScaleMode.ScaleToFit);
                EditorGUI.DropShadowLabel(r, "Columsn: " + m_FaceSheet.columns + ", Rows: " + m_FaceSheet.rows);
            }
            else
            {
                EditorGUI.DropShadowLabel(r, "Texture Required");
            }
        }
    }
}