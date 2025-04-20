using UnityEngine;
using System.Collections;

public class RateAppManager : MonoBehaviour
{
    public GameObject loadingPanel;

    public void RateApp()
    {
        string url = ""; // Declare URL outside preprocessor directives

#if UNITY_ANDROID
        url = "market://details?id=com.yourgame.cybersmart"; // Change package name
#elif UNITY_IOS
        url = "itms-apps://itunes.apple.com/app/idYOUR_APP_ID"; // Change App Store ID
#else
        Debug.LogWarning("RateApp feature is not supported on this platform.");
        return; // Exit function if unsupported
#endif

        StartCoroutine(ShowLoadingAndOpenURL(url));
    }

    private IEnumerator ShowLoadingAndOpenURL(string url)
    {
        loadingPanel.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        Application.OpenURL(url);
        loadingPanel.SetActive(false);
    }
}
