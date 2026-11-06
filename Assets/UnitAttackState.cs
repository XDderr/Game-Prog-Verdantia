using UnityEngine;
using UnityEngine.AI;

public class UnitAttackState : StateMachineBehaviour
{
    NavMeshAgent agent;
    AttackController attackController;

    public float stopAttackingDistance = 1.2f;
    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       agent = animator.GetComponent<NavMeshAgent>();
       attackController = animator.GetComponent<AttackController>();
       attackController.SetAttackMaterial();
    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       if(attackController.targetToAttack != null && animator.transform.GetComponent<UnitMovement>().isCommandedtoMove == false){
            LookAtTarget();
            //move towards enemy
            agent.SetDestination(attackController.targetToAttack.position);
            Attack();

            //should unit still attack
            float distanceFromTarget = Vector3.Distance(attackController.targetToAttack.position, animator.transform.position);
            if(distanceFromTarget > stopAttackingDistance || attackController.targetToAttack == null){
                animator.SetBool("isAttacking", false);
            }
       }
    }   

    private void LookAtTarget(){
        Vector3 direction = attackController.targetToAttack.position - agent.transform.position;
        agent.transform.rotation = Quaternion.LookRotation(direction);

        var yRotation = agent.transform.eulerAngles.y;
        agent.transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void Attack(){
        var damageToInflict = attackController.unitDamage;
        attackController.targetToAttack.GetComponent<Unit>().TakeDamage(damageToInflict);
    }

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       
    }
}
