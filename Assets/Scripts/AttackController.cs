    using UnityEngine;

public class AttackController : MonoBehaviour
{
    private Unit unitInt;
    public Transform targetToAttack;
    Animator animator;

    public Material idleStateMaterial;
    public Material followStateMaterial;
    public Material attackStateMaterial;

    public int unitDamage;

    void Start(){
        unitInt = GetComponent<Unit>();
        unitDamage = unitInt.unitInfo.unitDamage;
        animator = GetComponent<Animator>();
    }
    
    private void OnTriggerEnter(Collider other){
        if(other.CompareTag("Enemy") && targetToAttack == null){
            targetToAttack = other.transform;
        }
    }

    private void OnTriggerExit(Collider other){
        if(other.CompareTag("Enemy") && targetToAttack != null){
            targetToAttack = null;   
            animator.SetBool("isAttacking", false);
        }
    }


    //debug
    public void SetIdleMaterial(){
        GetComponent<Renderer>().material = idleStateMaterial;
    }
    public void SetFollowMaterial(){
        GetComponent<Renderer>().material = followStateMaterial;
    }
    public void SetAttackMaterial(){
        GetComponent<Renderer>().material = attackStateMaterial;
    }

    private void OnDrawGizmos(){
        //follow
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 6.77f * 0.5f);
        
        //attack
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);

        //stop attack distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 1.2f);
    }
    //debug
    
}
