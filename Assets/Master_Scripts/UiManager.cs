using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class UiManager : MonoBehaviour
{
    [Header("Primary Unit UI")]
    [SerializeField] private TMP_Text unitHealth;
    [SerializeField] private TMP_Text unitName;
    [SerializeField] private Image primaryUnitIcon; // Standard UI Image component

    [Header("Multi-Selection Panel")]
    [SerializeField] private Transform iconContainer; // Assign the white box RectTransform
    [SerializeField] private GameObject iconPrefab;   // UI Prefab containing an Image component

    // Cache instantiated icons to avoid constant Instantiate/Destroy overhead
    private List<GameObject> iconPool = new List<GameObject>();

    private void Update()
    {
        UpdateSelectedUnitUI();
    }

    private void UpdateSelectedUnitUI()
    {
        // Check selection list from UnitSelection singleton
        if (UnitSelection.Instance != null && UnitSelection.Instance.unitsSelected.Count > 0)
        {
            List<GameObject> selectedUnits = UnitSelection.Instance.unitsSelected;

            // 1. Process the First Unit (Displays name, HP, and primary portrait Image)
            GameObject primaryObject = selectedUnits[0];
            if (primaryObject != null)
            {
                Unit primaryUnit = primaryObject.GetComponent<Unit>();
                if (primaryUnit != null)
                {
                    SetTextVisibility(true);

                    // Name
                    if (primaryUnit.unitInfo != null)
                        unitName.text = primaryUnit.unitInfo.unitName;
                    else
                        unitName.text = primaryObject.name;

                    // Health
                    if (primaryUnit.unitInfo != null)
                        unitHealth.text = $"{primaryUnit.GetUnitHealth()} / {primaryUnit.unitInfo.health}";
                    else
                        unitHealth.text = $"{primaryUnit.GetUnitHealth()}";

                    // Main Left Portrait Image
                    if (primaryUnitIcon != null)
                    {
                        if (primaryUnit.unitInfo != null && primaryUnit.unitInfo.unitIcon != null)
                        {
                            primaryUnitIcon.gameObject.SetActive(true);
                            primaryUnitIcon.sprite = primaryUnit.unitInfo.unitIcon;
                        }
                        else
                        {
                            primaryUnitIcon.gameObject.SetActive(false);
                        }
                    }
                }
            }

            // 2. Populate the Multi-Selection Box ONLY for subsequent units (Index 1 onwards)
            UpdateMultiSelectIcons(selectedUnits);
        }
        else
        {
            ClearUI();
        }
    }

    private void UpdateMultiSelectIcons(List<GameObject> selectedUnits)
    {
        // Number of secondary units (excluding the first unit shown in the primary panel)
        int secondaryCount = Mathf.Max(0, selectedUnits.Count - 1);

        // Expand pool if more icon objects are needed
        while (iconPool.Count < secondaryCount)
        {
            GameObject newIconObj = Instantiate(iconPrefab, iconContainer);
            iconPool.Add(newIconObj);
        }

        // Configure icons in the pool
        for (int i = 0; i < iconPool.Count; i++)
        {
            if (i < secondaryCount)
            {
                // Index offset by +1 to skip the primary unit
                GameObject currentUnitObj = selectedUnits[i + 1]; 
                iconPool[i].SetActive(true);

                Image img = iconPool[i].GetComponent<Image>();
                if (img != null && currentUnitObj != null)
                {
                    Unit u = currentUnitObj.GetComponent<Unit>();
                    if (u != null && u.unitInfo != null)
                    {
                        img.sprite = u.unitInfo.unitIcon;
                    }
                }
            }
            else
            {
                // Disable excess icons in the pool
                iconPool[i].SetActive(false);
            }
        }
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
        if (primaryUnitIcon != null) primaryUnitIcon.gameObject.SetActive(false);

        // Hide all multi-select unit icons
        foreach (GameObject icon in iconPool)
        {
            icon.SetActive(false);
        }
    }
}