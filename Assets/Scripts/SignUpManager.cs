using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using System.Collections;

public class SignUpManager : MonoBehaviour
{
    private FirebaseAuth auth;

    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public TextMeshProUGUI errorText; // Shows error messages only

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
        });

        errorText.text = ""; // Clear error message on start
    }

    public void SignUp()
    {
        errorText.text = ""; // Clear previous errors

        if (passwordInput.text != confirmPasswordInput.text)
        {
            errorText.text = "Passwords do not match!";
            return;
        }

        StartCoroutine(SignUpCoroutine(emailInput.text, passwordInput.text, usernameInput.text));
    }

    private IEnumerator SignUpCoroutine(string email, string password, string username)
    {
        var signUpTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => signUpTask.IsCompleted);

        if (signUpTask.Exception != null)
        {
            errorText.text = "Sign Up Failed: " + signUpTask.Exception.InnerExceptions[0].Message;
        }
        else
        {
            PlayerPrefs.SetString("Username", username);
            PlayerPrefs.Save();
            SceneManager.LoadScene("HomeScene"); // Redirect on success
        }
    }
}
