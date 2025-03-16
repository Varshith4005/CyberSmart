using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using System.Collections;

public class ConfirmPasswordHandler : MonoBehaviour
{
    public GameObject passwordPopup;
    public GameObject loadingPanel;
    public InputField passwordInput;
    
    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        loadingPanel.SetActive(false);
    }

    public void ConfirmDeletion()
    {
        StartCoroutine(DeleteAccountCoroutine());
    }

    private IEnumerator DeleteAccountCoroutine()
    {
        loadingPanel.SetActive(true);

        FirebaseUser user = auth.CurrentUser;
        string password = passwordInput.text;
        string email = user.Email;
        
        Credential credential = EmailAuthProvider.GetCredential(email, password);
        
        // Re-authenticate the user
        var reauthTask = user.ReauthenticateAsync(credential);
        yield return new WaitUntil(() => reauthTask.IsCompleted);

        if (reauthTask.Exception != null)
        {
            Debug.LogError("Reauthentication failed: " + reauthTask.Exception);
            loadingPanel.SetActive(false);
            yield break;
        }

        // Delete the account
        var deleteTask = user.DeleteAsync();
        yield return new WaitUntil(() => deleteTask.IsCompleted);

        if (deleteTask.Exception != null)
        {
            Debug.LogError("Account deletion failed: " + deleteTask.Exception);
        }
        else
        {
            Debug.Log("Account deleted successfully");
            SceneManager.LoadScene("LoginScene");
        }

        loadingPanel.SetActive(false);
    }
}
