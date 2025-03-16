using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    private FirebaseAuth auth;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI errorText;
    public GameObject loadingPanel; // Loading animation panel

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        errorText.text = "";
        loadingPanel.SetActive(false);

        // ✅ Check if user is already logged in
        if (PlayerPrefs.GetInt("UserLoggedIn", 0) == 1 && auth.CurrentUser != null)
        {
            SceneManager.LoadScene("HomeScene"); // Skip login and go to Home Scene
        }
    }

    public void Login()
    {
        errorText.text = "";
        StartCoroutine(LoginCoroutine(emailInput.text, passwordInput.text));
    }

    private IEnumerator LoginCoroutine(string email, string password)
    {
        loadingPanel.SetActive(true); // Show loading animation

        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            errorText.text = "Login Failed: " + loginTask.Exception.InnerExceptions[0].Message;
            loadingPanel.SetActive(false); // Hide loading animation on failure
        }
        else
        {
            // ✅ Save login state
            PlayerPrefs.SetInt("UserLoggedIn", 1);
            PlayerPrefs.Save();

            yield return new WaitForSeconds(1.5f); // Simulate loading time
            SceneManager.LoadScene("HomeScene"); // Redirect to Home Scene
        }
    }
}
