using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenu : MonoBehaviour
{
    [SerializeField] private GameObject _entryPrefab;
    [SerializeField] public List<RadialMenuEntry> entries = new List<RadialMenuEntry>();
    [SerializeField] private float _radius = 300f;

    void AddEntry(string actionLabel)
    {
        GameObject actionEntry = Instantiate(_entryPrefab, transform);
        RadialMenuEntry radialMenuEntry = actionEntry.GetComponent<RadialMenuEntry>();
        radialMenuEntry.SetLabel(actionLabel);

        entries.Add(radialMenuEntry);
    }

    public void Open()
    {
        for (int i = 0; i < 5; i++)
        {
            AddEntry("Button" + i.ToString());
        }
        ArrangeButtons();
    }

    public void ArrangeButtons()
    {
        float radiansOfSeparation = (Mathf.PI * 2 / entries.Count);
        for (int i = 0; i < entries.Count; i++)
        {
            float x = Mathf.Sin(radiansOfSeparation * i) * _radius;
            float y = Mathf.Cos(radiansOfSeparation * i) * _radius;

            entries[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y, 0);

        }
    }

    public void ClearButtonsList()
    {
        entries.Clear();
    }
}