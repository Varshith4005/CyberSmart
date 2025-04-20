using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class NoButtonHandler : MonoBehaviour
{
    public GameObject loadingPanel;

    public void CancelPasswordPopup()
    {
        StartCoroutine(CancelPasswordCoroutine());
    }

    private IEnumerator CancelPasswordCoroutine()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true); // Show loading screen
            yield return new WaitForSeconds(1.5f); // Simulate loading delay
        }
        SceneManager.LoadScene("SettingsScene"); // Redirect back to SettingsScene
    }
}
