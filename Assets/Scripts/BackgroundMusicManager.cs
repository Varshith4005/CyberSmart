using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    private static BackgroundMusicManager instance;
    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static BackgroundMusicManager Instance => instance;

    void Start()
    {
        bool isMusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        SetMusicEnabled(isMusicEnabled);
    }

    public void SetMusicEnabled(bool isEnabled)
    {
        if (audioSource != null)
        {
            audioSource.mute = !isEnabled;
        }
    }
}
