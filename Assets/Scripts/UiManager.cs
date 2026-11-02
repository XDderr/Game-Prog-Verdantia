using UnityEngine;
using TMPro;

public class UiManager : MonoBehaviour
{
    UnitSelection unitSelection;
    TextMeshProUGUI unitText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unitSelection = GameObject.Find("SelectionManager").GetComponent<UnitSelection>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach(GameObject unit in unitSelection.unitsSelected){
            // unitText.text = unit;
        }
    }
}
