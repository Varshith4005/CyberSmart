using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Auth;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private GameObject[] titleCells;
    [SerializeField] private GameObject popup;
    [SerializeField] private TextMeshProUGUI popupTier;
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private TextMeshProUGUI errorText;     // Error message display
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private GameObject loadingPanel;        // Loading indicator

    private DatabaseReference dbReference;
    private FirebaseAuth auth;
    private string userId;

    private int currentTitleIndex;
    private HashSet<int> purchasedTitles = new HashSet<int>();

    private string equippedTitle = "";

    private string[] tiers = { "Uncommon", "Uncommon", "Uncommon", "Uncommon", "Uncommon",
                               "Common", "Common", "Common", "Common", "Common",
                               "Rare", "Rare", "Rare", "Rare", "Rare",
                               "Epic", "Epic", "Epic",
                               "Legendary", "Legendary",
                               "Mythic" };

    private int[] honorCosts = { 0, 150, 200, 250, 300, 350, 400, 450, 500, 550,
                                 600, 650, 700, 750, 800, 1000, 1200, 1400,
                                 2000, 2500, 3000 };

    private void Start()
    {
        InitializeFirebase();
        StartCoroutine(LoadDataAndUpdateUI());
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        if (auth.CurrentUser != null)
        {
            userId = auth.CurrentUser.UserId;
        }
        else
        {
            Debug.LogError("User not authenticated.");
        }
    }

    // Coroutine to load Firebase data before updating the UI
    private IEnumerator LoadDataAndUpdateUI()
    {
        loadingPanel.SetActive(true);

        yield return LoadPurchasedTitles();
        yield return LoadEquippedTitle();
        yield return LoadHonorPoints();

        UpdateUI();
        loadingPanel.SetActive(false);
    }

    private IEnumerator LoadPurchasedTitles()
    {
        var task = dbReference.Child("users").Child(userId).Child("purchasedTitles").GetValueAsync();

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to load purchased titles: {task.Exception}");
            yield break;
        }

        if (task.Result.Exists)
        {
            purchasedTitles.Clear();
            foreach (var child in task.Result.Children)
            {
                if (int.TryParse(child.Key, out int titleIndex))
                {
                    purchasedTitles.Add(titleIndex);
                }
            }
        }
    }

    private IEnumerator LoadEquippedTitle()
    {
        var task = dbReference.Child("users").Child(userId).Child("equippedTitle").GetValueAsync();

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to load equipped title: {task.Exception}");
            yield break;
        }

        if (task.Result.Exists && !string.IsNullOrEmpty(task.Result.Value.ToString()))
        {
            equippedTitle = task.Result.Value.ToString();
            PlayerPrefs.SetString("EquippedTitle", equippedTitle);
        }
        else
        {
            equippedTitle = "";
            PlayerPrefs.SetString("EquippedTitle", "No title equipped");
        }
    }

    private IEnumerator LoadHonorPoints()
    {
        var task = dbReference.Child("users").Child(userId).Child("honorPoints").GetValueAsync();

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Failed to load honor points: {task.Exception}");
            yield break;
        }

        if (task.Result.Exists)
        {
            int firebaseHonor = int.Parse(task.Result.Value.ToString());
            PlayerPrefs.SetInt("HonorPoints", firebaseHonor);  // Sync local honor with Firebase
        }
    }

    private void UpdateUI()
    {
        bool anyTitleEquipped = false;  // Flag to track equipped title

        for (int i = 0; i < titleCells.Length; i++)
        {
            if (titleCells[i] == null)
            {
                Debug.LogWarning($"Title cell at index {i} is null.");
                continue;
            }

            Button button = titleCells[i].transform.Find("Redeem")?.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogWarning($"Button missing in cell {i}");
                continue;
            }

            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText == null)
            {
                Debug.LogWarning($"Button text missing in cell {i}");
                continue;
            }

            if (purchasedTitles.Contains(i))
            {
                string titleName = titleCells[i].transform.Find("TitleName")?.GetComponent<TextMeshProUGUI>()?.text;

                if (titleName == equippedTitle)
                {
                    buttonText.text = "Equipped";
                    buttonText.color = Color.yellow;

                    int validIndex = i;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => StartCoroutine(UnequipTitle(validIndex)));

                    anyTitleEquipped = true;  // Title equipped
                }
                else
                {
                    buttonText.text = "Equip";
                    buttonText.color = Color.black;

                    int validIndex = i;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => StartCoroutine(EquipTitle(validIndex)));
                }
            }
            else
            {
                buttonText.text = "Redeem";
                buttonText.color = Color.black;

                int validIndex = i;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ShowPopup(validIndex));
            }
        }

        // ✅ Display "No title equipped" if none are equipped
        if (!anyTitleEquipped)
        {
            equippedTitle = "No title equipped";
            PlayerPrefs.SetString("EquippedTitle", equippedTitle);
            PlayerPrefs.Save();
        }
    }

    public void ShowPopup(int titleIndex)
    {
        currentTitleIndex = titleIndex;

        string tier = GetTierFromIndex(titleIndex);
        int cost = honorCosts[titleIndex];

        popupTier.text = tier;
        popupText.text = $"Do you want to buy this title for {cost} honor points?";

        errorText.gameObject.SetActive(false);
        popup.SetActive(true);

        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(() => ConfirmPurchase());

        noButton.onClick.RemoveAllListeners();
        noButton.onClick.AddListener(() => popup.SetActive(false));
    }

    public void ConfirmPurchase()
    {
        StartCoroutine(PurchaseTitleCoroutine());
    }

    private IEnumerator PurchaseTitleCoroutine()
{
    loadingPanel.SetActive(true);  // Show loading indicator

    // Load the latest honor points from Firebase
    var task = dbReference.Child("users").Child(userId).Child("honorPoints").GetValueAsync();
    yield return new WaitUntil(() => task.IsCompleted);

    if (task.Exception != null)
    {
        Debug.LogError($"Failed to load honor points: {task.Exception}");
        loadingPanel.SetActive(false);
        yield break;
    }

    int currentHonor = task.Result.Exists ? int.Parse(task.Result.Value.ToString()) : 0;
    int cost = honorCosts[currentTitleIndex];

    if (currentHonor >= cost)
    {
        // ✅ Successful purchase: deduct honor points
        currentHonor -= cost;

        // ✅ Update Firebase and PlayerPrefs
        PlayerPrefs.SetInt("HonorPoints", currentHonor);
        PlayerPrefs.Save();
        yield return dbReference.Child("users").Child(userId).Child("honorPoints").SetValueAsync(currentHonor);

        // ✅ Add the purchased title
        purchasedTitles.Add(currentTitleIndex);
        yield return dbReference.Child("users").Child(userId).Child("purchasedTitles")
            .Child(currentTitleIndex.ToString()).SetValueAsync(true);

        // ✅ Close the popup on successful purchase
        popup.SetActive(false);  
    }
    else
    {
        // ❌ Insufficient honor points: show error text
        errorText.text = "Insufficient honor points!";
        errorText.color = Color.red;
        errorText.gameObject.SetActive(true);

        // ✅ Add a small delay to ensure the error message is displayed properly
        yield return new WaitForSeconds(0.1f);
    }

    loadingPanel.SetActive(false);
    UpdateUI();
}



    private IEnumerator EquipTitle(int titleIndex)
    {
        equippedTitle = titleCells[titleIndex].transform.Find("TitleName")?.GetComponent<TextMeshProUGUI>()?.text;

        yield return dbReference.Child("users").Child(userId).Child("equippedTitle").SetValueAsync(equippedTitle);

        PlayerPrefs.SetString("EquippedTitle", equippedTitle);
        PlayerPrefs.Save();
        UpdateUI();
    }

    private IEnumerator UnequipTitle(int titleIndex)
    {
        equippedTitle = "";

        yield return dbReference.Child("users").Child(userId).Child("equippedTitle").SetValueAsync("");

        PlayerPrefs.SetString("EquippedTitle", "No title equipped");
        PlayerPrefs.Save();
        UpdateUI();
    }

    private string GetTierFromIndex(int index)
    {
        return (index >= 0 && index < tiers.Length) ? tiers[index] : "Unknown";
    }
}
