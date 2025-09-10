using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CloseMenuBehind : MonoBehaviour
{
    [SerializeField] private CafeMenuUIWindowsController _cafeMenu;
    private const string CAFE_SCENE_NAME = "cafe_node";

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void ShowCanvas()
    {
        if (_cafeMenu == null)
            return;

        _cafeMenu.cafeMenuGroup.alpha = 1;
    }
    public void HideCanvas()
    {
        if (_cafeMenu == null)
            return;

        _cafeMenu.cafeMenuGroup.alpha = 0;
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == CAFE_SCENE_NAME)
        {
            _cafeMenu = FindAnyObjectByType<CafeMenuUIWindowsController>();
        }
    }
}
