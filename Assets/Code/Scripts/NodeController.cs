using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeController : MonoBehaviour
{
    public bool nodeCompleted;
    public bool isCurrentNode;
    public bool isStartingNode;


public void SetIsCurrentNodeBool()
{
    isCurrentNode = true;
}

/*public void Update()
{
    if (isCurrentNode == true)
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
*/
}
