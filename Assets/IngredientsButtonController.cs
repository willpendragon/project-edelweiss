using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngredientsButtonController : MonoBehaviour
{
    [SerializeField] Sprite ingredientImage;
    [SerializeField] Image ingredientImageUI;

    private void Start()
    {
        ingredientImageUI.sprite = ingredientImage;
    }
}