using UnityEngine;

public class OverworldMapManager : MonoBehaviour
{
    public static OverworldMapManager Instance;

    [SerializeField] CalendarController _calendarController;

    public CalendarController CalendarController => _calendarController;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
}
