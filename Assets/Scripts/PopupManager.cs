using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    [SerializeField] private TitleManager titleManager;    // Manually assignable reference

    public GameObject popupPanel;                
    public TextMeshProUGUI popupTitle;           
    public TextMeshProUGUI popupDescription;     
    public TextMeshProUGUI popupCost;            
    public TextMeshProUGUI popupTier;            
    public Button yesButton;                     
    public Button noButton;                      

    private string selectedTitle;                
    private int selectedCost;                    

    private void Start()
    {
        if (titleManager == null)
        {
            // Ensure TitleManager reference is assigned
            titleManager = FindObjectOfType<TitleManager>();

            if (titleManager == null)
            {
                Debug.LogError("TitleManager reference is missing!");
            }
        }

        // Assign button events
        yesButton.onClick.AddListener(ConfirmPurchase);
        noButton.onClick.AddListener(HidePopup);
    }

    public void ShowPopup(string title, string description, int cost, string tier)
    {
        selectedTitle = title;
        selectedCost = cost;

        popupTitle.text = title;
        popupDescription.text = description;
        popupCost.text = $"{cost} honor points";
        popupTier.text = tier;

        popupPanel.SetActive(true);
    }

    public void ConfirmPurchase()
    {
        if (titleManager != null)
        {
            titleManager.ConfirmPurchase();
            HidePopup();
        }
        else
        {
            Debug.LogError("TitleManager reference is missing or title is null!");
        }
    }

    public void HidePopup()
    {
        popupPanel.SetActive(false);
    }
}
