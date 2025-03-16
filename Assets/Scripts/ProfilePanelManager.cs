using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfilePanelManager : MonoBehaviour
{
    public Image avatarImage;
    public TMP_InputField usernameInput;
    public Sprite defaultAvatar; // Assign your default avatar image in the Inspector

    void Start()
    {
        LoadProfileData();
    }

    void LoadProfileData()
    {
        // Load saved username
        usernameInput.text = PlayerPrefs.GetString("Username", "Guest");

        // Load avatar (Automatically updates when changed)
        UpdateAvatar();
    }

    public void UpdateAvatar()
    {
        string avatarPath = PlayerPrefs.GetString("AvatarPath", ""); // Get saved avatar path

        if (!string.IsNullOrEmpty(avatarPath) && avatarPath != "DefaultAvatar")
        {
            Sprite newAvatar = Resources.Load<Sprite>("Avatars/" + avatarPath);
            if (newAvatar != null)
            {
                avatarImage.sprite = newAvatar; // Set selected avatar
            }
            else
            {
                avatarImage.sprite = defaultAvatar; // Use default if loading fails
            }
        }
        else
        {
            avatarImage.sprite = defaultAvatar; // Always use the specific default avatar
        }
    }

    public void SaveUsername()
    {
        PlayerPrefs.SetString("Username", usernameInput.text);
        PlayerPrefs.Save();
    }
}
