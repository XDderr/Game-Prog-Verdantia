using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour
{
    [SerializeField] private GameObject enemyGrunt;
    private void OnTriggerEnter(Collider other){
        if(other.CompareTag("Unit")){
            Debug.Log("Unit Entry");
            spawnEnemies();
        }
        
    }
    
    void spawnEnemies(){
        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = Quaternion.identity;

        Instantiate(enemyGrunt, spawnPosition, spawnRotation);
    }
}
