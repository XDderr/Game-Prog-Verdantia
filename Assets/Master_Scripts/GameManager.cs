using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public List<GameObject> soldierUnits = new List<GameObject>();
    public UnitSelection unitSelection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start(){
        unitSelection = GameObject.Find("SelectionManager").GetComponent<UnitSelection>();
    }
    
    private void Update(){
        foreach(GameObject unit in unitSelection.leadersSelected){
            Debug.Log("Game Manager: Leader in List");
        }
    }
}
