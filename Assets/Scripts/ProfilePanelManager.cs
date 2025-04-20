using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Collections;

public class ProfilePanelManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image avatarImage;              // Avatar display
    public TMP_Text usernameText;          // Username display
    public TMP_Text honorPointsText;       // Honor points display
    public TMP_Text equippedTitleText;     // Equipped title display
    public Sprite defaultAvatar;           // Default avatar image

    private DatabaseReference dbReference;
    private string userEmail;              // Email (Fetched but not displayed)
    private int highestScore;              // Highest Score (Fetched but not displayed)

    void Start()
    {
        // ✅ Load cached data instantly for quick display
        LoadCachedData();

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                LoadProfileData();
            }
        });
    }

    /// <summary>
    /// Load cached data instantly to avoid delay.
    /// </summary>
    void LoadCachedData()
    {
        // ✅ Load locally cached username, honor points, and equipped title
        usernameText.text = PlayerPrefs.GetString("Username", "Guest");
        honorPointsText.text = $"Honor: {PlayerPrefs.GetInt("HonorPoints", 0)}";

        // ✅ Load equipped title from cache
        string equippedTitle = PlayerPrefs.GetString("EquippedTitle", "No Title Equipped");
        equippedTitleText.text = $"{equippedTitle}";

        // ✅ Load avatar instantly from cache
        UpdateAvatar();
    }

    /// <summary>
    /// Fetch profile data from Firebase in the background and cache it.
    /// </summary>
    void LoadProfileData()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;

        if (user != null)
        {
            string userId = user.UserId;

            // Fetch profile data from Firebase
            dbReference.Child("users").Child(userId).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    DataSnapshot snapshot = task.Result;

                    // ✅ Load and cache username
                    if (snapshot.Child("username").Exists)
                    {
                        string username = snapshot.Child("username").Value.ToString();
                        usernameText.text = username;
                        PlayerPrefs.SetString("Username", username);
                    }

                    // ✅ Load and cache honor points
                    if (snapshot.Child("honorPoints").Exists)
                    {
                        int honorPoints = int.Parse(snapshot.Child("honorPoints").Value.ToString());
                        honorPointsText.text = $"Honor: {honorPoints}";
                        PlayerPrefs.SetInt("HonorPoints", honorPoints);
                    }

                    // ✅ Load and cache equipped title
                    if (snapshot.Child("equippedTitle").Exists)
                    {
                        string equippedTitle = snapshot.Child("equippedTitle").Value.ToString();
                        equippedTitleText.text = $"{equippedTitle}";
                        PlayerPrefs.SetString("EquippedTitle", equippedTitle);
                    }
                    else
                    {
                        equippedTitleText.text = "No Title Equipped";
                    }

                    // ✅ Fetch email and highest score (not displayed)
                    if (snapshot.Child("email").Exists)
                    {
                        userEmail = snapshot.Child("email").Value.ToString();
                    }

                    if (snapshot.Child("highestScore").Exists)
                    {
                        highestScore = int.Parse(snapshot.Child("highestScore").Value.ToString());
                    }

                    // ✅ Load equipped avatar from Firebase
                    if (snapshot.Child("equippedAvatar").Exists)
                    {
                        string equippedAvatar = snapshot.Child("equippedAvatar").Value.ToString();

                        if (!string.IsNullOrEmpty(equippedAvatar))
                        {
                            // Cache the equipped avatar
                            PlayerPrefs.SetString("EquippedAvatar", equippedAvatar);
                            PlayerPrefs.Save();

                            DisplayEquippedAvatar(equippedAvatar);
                        }
                        else
                        {
                            DisplayDefaultAvatar();
                        }
                    }
                    else
                    {
                        DisplayDefaultAvatar();
                    }

                    // ✅ Save all data to cache
                    PlayerPrefs.Save();
                }
                else
                {
                    // ⚠️ Fallback to cached data if Firebase fails
                    LoadCachedData();
                }
            });
        }
        else
        {
            // ⚠️ Fallback to cached data if not authenticated
            LoadCachedData();
        }
    }

    /// <summary>
    /// Load avatar from cache or display the default avatar.
    /// </summary>
    public void UpdateAvatar()
    {
        string equippedAvatar = PlayerPrefs.GetString("EquippedAvatar", "DefaultAvatar");

        if (!string.IsNullOrEmpty(equippedAvatar) && equippedAvatar != "DefaultAvatar")
        {
            Sprite newAvatar = Resources.Load<Sprite>("Avatars/" + equippedAvatar);
            
            if (newAvatar != null)
            {
                avatarImage.sprite = newAvatar;
            }
            else
            {
                avatarImage.sprite = defaultAvatar;  // Fallback
            }
        }
        else
        {
            avatarImage.sprite = defaultAvatar;
        }
    }

    /// <summary>
    /// Displays the equipped avatar.
    /// </summary>
    private void DisplayEquippedAvatar(string avatarName)
    {
        Sprite equippedSprite = Resources.Load<Sprite>("Avatars/" + avatarName);

        if (equippedSprite != null)
        {
            avatarImage.sprite = equippedSprite;
        }
        else
        {
            avatarImage.sprite = defaultAvatar;  // Fallback to default
        }
    }

    /// <summary>
    /// Displays the default avatar.
    /// </summary>
    private void DisplayDefaultAvatar()
    {
        avatarImage.sprite = defaultAvatar;
    }

    /// <summary>
    /// Load online profile picture from URL.
    /// </summary>
    private IEnumerator LoadProfilePicture(string url)
    {
        using (WWW www = new WWW(url))
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                Texture2D texture = www.texture;
                if (texture != null)
                {
                    Rect rect = new Rect(0, 0, texture.width, texture.height);
                    avatarImage.sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
                }
                else
                {
                    UpdateAvatar();  // Load local avatar if texture fails
                }
            }
            else
            {
                UpdateAvatar();  // Use local avatar if URL fails
            }
        }
    }
}
