using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioClip tickClip;
    public AudioClip aciertoClip;
    public AudioClip errorClip;
    public AudioClip victoryClip;

    private AudioSource sfxSource;
    private float sfxVolume = 0.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persistir entre escenas
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        sfxSource.volume = sfxVolume;
    }

    public void PlayTick() => PlaySound(tickClip);
    public void PlayAcierto() => PlaySound(aciertoClip);
    public void PlayError() => PlaySound(errorClip);
    public void PlayVictory() => PlaySound(victoryClip);

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void SetVolume(float volume)
    {
        sfxVolume = volume;
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}

