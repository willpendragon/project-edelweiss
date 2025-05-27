using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrystalBlendingController : MonoBehaviour
{
    [SerializeField] GameObject crystalObject;
    [SerializeField] Slider slider;

    public void ThrowIngredient()
    {
        GameObject crystalObjectInstance = Instantiate(crystalObject);
        crystalObjectInstance.GetComponent<Animator>().SetTrigger("CrystalTossed");
        slider.value += 1;
        Destroy(crystalObjectInstance, 5);
    }
}
