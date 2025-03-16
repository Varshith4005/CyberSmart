using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using System.Collections;

public class LogoutManager : MonoBehaviour
{
    private FirebaseAuth auth;
    public GameObject loadingPanel;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    public void LogOut()
    {
        StartCoroutine(LogOutCoroutine());
    }

    private IEnumerator LogOutCoroutine()
    {
        loadingPanel.SetActive(true);
        yield return new WaitForSeconds(1.5f);

        auth.SignOut();
        PlayerPrefs.SetInt("UserLoggedIn", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene("LoginScene");
    }
}
