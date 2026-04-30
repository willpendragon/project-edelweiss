using UnityEngine;

public enum SoundType
{
    // Remember in inspector to match the enum sequence.
    BATTLEBEGINS,
    NEXTTURN,
    SELECT, // Blue grid highlight.
    UIHOVER, // UI Hover.
    CONFIRMMOVE, // UI Select.
    RADIALHOVERSWITCH,
    POPUPMESSAGE,
    UIDIALOGUEOPEN,
    UIDIALOGUECLOSE,
    CRITICALHIT,
    ENEMYDEATH,
    ENEMYHIT,
    SWORDATTACKKNOCKBACK,
    MAGNET,
    PICKUPKEY,
    PICKUPUPGRADE,
    PICKUPMINIBOSSKEY,
    PICKUPBOSSKEY,
    SWORDATTACK
}

[RequireComponent(typeof(AudioSource))]
public class BattleSFXManager : MonoBehaviour
{
    public void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public static BattleSFXManager Instance { get; private set; }

    [SerializeField] private AudioClip[] soundList;
    private AudioSource _audioSource;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        Instance._audioSource.PlayOneShot(Instance.soundList[(int)sound], volume);
    }
}