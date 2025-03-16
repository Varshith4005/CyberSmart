using UnityEngine;
using UISwitcher; // Import UISwitcher namespace
using UnityEngine.Android; // Required for Android vibration support

public class GameSettingsManager : MonoBehaviour
{
    public UISwitcher.UISwitcher musicToggle;
    public UISwitcher.UISwitcher clickToggle;
    public UISwitcher.UISwitcher notificationsToggle;
    public UISwitcher.UISwitcher vibrationToggle; // Added Vibration Toggle

    void Start()
    {
        // Load saved states
        musicToggle.isOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        clickToggle.isOn = PlayerPrefs.GetInt("ClickSoundEnabled", 1) == 1;
        notificationsToggle.isOn = PlayerPrefs.GetInt("NotificationsEnabled", 1) == 1;
        vibrationToggle.isOn = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1; // Load vibration state

        // Add listeners to detect switch state changes
        musicToggle.onValueChanged.AddListener(ToggleMusic);
        clickToggle.onValueChanged.AddListener(ToggleClickSounds);
        notificationsToggle.onValueChanged.AddListener(ToggleNotifications);
        vibrationToggle.onValueChanged.AddListener(ToggleVibration);
    }

    void ToggleMusic(bool isOn)
    {
        if (BackgroundMusicManager.Instance != null)
        {
            BackgroundMusicManager.Instance.SetMusicEnabled(isOn);
        }
        PlayerPrefs.SetInt("MusicEnabled", isOn ? 1 : 0);
    }

    void ToggleClickSounds(bool isOn)
    {
        if (ButtonClickManager.Instance != null)
        {
            ButtonClickManager.Instance.SetClickSoundEnabled(isOn);
        }
        PlayerPrefs.SetInt("ClickSoundEnabled", isOn ? 1 : 0);
    }

    void ToggleNotifications(bool isOn)
    {
        PlayerPrefs.SetInt("NotificationsEnabled", isOn ? 1 : 0);
        Debug.Log("Notifications " + (isOn ? "Enabled" : "Disabled"));
    }

    void ToggleVibration(bool isOn)
    {
        PlayerPrefs.SetInt("VibrationEnabled", isOn ? 1 : 0);
        Debug.Log("Vibration " + (isOn ? "Enabled" : "Disabled"));
    }

    public static void Vibrate()
    {
        if (PlayerPrefs.GetInt("VibrationEnabled", 1) == 1)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate(); // Vibrate the device (Android only)
#endif
            Debug.Log("Device Vibrated");
        }
    }
}
