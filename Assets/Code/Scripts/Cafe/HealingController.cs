using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealingController : MonoBehaviour
{
    public UnityEvent<float> HealingPlayer;

    public delegate void SavePoint(float savedPlayerHealth);
    public static event SavePoint OnSavePoint;

    //public Player player;
    [SerializeField] float healingAmount;
    [SerializeField] float healingPrice;

    //public void HealPlayer()
    //{
    //    if (player.coins > 0 && player.coins >= healingAmount)
    //    {
    //        player.healthPoints += healingAmount;
    //        player.coins -= healingPrice;
    //        OnSavePoint(player.healthPoints);
    //    }
    //}
}
