using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MasterVolumeController : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider volumeSlider;

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        volumeSlider.value = savedVolume;
        SetVolume(savedVolume);
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }
}
