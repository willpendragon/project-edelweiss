using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MasterVolumeController : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider volumeSlider;
    public string mixerName;

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(mixerName, 0.75f);
        volumeSlider.value = savedVolume;
        SetVolume(savedVolume);
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat(mixerName, Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(mixerName, volume);
    }
}
