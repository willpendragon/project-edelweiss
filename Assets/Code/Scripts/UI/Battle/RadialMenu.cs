using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class RadialMenu : MonoBehaviour
{
    [SerializeField] private GameObject _entryPrefab;
    [SerializeField] public List<RadialMenuEntry> entries = new List<RadialMenuEntry>();
    [SerializeField] private float _radius = 300f;

    // Define fixed "slots" (clock positions in degrees)
    private Dictionary<int, float> fixedAngles = new Dictionary<int, float>()
    {
        {1, 90f},   
        {2, 0f},    
        {3, 270f}, 
        {4, 180f},  
        {5, 45f},   
        {6, 135f},  
        {7, 225f},  
        {8, 315f},
        {9, 340f},  // Custom position for Attunement (band-aid fix).
    };

    // Spacing.
    private Dictionary<int, float> customRadiusMultiplier = new Dictionary<int, float>()
    {
        {9, 1.3f}, // Specific spacing for Attunement button, band-aid fix. 
    };

    // Manual Offsets: XY

    private Dictionary<int, Vector2> customOffsets = new Dictionary<int, Vector2>()
    {
        {9, new Vector2(50f, 0f)} // Specific offset for Attunement Button.
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

        // Apply custom radius when applicable.
        float effectiveRadius = _radius;
        if (customRadiusMultiplier.ContainsKey(entry.priority))
        {
            effectiveRadius *= customRadiusMultiplier[entry.priority];
        }

        float x = MathF.Cos(angleRad) * effectiveRadius;
        float y = Mathf.Sin(angleRad) * effectiveRadius;

        // float x = Mathf.Cos(angleRad) * _radius;
        // float y = Mathf.Sin(angleRad) * _radius;

        // When applicable, pick and apply an offset from the dictionary (currently only Attunement icon has an offset).
        if (customOffsets.ContainsKey(entry.priority))
        {
            x += customOffsets[entry.priority].x;
            y += customOffsets[entry.priority].y;
        }
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