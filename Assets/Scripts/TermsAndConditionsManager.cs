using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TermsAndConditionsManager : MonoBehaviour
{
    public GameObject loadingPanel;

    public void OpenTermsAndConditions()
    {
        StartCoroutine(ShowLoadingAndLoadScene());
    }

    private IEnumerator ShowLoadingAndLoadScene()
    {
        loadingPanel.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("TermsAndConditions");
    }
}
