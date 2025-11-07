using UnityEngine;
using UnityEngine.SceneManagement;
public class CloseMenuBehind : MonoBehaviour
{
    [SerializeField] private CafeMenuUIWindowsController _cafeMenuWindowsController;
    [SerializeField] private CanvasGroup[] _cafeMenus;
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
        if (_cafeMenus.Length == 0)
            return;
        // Shows only the Dialogue tab.
        _cafeMenus[1].alpha = 1;
    }
    public void HideCanvas()
    {
        if (_cafeMenus.Length == 0)
            return;
        foreach (var menu in _cafeMenus)
        {
            menu.alpha = 0;
        }
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == CAFE_SCENE_NAME)
        {
            _cafeMenuWindowsController = FindAnyObjectByType<CafeMenuUIWindowsController>();
            _cafeMenus = _cafeMenuWindowsController.cafeMenus;
        }
    }
}
