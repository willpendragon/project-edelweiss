using UnityEditor;
[CustomEditor(typeof(FaceController))]
public class FaceControllerEditor : Editor
{
    private static readonly string[] _Exclude = new string[] { "m_Script" };
    public override void OnInspectorGUI()
    {
        FaceController m_FaceController = (FaceController)target;
        EditorApplication.update += m_FaceController.UpdateSheet;

        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, _Exclude);
        serializedObject.ApplyModifiedProperties();
        if (m_FaceController.sheet)
        {
            EditorGUILayout.HelpBox("frames: " + m_FaceController.sheet.columns * m_FaceController.sheet.rows + "\n" +
                                    "Offset: " + m_FaceController.Offset + "\n" +
                                    "Size: " + m_FaceController.TextureSize, MessageType.Info);
        }
    }
}