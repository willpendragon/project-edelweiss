using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemFoodIconHelper : MonoBehaviour
{
    [SerializeField] private GameObject _orderButton;
    public void ActivateOrderButton(bool flag)
    {
        if (flag == true)
        {
            _orderButton.SetActive(true);
        }
        else
        {
            _orderButton.SetActive(false);
        }
    }
}
