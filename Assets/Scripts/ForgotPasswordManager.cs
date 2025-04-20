using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using System.Collections;

public class ForgotPasswordManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TextMeshProUGUI messageText;     // To show success or error message
    public GameObject loadingPanel;         // Loading animation panel

    private FirebaseAuth auth;

    void Start()
    {
        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
            }
            else
            {
                Debug.LogError($"Firebase Dependency Error: {task.Result}");
                messageText.text = "Failed to initialize Firebase!";
            }
        });

        messageText.text = ""; 
        loadingPanel.SetActive(false);
    }

    public void SendResetLink()
    {
        string email = emailInput.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            messageText.text = "Please enter your email!";
            messageText.color = Color.red;
            return;
        }

        StartCoroutine(SendPasswordResetCoroutine(email));
    }

    private IEnumerator SendPasswordResetCoroutine(string email)
    {
        loadingPanel.SetActive(true);     // Show loading animation
        messageText.text = "";            // Clear previous messages

        var resetTask = auth.SendPasswordResetEmailAsync(email);
        yield return new WaitUntil(() => resetTask.IsCompleted);

        loadingPanel.SetActive(false);    // Hide loading animation

        if (resetTask.Exception != null)
        {
            // Handle different errors
            FirebaseException firebaseEx = resetTask.Exception.GetBaseException() as FirebaseException;

            if (firebaseEx != null)
            {
                switch ((AuthError)firebaseEx.ErrorCode)
                {
                    case AuthError.InvalidEmail:
                        messageText.text = "Invalid email format!";
                        break;
                    case AuthError.MissingEmail:
                        messageText.text = "Email field is empty!";
                        break;
                    case AuthError.UserNotFound:
                        messageText.text = "No account with this email!";
                        break;
                    default:
                        messageText.text = "Failed to send reset email!";
                        break;
                }
            }
            else
            {
                messageText.text = "Unknown error occurred!";
            }

            messageText.color = Color.red;
        }
        else
        {
            messageText.text = "Reset link sent successfully! Check your email.";
            messageText.color = Color.green;
        }
    }

    public void GoBack()
    {
        SceneManager.LoadScene("LoginScene");  // Navigate back to login scene
    }
}
