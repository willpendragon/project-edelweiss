using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrystalBlendingController : MonoBehaviour
{
    [SerializeField] GameObject crystalObject;
    [SerializeField] Slider slider;
    [SerializeField] Button[] ingredientButtons;
    //[SerializeField] RectTransform ingredientsButtonsGrid;
    [SerializeField] Animator blendingMachine;

    private void Start()
    {
        //foreach (var button in ingredientButtons)
        //{
        //    Instantiate(button, ingredientsButtonsGrid);
        //}
    }
    public void ThrowIngredient()
    {
        GameObject crystalObjectInstance = Instantiate(crystalObject);
        crystalObjectInstance.GetComponent<Animator>().SetTrigger("CrystalTossed");
        slider.value += 1;
        Destroy(crystalObjectInstance, 5);
    }

    public void BlendIngredients()
    {
        blendingMachine.SetTrigger("CrystalBlending");
    }
}
