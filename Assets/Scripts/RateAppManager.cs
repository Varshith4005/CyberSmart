using UnityEngine;
using System.Collections;

public class RateAppManager : MonoBehaviour
{
    public GameObject loadingPanel;

    public void RateApp()
    {
#if UNITY_ANDROID
        string url = "market://details?id=com.yourgame.cybersmart"; // Change package name
#elif UNITY_IOS
        string url = "itms-apps://itunes.apple.com/app/idYOUR_APP_ID"; // Change App Store ID
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
