using UnityEngine;

public class AttackController : MonoBehaviour
{
    public Transform targetToAttack;
    
    private void onTriggerEnter(Collider other){
        if(other.CompareTag("Enemy") && targetToAttack == null){
            targetToAttack = other.transform;
        }
    }

    private void onTriggerExit(Collider other){
        if(other.CompareTag("Enemy") && targetToAttack != null){
            targetToAttack = null;   
        }
    }
}
