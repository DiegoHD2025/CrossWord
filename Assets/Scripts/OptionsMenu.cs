using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Scrollbar musicScrollbar;
    public Scrollbar sfxScrollbar;

    void Start()
    {
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        musicScrollbar.value = musicVol;
        MusicManager.Instance?.SetVolume(musicVol);

        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        sfxScrollbar.value = sfxVol;
        SoundManager.Instance?.SetVolume(sfxVol);
    }

    public void OnMusicVolumeChanged()
    {
        float volume = musicScrollbar.value;
        MusicManager.Instance?.SetVolume(volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void OnSFXVolumeChanged()
    {
        float volume = sfxScrollbar.value;
        SoundManager.Instance?.SetVolume(volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}


