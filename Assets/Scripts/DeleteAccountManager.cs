using UnityEngine;

public class DeleteAccountManager : MonoBehaviour
{
    public GameObject confirmationPopup;

    void Start()
    {
        confirmationPopup.SetActive(false);
    }

    public void ShowConfirmationPopup()
    {
        confirmationPopup.SetActive(true);
    }
}
