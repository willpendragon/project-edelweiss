using UnityEngine;
using UnityEngine.UI;

public class BarVisualizationHelper : MonoBehaviour
{
    [SerializeField] Animator barAnimator;
    [SerializeField] Slider barSlider;
    public void CheckBarStatus()
    {
        if (barSlider != null)
        {
            if (barSlider.value <= 0)
            {
                barAnimator.SetTrigger("BarDepleted");
            }
            else
            {
                return;
            }
        }
    }
}
