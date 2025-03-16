using UnityEngine;
using System.Collections;

public class ContactUsManager : MonoBehaviour
{
    public GameObject loadingPanel;

    public void OpenContactUs()
    {
        StartCoroutine(ShowLoadingAndOpenEmail());
    }

    private IEnumerator ShowLoadingAndOpenEmail()
    {
        loadingPanel.SetActive(true);
        yield return new WaitForSeconds(1.5f);

        string email = "support@cybersmart.com";
        string subject = System.Uri.EscapeDataString("Support Request");
        string body = System.Uri.EscapeDataString("Hello, I need help with...");
        string mailto = $"mailto:{email}?subject={subject}&body={body}";

        Application.OpenURL(mailto);
        loadingPanel.SetActive(false);
    }
}
