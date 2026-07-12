using UnityEngine;
using DG.Tweening;

namespace ProjectEdelweiss.Settings
{
    [CreateAssetMenu(fileName = "NewEnemyCameraSettings", menuName = "Camera/Enemy Camera Settings")]
    public class EnemyCameraSettings : ScriptableObject
    {
        [Header("Transition Timings")]
        [Tooltip("Change this to adjust how long it takes the camera to pan to an enemy.")]
        public float PanDuration = 1.2f;

        [Tooltip("Change this to adjust the easing curve. InOutSine/InOutQuad most probably look better to achieve FFTA style.")]
        public Ease PanEase = Ease.InOutSine;

        [Header("Optional Pauses")]
        [Tooltip("Waiting time, I can hook into this later to add a pause between each time the camera focuses on an enemy.")]

        public float PostPanDelay = 0.5f;

        // Values concerning only Deity's full Angry Meter camera close up.

        public float AngeredPanDuration = 0.4f;
        public Ease AngeredPanEase = Ease.OutBack;
        public float AngeredPauseDuration = 1.5f;
    }
}