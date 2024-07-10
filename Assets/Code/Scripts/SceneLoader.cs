using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] string sceneName;
    private float loadingBattleSceneWaitTime = 1;

    public delegate void LoadingScene();
    public static event LoadingScene OnLoadingScene;
    public void ChangeScene()
    {
        OnLoadingScene();
        StartCoroutine("LoadScene");
    }
    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(loadingBattleSceneWaitTime);
        SceneManager.LoadSceneAsync(sceneName);
    }
}
