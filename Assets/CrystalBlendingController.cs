using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrystalBlendingController : MonoBehaviour
{
    [SerializeField] GameObject crystalObject;
    [SerializeField] Slider slider;
    [SerializeField] Button[] ingredientButtons;
    [SerializeField] Animator blendingMachine;
    [SerializeField] TextMeshProUGUI ingredientNotification;
    [SerializeField] string newIngredientAddedText = "New Ingredient Added";
    [SerializeField] RectTransform blendingResultPanel;
    public void ThrowIngredient()
    {
        GameObject crystalObjectInstance = Instantiate(crystalObject);
        crystalObjectInstance.GetComponent<Animator>().SetTrigger("CrystalTossed");
        ingredientNotification.text = newIngredientAddedText;
        slider.value += 1;
        float destroyDelay = 5f;
        Destroy(crystalObjectInstance, destroyDelay);
        StartCoroutine(ResetStartText(2f));
    }
    IEnumerator ResetStartText(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        ingredientNotification.text = null;
    }

    public void BlendIngredients()
    {
        blendingMachine.SetTrigger("CrystalBlending");
        StartCoroutine(DisplayRecipeResultAnimation(3f));
    }

    IEnumerator DisplayRecipeResultAnimation(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        blendingResultPanel.localScale = Vector3.one;
    }
}
