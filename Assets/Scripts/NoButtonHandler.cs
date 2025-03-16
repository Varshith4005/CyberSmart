using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class NoButtonHandler : MonoBehaviour
{
    public GameObject confirmationPopup;
    public GameObject loadingPanel;

    void Start()
    {
        loadingPanel.SetActive(false);
    }

    public void CancelDeleteAccount()
    {
        StartCoroutine(CancelDeletionCoroutine());
    }

    private IEnumerator CancelDeletionCoroutine()
    {
        confirmationPopup.SetActive(false); // Hide confirmation popup
        loadingPanel.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        
        SceneManager.LoadScene("SettingsScene");
    }
}
