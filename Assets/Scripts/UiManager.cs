using UnityEngine;
using UnityEngine.UI; // Required for Image, Slider, Button, etc.
using TMPro; 

public class UiManager : MonoBehaviour
{
    private UnitSelection unitSelect;
    [SerializeField] private TMP_Text unitHealth;
    [SerializeField] private TMP_Text unitName;

    private void Update()
    {
        UpdateSelectedUnitUI();
    }

    private void UpdateSelectedUnitUI()
    {
        // Check if UnitSelection singleton instance exists and has selected units
        if (UnitSelection.Instance != null && UnitSelection.Instance.unitsSelected.Count > 0)
        {
            // Get the first unit in the unitsSelected list
            GameObject selectedObject = UnitSelection.Instance.unitsSelected[0];

            if (selectedObject != null)
            {
                // Fetch the Unit component on the selected GameObject
                Unit unit = selectedObject.GetComponent<Unit>();

                if (unit != null)
                {
                    // Ensure UI text components are active
                    SetTextVisibility(true);

                    // Get the name from unitInfo ScriptableObject
                    if (unit.unitInfo != null)
                    {
                        unitName.text = unit.unitInfo.unitName;
                    }
                    else
                    {
                        unitName.text = selectedObject.name;
                    }

                    // Get current health from Unit component and max health from UnitInfo
                    if (unit.unitInfo != null)
                    {
                        unitHealth.text = $"HP: {unit.GetUnitHealth()} / {unit.unitInfo.health}";
                    }
                    else
                    {
                        unitHealth.text = $"HP: {unit.GetUnitHealth()}";
                    }

                    return;
                }
            }
        }

        // If no unit is selected or selected object has no Unit component, clear/hide UI text
        ClearUI();
    }

    private void SetTextVisibility(bool isVisible)
    {
        if (unitHealth != null) unitHealth.gameObject.SetActive(isVisible);
        if (unitName != null) unitName.gameObject.SetActive(isVisible);
    }

    private void ClearUI()
    {
        if (unitHealth != null) unitHealth.text = "";
        if (unitName != null) unitName.text = "";
    }
}
