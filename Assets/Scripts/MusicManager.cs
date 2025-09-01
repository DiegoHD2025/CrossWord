using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioClip menuMusic;
    public AudioClip levelMusic;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

        // Reproduce música para la primera escena
        PlayMusicForScene(SceneManager.GetActiveScene().name);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    void PlayMusicForScene(string sceneName)
    {
        // Escena de menú principal
        if (sceneName == "Menu" || sceneName == "MenuPrincipal" || sceneName == "Main")
        {
            if (audioSource.clip != menuMusic)
            {
                audioSource.clip = menuMusic;
                audioSource.Play();
            }
        }
        // Escenas de niveles
        else if (
            sceneName == "SampleScene" ||
            sceneName == "IntermediateGame" ||
            sceneName == "AdvanceGame")
        {
            if (audioSource.clip != levelMusic)
            {
                audioSource.clip = levelMusic;
                audioSource.Play();
            }
        }
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
}

