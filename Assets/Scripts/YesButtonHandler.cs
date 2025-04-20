using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using Firebase.Auth;

public class YesButtonHandler : MonoBehaviour
{
    public GameObject loadingPanel;
    public GameObject passwordPopup;
    public TMP_InputField passwordInputField; // TMP Input Field for password input
    public TMP_Text errorText; // TMP Text to display errors
    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        loadingPanel.SetActive(false);
        passwordPopup.SetActive(false);
        errorText.text = "";
    }

    public void ShowPasswordPopup()
    {
        passwordPopup.SetActive(true);
    }

    public void ConfirmPasswordAndDeleteAccount()
    {
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(password))
        {
            errorText.text = "Password cannot be empty.";
            return;
        }

        StartCoroutine(DeleteAccountCoroutine(password));
    }

    private IEnumerator DeleteAccountCoroutine(string password)
    {
        loadingPanel.SetActive(true);
        errorText.text = "";
        yield return new WaitForSeconds(1.5f); // Simulate loading time

        FirebaseUser user = auth.CurrentUser;
        if (user == null)
        {
            errorText.text = "No user found.";
            Debug.LogError("Error: No user found.");
            loadingPanel.SetActive(false);
            yield break;
        }

        Debug.Log("User found: " + user.Email);

        // Reauthenticate the user with password
        Credential credential = EmailAuthProvider.GetCredential(user.Email, password);
        var reauthTask = user.ReauthenticateAsync(credential);
        yield return new WaitUntil(() => reauthTask.IsCompleted);

        if (reauthTask.Exception != null)
        {
            errorText.text = "Incorrect password. Please try again.";
            Debug.LogError("Reauthentication failed: " + reauthTask.Exception);
            loadingPanel.SetActive(false);
            yield break;
        }

        Debug.Log("Reauthentication successful. Proceeding to delete account...");

        // Proceed with account deletion
        var deleteTask = user.DeleteAsync();
        yield return new WaitUntil(() => deleteTask.IsCompleted);

        if (deleteTask.Exception != null)
        {
            errorText.text = "Failed to delete account.";
            Debug.LogError("Account deletion failed: " + deleteTask.Exception);
            loadingPanel.SetActive(false);
            yield break;
        }

        Debug.Log("Account deleted successfully. Redirecting to login scene...");

        PlayerPrefs.SetInt("UserLoggedIn", 0); // Clear login state
        PlayerPrefs.Save();

        SceneManager.LoadScene("LoginScene"); // Redirect to login page
    }
}
