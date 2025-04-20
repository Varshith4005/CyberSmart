using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using Firebase;
using Firebase.Auth;

public class ConfirmPasswordHandler : MonoBehaviour
{
    public GameObject loadingPanel;
    public TMP_InputField passwordInputField; // TMP Input Field for password input
    public TMP_Text errorText; // TMP Text to display errors
    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        if (loadingPanel == null)
            Debug.LogError("❌ Error: loadingPanel is not assigned in the Inspector!");

        if (passwordInputField == null)
            Debug.LogError("❌ Error: passwordInputField is not assigned in the Inspector!");

        if (errorText == null)
            Debug.LogError("❌ Error: errorText is not assigned in the Inspector!");

        loadingPanel?.SetActive(false);
        errorText.text = "";
    }

    public void ConfirmDeletion()
    {
        if (passwordInputField == null)
        {
            Debug.LogError("❌ Error: Password Input Field is NULL.");
            return;
        }

        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(password))
        {
            errorText.text = "⚠️ Password cannot be empty.";
            return;
        }

        StartCoroutine(DeleteAccountCoroutine(password));
    }

    private IEnumerator DeleteAccountCoroutine(string password)
    {
        if (auth == null)
        {
            Debug.LogError("❌ FirebaseAuth is null!");
            yield break;
        }

        FirebaseUser user = auth.CurrentUser;
        if (user == null)
        {
            errorText.text = "⚠️ No user found.";
            Debug.LogError("❌ Error: No user found.");
            yield break;
        }

        Debug.Log("✅ User found: " + user.Email);

        loadingPanel?.SetActive(true);
        yield return new WaitForSeconds(1.5f);

        // 🔹 Reauthenticate the user
        Credential credential = EmailAuthProvider.GetCredential(user.Email, password);
        var reauthTask = user.ReauthenticateAsync(credential);
        yield return new WaitUntil(() => reauthTask.IsCompleted);

        if (reauthTask.Exception != null)
        {
            FirebaseException firebaseEx = reauthTask.Exception.GetBaseException() as FirebaseException;
            if (firebaseEx != null)
            {
                Debug.LogError("🔥 Firebase Error Code: " + firebaseEx.ErrorCode);
                Debug.LogError("📌 Firebase Error Message: " + firebaseEx.Message);

                if (firebaseEx.ErrorCode == 17009) // ERROR_WRONG_PASSWORD
                {
                    Debug.LogError("❌ Incorrect password entered.");
                    errorText.text = "Incorrect password. Please try again.";
                }
                else
                {
                    errorText.text = "Incorrect password. Please try again.";
                }
            }
            else
            {
                Debug.LogError("❌ Reauthentication failed: " + reauthTask.Exception);
                errorText.text = "⚠️ Reauthentication error. Please try again.";
            }

            loadingPanel?.SetActive(false);
            yield break;
        }

        Debug.Log("✅ Reauthentication successful. Proceeding to delete account...");

        // 🔹 Delete the user account
        var deleteTask = user.DeleteAsync();
        yield return new WaitUntil(() => deleteTask.IsCompleted);

        if (deleteTask.Exception != null)
        {
            errorText.text = "⚠️ Failed to delete account.";
            Debug.LogError("❌ Account deletion failed: " + deleteTask.Exception);
            loadingPanel?.SetActive(false);
            yield break;
        }

        Debug.Log("✅ Account deleted successfully. Redirecting to login scene...");

        PlayerPrefs.SetInt("UserLoggedIn", 0);
        PlayerPrefs.Save();

        loadingPanel?.SetActive(false);
        SceneManager.LoadScene("LoginScene");
    }
}
