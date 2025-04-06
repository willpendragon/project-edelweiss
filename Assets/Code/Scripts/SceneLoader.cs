using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] string sceneName;
    [SerializeField] string currentSceneName;
    private float loadingSceneWaitTime = 1;

    public delegate void LoadingScene();
    public static event LoadingScene OnLoadingScene;
    public void Start()
    {
        if (currentSceneName == "battle_transition")
        {
            // Start the Mountain Ascending Sequence
            StartCoroutine("LoadBattle");
            Debug.Log("Playing the Mountain Ascending Sequence");
        }
        else if (sceneName == "battle_prototype")
        {
            SceneManager.LoadSceneAsync(sceneName);
        }
    }

    IEnumerator LoadBattle()
    {
        float loadingBattleSceneWaitTime = 2.8f;
        yield return new WaitForSeconds(loadingBattleSceneWaitTime);
        SceneManager.LoadSceneAsync("battle_prototype");
    }

    public void ChangeScene()
    {
        OnLoadingScene();
        StartCoroutine("LoadScene");
    }
    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(loadingSceneWaitTime);
        SceneManager.LoadSceneAsync(sceneName);
    }
}
