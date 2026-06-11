using TMPro;
using UnityEngine;

public class OverworldUIManager : MonoBehaviour
{
    public static OverworldUIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _keyCounterText;
    [SerializeField] private EventsUIManager _eventsUIManager;

    public EventsUIManager EventsUIManager => _eventsUIManager;
    public void UpdateKeyCounterText(string message)
    {
        _keyCounterText.text = message;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}