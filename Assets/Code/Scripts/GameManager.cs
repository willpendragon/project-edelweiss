using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public NodeController currentNode;
    private static GameManager _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
            return;
        }
        Destroy(this.gameObject);
    }
    public void MarkCurrentNodeAsCompleted()

    {
        currentNode.nodeCompleted = true;
    }
}
