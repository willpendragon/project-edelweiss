using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VersionShower : MonoBehaviour
{
    private void Awake()
    {
       if (TryGetComponent(out TextMeshProUGUI output))
            output.text = Application.version;
    }
}
