using TMPro;
using UnityEngine;

public class OverworldUIManager : MonoBehaviour
{
    public static OverworldUIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _keyCounterText;
    [SerializeField] private TextMeshProUGUI _miniBossKeyCounterText;
    [SerializeField] private TextMeshProUGUI _bossKeyCounterText;
    [SerializeField] private EventsUIManager _eventsUIManager;

    public EventsUIManager EventsUIManager => _eventsUIManager;

    public void UpdateKeyCounterText(string message)
    {
        _keyCounterText.text = message;
    }

    public void UpdateMiniBossKeyCounterText(string message)
    {
        _miniBossKeyCounterText.text = message;
    }

    public void UpdateBossKeyCounterText(string message)
    {
        _bossKeyCounterText.text = message;
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