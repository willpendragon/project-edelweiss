using UnityEngine;
using DG.Tweening;

public class CrystalAnimation : MonoBehaviour
{
    public float rotationDuration = 5f;
    public float upDownDuration = 2f;
    public float upDownDistance = 0.5f;

    void Start()
    {
        // Start the rotation animation
        RotateCrystal();

        // Start the up and down movement animation
        MoveCrystalUpDown();
    }

    void RotateCrystal()
    {
        // Rotate the crystal around the Y-axis indefinitely
        transform.DORotate(new Vector3(0, 360, 0), rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    void MoveCrystalUpDown()
    {
        // Move the crystal up and down indefinitely
        transform.DOMoveY(transform.position.y + upDownDistance, upDownDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
