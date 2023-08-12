using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckRequirement : MonoBehaviour
{
    public NodeController previousNode;
    public SceneLoader sceneLoader;
    public GameManager gameManager;

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
    //this.gameObject.tag = "CurrentNode";

}
IEnumerator LoadBattleScene()
{
    yield return new WaitForSeconds(5);
    sceneLoader.LoadScene();
}
}
