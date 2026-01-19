using System.Collections.Generic;
using UnityEngine;

public class RadialMenu : MonoBehaviour
{
    [SerializeField] private GameObject _entryPrefab;
    [SerializeField] public List<RadialMenuEntry> entries = new List<RadialMenuEntry>();
    [SerializeField] private float _radius = 300f;

    // Define fixed "slots" (clock positions in degrees)
    private Dictionary<int, float> fixedAngles = new Dictionary<int, float>()
    {
        {1, 90f},   // 3 o’clock
        {2, 0f},    // 12 o’clock
        {3, 270f},  // 9 o’clock
        {4, 180f},  // 6 o’clock
        {5, 45f},   // 1:30
        {6, 135f},  // 4:30
        {7, 225f},  // 7:30
        {8, 315f},  // 10:30 
    };

    public void ArrangeButtons()
    {
        entries.Sort((a, b) => a.priority.CompareTo(b.priority));

        foreach (RadialMenuEntry entry in entries)
        {
            if (!fixedAngles.ContainsKey(entry.priority))
            {
                continue;
            }

            float angleDeg = fixedAngles[entry.priority]; // The position of the Icon on the radial menu is driven by the priority.
            float angleRad = angleDeg * Mathf.Deg2Rad;

            float x = Mathf.Cos(angleRad) * _radius;
            float y = Mathf.Sin(angleRad) * _radius;

            entry.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y, 0);
        }
    }

    public void ClearButtonsList()
    {
        foreach (var entry in entries)
        {
            if (entry != null)
                Destroy(entry.gameObject);
        }
        entries.Clear();
    }
}