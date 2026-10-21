using UnityEngine;
using System.Collections.Generic;

public class UnitSelection : MonoBehaviour
{

    public static UnitSelection Instance { get; set; }

    //list of gameobjects
    public List<GameObject> allUnitsList = new List<GameObject>();
    public List<GameObject> unitsSelected = new List<GameObject>();
    //references the ground and stuff
    public LayerMask clickable;
    public LayerMask ground;
    public GameObject groundMarker;

    private Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake(){
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        cam = Camera.main;
    }

    private void SelectByClick(GameObject unit){
        DeselectAll();
        unitsSelected.Add(unit);
        EnableUnitMovement(unit, true);
        SelectIndicator(unit, true);
    }
    private void DeselectAll(){
        foreach(var unit in unitsSelected)
        {
            EnableUnitMovement(unit, false);
            SelectIndicator(unit, false);
        }
        unitsSelected.Clear();
        groundMarker.SetActive(false);
    }

    private void EnableUnitMovement(GameObject unit, bool shouldMove){
        unit.GetComponent<UnitMovement>().enabled = shouldMove;
    }

    private void MultiSelect(GameObject unit){
        if(unitsSelected.Contains(unit) == false){
            unitsSelected.Add(unit);
            SelectIndicator(unit, true);
            EnableUnitMovement(unit, true);
        }else{
            EnableUnitMovement(unit, false);
            SelectIndicator(unit, false);
            unitsSelected.Remove(unit);
        }
    }

    private void SelectIndicator(GameObject unit, bool isVisible){
        unit.transform.GetChild(0).gameObject.SetActive(isVisible);
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(2))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, clickable))
            {
                if(Input.GetKey(KeyCode.LeftShift)){
                    MultiSelect(hit.collider.gameObject);
                }
                else{
                    SelectByClick(hit.collider.gameObject);
                }         
            }
        }

        if(Input.GetMouseButtonDown(1) && unitsSelected.Count >= 1){
                DeselectAll();
                 Debug.Log("Mouse 2 Pressed");
            }

        if(Input.GetMouseButtonDown(0) && unitsSelected.Count > 0)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
            {
                groundMarker.transform.position = hit.point;
                groundMarker.SetActive(false);
                groundMarker.SetActive(true);
            }
        }
    }

    
}