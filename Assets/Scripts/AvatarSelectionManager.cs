using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.SceneManagement;
using System.Collections;

public class AvatarSelectionManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image avatarPreview;                  // Preview image (top circle)
    public Button equipButton;                   // Equip/Unequip button
    public GameObject loadingScreen;             // Loading screen
    public Sprite defaultAvatar;                 // Default avatar sprite

    private Sprite selectedAvatar;               // Currently selected avatar sprite
    private string equippedAvatarPath;           // Path of the equipped avatar
    private DatabaseReference dbReference;       // Firebase DB reference
    private bool isEquipped = false;             // Equip state

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;

                // ✅ Load the equipped avatar on scene start
                StartCoroutine(LoadEquippedAvatar());
            }
        });

        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(ToggleEquip);
    }

    /// <summary>
    /// Load equipped avatar from Firebase or PlayerPrefs and set the correct state.
    /// </summary>
    private IEnumerator LoadEquippedAvatar()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;

        if (user != null)
        {
            string userId = user.UserId;

            // First, load from Firebase
            var task = dbReference.Child("users").Child(userId).Child("equippedAvatar").GetValueAsync();

            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsCompleted && task.Result.Exists)
            {
                string avatarName = task.Result.Value.ToString();

                // ✅ Sync with PlayerPrefs
                PlayerPrefs.SetString("EquippedAvatar", avatarName);
                PlayerPrefs.Save();

                equippedAvatarPath = avatarName;

                if (!string.IsNullOrEmpty(avatarName) && avatarName != "DefaultAvatar")
                {
                    Sprite equippedSprite = Resources.Load<Sprite>("Avatars/" + avatarName);

                    if (equippedSprite != null)
                    {
                        avatarPreview.sprite = equippedSprite;
                        equipButton.GetComponentInChildren<TMP_Text>().text = "Unequip";
                        equipButton.gameObject.SetActive(true);
                        isEquipped = true;
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
            }
            else
            {
                DisplayDefaultAvatar();
            }
        }
    }

    /// <summary>
    /// Display the default avatar.
    /// </summary>
    private void DisplayDefaultAvatar()
    {
        avatarPreview.sprite = defaultAvatar;
        equipButton.GetComponentInChildren<TMP_Text>().text = "Equip";
        equipButton.gameObject.SetActive(true);
        isEquipped = false;
    }

    /// <summary>
    /// Toggle between Equip and Unequip states.
    /// </summary>
    private void ToggleEquip()
    {
        // Show loading screen
        StartCoroutine(ShowLoadingScreen());

        if (isEquipped)
        {
            StartCoroutine(UnequipAvatar());
        }
        else
        {
            StartCoroutine(EquipAvatar());
        }
    }

    /// <summary>
    /// Equip the selected avatar and save to Firebase.
    /// </summary>
    private IEnumerator EquipAvatar()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;

        if (user != null && selectedAvatar != null)
        {
            string userId = user.UserId;
            string avatarName = selectedAvatar.name;

            var task = dbReference.Child("users").Child(userId).Child("equippedAvatar").SetValueAsync(avatarName);

            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsCompleted)
            {
                // ✅ Save to Firebase and PlayerPrefs
                equippedAvatarPath = avatarName;
                PlayerPrefs.SetString("EquippedAvatar", avatarName);
                PlayerPrefs.Save();

                // ✅ Update UI
                avatarPreview.sprite = selectedAvatar;
                equipButton.GetComponentInChildren<TMP_Text>().text = "Unequip";
                equipButton.gameObject.SetActive(true);
                isEquipped = true;

                // ✅ Redirect to Home Scene
                StartCoroutine(RedirectToHomeScene());
                Debug.Log($"Equipped avatar: {avatarName}");
            }
            else
            {
                Debug.LogError("Failed to equip avatar.");
            }
        }
    }

    /// <summary>
    /// Unequip the current avatar and reset to default.
    /// </summary>
    private IEnumerator UnequipAvatar()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;

        if (user != null)
        {
            string userId = user.UserId;

            var task = dbReference.Child("users").Child(userId).Child("equippedAvatar").SetValueAsync("DefaultAvatar");

            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsCompleted)
            {
                // ✅ Save default avatar to Firebase and PlayerPrefs
                equippedAvatarPath = "DefaultAvatar";
                PlayerPrefs.SetString("EquippedAvatar", "DefaultAvatar");
                PlayerPrefs.Save();

                // ✅ Reset UI
                DisplayDefaultAvatar();
                isEquipped = false;

                // ✅ Redirect to Home Scene
                StartCoroutine(RedirectToHomeScene());

                Debug.Log("Unequipped avatar.");
            }
            else
            {
                Debug.LogError("Failed to unequip avatar.");
            }
        }
    }

    /// <summary>
    /// Select an avatar from the scroll view.
    /// </summary>
    public void SelectAvatar(Sprite avatarSprite)
    {
        selectedAvatar = avatarSprite;

        if (selectedAvatar != null)
        {
            avatarPreview.sprite = selectedAvatar;
            equipButton.gameObject.SetActive(true);

            // ✅ Show "Equip" or "Unequip" based on state
            if (selectedAvatar.name == equippedAvatarPath)
            {
                equipButton.GetComponentInChildren<TMP_Text>().text = "Unequip";
                isEquipped = true;
            }
            else
            {
                equipButton.GetComponentInChildren<TMP_Text>().text = "Equip";
                isEquipped = false;
            }
        }
    }

    /// <summary>
    /// Display the loading screen during avatar changes.
    /// </summary>
    private IEnumerator ShowLoadingScreen()
    {
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        loadingScreen.SetActive(false);
    }

    /// <summary>
    /// Redirect to the home scene with updated avatar.
    /// </summary>
    private IEnumerator RedirectToHomeScene()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("HomeScene");
    }
}
