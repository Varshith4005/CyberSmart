using UnityEngine;

public class ButtonClickManager : MonoBehaviour
{
    private static ButtonClickManager instance;
    private AudioSource audioSource;
    private bool isClickSoundEnabled;

    public AudioClip clickSound; // Assign this in the Inspector

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Ensure AudioSource is attached
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            isClickSoundEnabled = PlayerPrefs.GetInt("ClickSoundEnabled", 1) == 1;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static ButtonClickManager Instance => instance;

    public void SetClickSoundEnabled(bool isEnabled)
    {
        isClickSoundEnabled = isEnabled;
        PlayerPrefs.SetInt("ClickSoundEnabled", isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void PlayClickSound()
    {
        Debug.Log("PlayClickSound() called");

        if (isClickSoundEnabled && clickSound != null)
        {
            Debug.Log("Playing Click Sound...");
            
            if (audioSource.isPlaying) 
            {
                Debug.Log("AudioSource was playing, forcing restart...");
                audioSource.Stop(); // Force reset
            }

            audioSource.PlayOneShot(clickSound);
            Debug.Log("Sound should now play.");
        }
        else
        {
            Debug.LogWarning("Sound not playing - Either disabled, missing audioSource, or missing clip.");
        }
    }
}
