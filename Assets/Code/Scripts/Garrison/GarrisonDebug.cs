using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GarrisonDebug : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Jump from Cafe into the Garrison/Recruit scene
        if (Input.GetKeyDown(KeyCode.F12))
        {
            LoadGarrison();
        }
    }

    public void LoadGarrison()
    {
        Debug.Log("Loading 'garrison' scene...");
        SceneManager.LoadScene("garrison");
    }
}
