using UnityEngine;
using UnityEngine.UI;

public class ButtonVibration : MonoBehaviour
{
    private Button button; // Reference to the button

    void Start()
    {
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(TriggerVibration);
        }
    }

    void TriggerVibration()
    {
        GameSettingsManager.Vibrate(); // Vibrates if enabled
    }
}
