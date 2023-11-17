using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckRequirement : MonoBehaviour
{
    public NodeController previousNode;
    public SceneLoader sceneLoader;
    public GameManager gameManager;
    private float loadingBattleSceneWaitTime = 1;

    public delegate void LoadingBattle();
    public static event LoadingBattle OnLoadingBattle;

    public void CheckPreviousNode()
    {
        if (previousNode.nodeCompleted == true || this.gameObject.GetComponent<NodeController>().isStartingNode == true)
        {
            StartCoroutine("LoadBattleScene");
            Debug.Log("You can proceed to next level");
        }
        else
        {
            Debug.Log("You don't meet the prerequisites to enter the next level");
        }
    }

    public void SetCurrentNode()
    {
        gameManager.currentNode = this.gameObject.GetComponent<NodeController>();
        gameManager.currentNode.SetIsCurrentNodeBool();
    }
    IEnumerator LoadBattleScene()
    {
        OnLoadingBattle();
        yield return new WaitForSeconds(loadingBattleSceneWaitTime);
        sceneLoader.LoadScene();
    }
}
