using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Collections;

public class SignUpManager : MonoBehaviour
{
    private FirebaseAuth auth;
    private DatabaseReference dbReference;  // Firebase database reference

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
            dbReference = FirebaseDatabase.DefaultInstance.RootReference; // Initialize database reference
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
            FirebaseUser newUser = signUpTask.Result.User;

            if (newUser != null)
            {
                string userId = newUser.UserId;

                // ✅ Set the default profile picture path
                string defaultProfilePicture = "Avatars/default_avatar";  // Replace with your default path

                // ✅ Automatically create user entry in Firebase
                dbReference.Child("users").Child(userId).SetRawJsonValueAsync($@"
                {{
                    ""username"": ""{username}"",
                    ""email"": ""{email}"",
                    ""profilePicture"": ""{defaultProfilePicture}"",
                    ""honorPoints"": 0,
                    ""highestScore"": 0
                }}");

                // Save username and email locally
                PlayerPrefs.SetString("Username", username);
                PlayerPrefs.SetString("Email", email);
                PlayerPrefs.SetString("AvatarPath", defaultProfilePicture);
                PlayerPrefs.Save();

                // Redirect to Home Scene
                SceneManager.LoadScene("HomeScene");
            }
        }
    }
}
