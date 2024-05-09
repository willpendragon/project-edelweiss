using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BanterSpawner : MonoBehaviour
{
    public GameObject banterPrefab;
    public string banterText;
    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "battle_prototype")
        {
            SpawnBanter(this.transform.position, banterText);
        }
    }
    public void SpawnBanter(Vector3 position, string text)
    {
        GameObject banterInstance = Instantiate(banterPrefab, position, Quaternion.identity, transform);
        TextMeshProUGUI textComponent = banterInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = text;
        }

        Destroy(banterInstance, 7f);
    }
}