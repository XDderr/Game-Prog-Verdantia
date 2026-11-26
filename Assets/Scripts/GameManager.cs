using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string gameOverScene;
    public List<GameObject> soldierUnits = new List<GameObject>();
    public UnitSelection unitSelection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start(){
        unitSelection = GameObject.Find("SelectionManager").GetComponent<UnitSelection>();
    }
    
    private void Update(){
        if(unitSelection.allUnitsList.Count == 0){
            LoadSceneByName(gameOverScene);
        }
    }

    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
