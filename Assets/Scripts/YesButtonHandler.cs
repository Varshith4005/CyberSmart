using UnityEngine;

public class YesButtonHandler : MonoBehaviour
{
    public GameObject confirmationPopup;
    public GameObject passwordPopup;

    void Start()
    {
        passwordPopup.SetActive(false);
    }

    public void ShowPasswordPopup()
    {
        confirmationPopup.SetActive(false); // Hide confirmation popup
        passwordPopup.SetActive(true); // Show password popup
    }
}
