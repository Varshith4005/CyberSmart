using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Extensions;
using System.Linq;

public class LeaderBoardManager : MonoBehaviour
{
    public Transform scrollViewContent;  // Reference to the ScrollView Content holding the panels

    private List<Transform> playerPanels = new List<Transform>();

    void Awake()
    {
        // Clear cache when the scene loads
        Resources.UnloadUnusedAssets();
    }

    void Start()
    {
        LoadLeaderboard();
    }

    private void LoadLeaderboard()
    {
        Debug.Log("✅ Fetching fresh leaderboard data...");

        // Force Firebase to sync fresh data
        FirebaseDatabase.DefaultInstance.GetReference("users").KeepSynced(false);
        FirebaseDatabase.DefaultInstance.GetReference("users").KeepSynced(true);

        Query query = FirebaseDatabase.DefaultInstance.RootReference.Child("users").OrderByChild("username");

        query.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("❌ Failed to fetch leaderboard data.");
                return;
            }

            DataSnapshot snapshot = task.Result;
            List<PlayerData> players = new List<PlayerData>();

            foreach (var user in snapshot.Children)
            {
                string username = user.Child("username").Value?.ToString() ?? "Unknown";
                string title = user.Child("equippedTitle").Value?.ToString() ?? "No Title";
                int honor = int.Parse(user.Child("honorPoints").Value?.ToString() ?? "0");
                string avatarPath = user.Child("equippedAvatar").Value?.ToString() ?? "Avatars/default_avatar";

                players.Add(new PlayerData(username, title, honor, avatarPath));
                Debug.Log($"✅ Fetched: {username} | {honor} points | Avatar: {avatarPath}");
            }

            // Sort by honor points (descending) and take the top 20 players
            players = players.OrderByDescending(p => p.honorPoints).Take(20).ToList();

            Debug.Log($"✅ Total players fetched: {players.Count}");

            // Display players in the UI panels
            DisplayLeaderboard(players);
        });
    }

    private void DisplayLeaderboard(List<PlayerData> players)
    {
        // Cache the player panels only once
        if (playerPanels.Count == 0)
        {
            for (int i = 0; i < scrollViewContent.childCount; i++)
            {
                playerPanels.Add(scrollViewContent.GetChild(i));
            }
        }

        for (int i = 0; i < playerPanels.Count; i++)
        {
            if (i < players.Count)
            {
                playerPanels[i].gameObject.SetActive(true);
                UpdatePlayerPanel(playerPanels[i], players[i], i + 1);
            }
            else
            {
                playerPanels[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdatePlayerPanel(Transform panel, PlayerData player, int rank)
    {
        bool isTop3 = rank <= 3;

        // Access UI elements in the panel
        Transform userPanel = panel.GetChild(0); 
        Text usernameText = userPanel.GetChild(0).GetComponent<Text>();
        Text titleText = userPanel.GetChild(1).GetComponent<Text>();
        Text honorText = userPanel.GetChild(2).GetComponent<Text>();
        Image avatarImage = panel.GetChild(1).GetComponent<Image>();

        if (isTop3)
        {
            // Top 3 → Rank is displayed as an image
            Image rankImage = panel.GetChild(2).GetComponent<Image>();
            
            rankImage.gameObject.SetActive(true);
            usernameText.text = player.username;
            titleText.text = player.title;
            honorText.text = $"{player.honorPoints} pts";

            // Load avatar image with cache busting
            StartCoroutine(LoadAvatar(player.avatarPath, avatarImage));
        }
        else
        {
            // Rank is displayed as text for players ranked 4th to 20th
            Text rankText = panel.GetChild(2).GetComponent<Text>();
            
            rankText.gameObject.SetActive(true);
            rankText.text = rank.ToString();
            usernameText.text = player.username;
            titleText.text = player.title;
            honorText.text = $"{player.honorPoints} pts";

            // Load avatar image with cache busting
            StartCoroutine(LoadAvatar(player.avatarPath, avatarImage));
        }
    }

    private IEnumerator LoadAvatar(string avatarPath, Image avatarImage)
    {
        // Force cache busting by adding a random suffix
        string cacheBusterPath = $"{avatarPath}?v={Random.Range(1000, 9999)}";

        // Clear cache before loading
        Resources.UnloadUnusedAssets();
        yield return null;

        // Load the avatar
        ResourceRequest request = Resources.LoadAsync<Sprite>(avatarPath);
        yield return request;

        if (request.asset != null)
        {
            avatarImage.sprite = request.asset as Sprite;
            Debug.Log($"✅ Avatar loaded: {cacheBusterPath}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Avatar not found: {avatarPath}. Using default.");
            avatarImage.sprite = Resources.Load<Sprite>("Avatars/default_avatar");
        }
    }

    public class PlayerData
    {
        public string username;
        public string title;
        public int honorPoints;
        public string avatarPath;

        public PlayerData(string username, string title, int honorPoints, string avatarPath)
        {
            this.username = username;
            this.title = title;
            this.honorPoints = honorPoints;
            this.avatarPath = avatarPath;
        }
    }
}
