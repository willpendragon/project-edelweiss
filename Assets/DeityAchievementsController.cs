using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeityAchievementsController : MonoBehaviour
{
    public int killedEnemies;
    public int anguanaUnlockMilestone;

    public void Start()
    {
        CheckRequirements();
    }
    public bool CheckRequirements()
    {
        //Checks how many enemies have been killed. If the threshold is met, the method returns "true", allowing the logic to catch Anguana inside the gameplay flow.
        if (killedEnemies >= anguanaUnlockMilestone)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
