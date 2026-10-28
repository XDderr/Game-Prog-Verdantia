using UnityEngine;
using System.Collections.Generic;

public class UnitSelection : MonoBehaviour
{

    public static UnitSelection Instance { get; set; }

    //list of gameobjects
    public List<GameObject> allUnitsList = new List<GameObject>();
    public List<GameObject> unitsSelected = new List<GameObject>();
    public List<GameObject> leadersSelected = new List<GameObject>();
    //references the ground and stuff
    public LayerMask clickable;
    public LayerMask ground;
    public LayerMask attackable;
    public GameObject groundMarker;
    public bool attackCursorVisible;

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
        if(unit.CompareTag("Leader")){
            unitsSelected.Remove(unit);
            leadersSelected.Add(unit);
        }
    }

    private void MultiSelect(GameObject unit){
        if(unitsSelected.Contains(unit) == false){
            unitsSelected.Add(unit);
            SelectIndicator(unit, true);
            EnableUnitMovement(unit, true);
        } 
        else{
            EnableUnitMovement(unit, false);
            SelectIndicator(unit, false);
            unitsSelected.Remove(unit);
        }
    }

    private void DeselectAll(){
        foreach(var unit in unitsSelected)
        {
            EnableUnitMovement(unit, false);
            SelectIndicator(unit, false);
        }
        foreach(var unit in leadersSelected)
        {
            EnableUnitMovement(unit, false);
            SelectIndicator(unit, false);
        }
        unitsSelected.Clear();
        leadersSelected.Clear();
        groundMarker.SetActive(false);
    }

    private void EnableUnitMovement(GameObject unit, bool shouldMove){
        unit.GetComponent<UnitMovement>().enabled = shouldMove;
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
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, clickable, QueryTriggerInteraction.Ignore))
            {
                if(Input.GetKey(KeyCode.LeftShift)){
                    MultiSelect(hit.collider.gameObject);
                }
                else{
                    SelectByClick(hit.collider.gameObject);
                }         
                if(hit.collider.CompareTag("Leader")){
                    Debug.Log("Leader Hit!");
                    
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
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, ground, QueryTriggerInteraction.Ignore))
            {
                groundMarker.transform.position = hit.point;
                groundMarker.SetActive(false);
                groundMarker.SetActive(true);
            }
        }
        
        if(unitsSelected.Count > 0 && AtleastOneOffensiveUnit(unitsSelected)){
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, attackable, QueryTriggerInteraction.Ignore))
            {
                Debug.Log("Enemy Hovered with Mouse");
                attackCursorVisible = true;

                if(Input.GetMouseButtonDown(0)){
                    Transform target = hit.transform;
                    foreach(GameObject unit in unitsSelected){
                        if(unit.GetComponent<AttackController>()){
                            unit.GetComponent<AttackController>().targetToAttack = target;
                        }
                    }
                }       
            }
            else{
                attackCursorVisible = false;
            }
        }

        
    }

    private bool AtleastOneOffensiveUnit(List<GameObject> unitsSelected){
        foreach(GameObject unit in unitsSelected){
            if(unit.GetComponent<AttackController>()){
                return true;
            }
        }
        return false;
    }

    
}