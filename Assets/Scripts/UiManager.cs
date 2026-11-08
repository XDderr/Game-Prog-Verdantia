using UnityEngine;
using UnityEngine.UI; // Required for Image, Slider, Button, etc.
using TMPro; 

public class UiManager : MonoBehaviour
{
    private UnitSelection unitSelect;
    [SerializeField] private TMP_Text Health;
    [SerializeField] private Button healthButton;
    [SerializeField] private Slider volumeSlider;

    void Start()
    {
        unitSelect = GetComponent<UnitSelection>();
        // Accessing and modifying properties directly
        Health.text = "HP: 100/100";
        healthButton.onClick.AddListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        Debug.Log("Button Clicked!");
    }
}
