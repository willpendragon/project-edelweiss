using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealingController : MonoBehaviour
{
    public UnityEvent<float> HealingPlayer;

    public Player player;
    [SerializeField] float healingAmount;
    [SerializeField] float healingPrice;

    public void HealPlayer()
    {
        player.healthPoints += healingAmount;
        player.coins -= healingPrice;
    }
}
