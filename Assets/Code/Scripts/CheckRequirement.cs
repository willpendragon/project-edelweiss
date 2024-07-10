using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckRequirement : MonoBehaviour
{
    public NodeController previousNode;
    public SceneLoader sceneLoader;
    public GameManager gameManager;

    public void Start()
    {
        sceneLoader = GameObject.FindGameObjectWithTag("SceneLoader").GetComponent<SceneLoader>();
        gameManager = GameManager.Instance;
    }
    public void CheckPreviousNode()
    {
        if (previousNode.nodeCompleted == true || this.gameObject.GetComponent<NodeController>().isStartingNode == true)
        {
            sceneLoader.ChangeScene();
            Debug.Log("You can enter this level");
        }
        else
        {
            Debug.Log("You can't enter this level");
        }
    }
    public void SetCurrentNode()
    {
        gameManager.currentNode = this.gameObject.GetComponent<NodeController>();
        gameManager.currentNode.SetIsCurrentNodeBool();
    }
}
