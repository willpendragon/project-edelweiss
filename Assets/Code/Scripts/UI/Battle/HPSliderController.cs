using UnityEngine;
using UnityEngine.UI;

public class HPSliderController : MonoBehaviour
{
    public Slider slider;

    public void DestroySlider()
    {
        if (slider.value <= 0)
        {
            Destroy(transform.gameObject);
        }
    }
}