using UnityEngine;

[CreateAssetMenu(fileName = "New Face Sheet", menuName = "Face Sheet", order = 310)]
public class FaceSheet : ScriptableObject
{
    public Texture texture;
    public int columns = 2;
    public int rows = 2;
    private void OnValidate()
    {
        if (columns <= 0) { columns = 1; }
        if (rows <= 0) { rows = 1; }
    }
}