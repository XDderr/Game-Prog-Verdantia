using UnityEngine;
using UnityEngine.AI;

public class UnitAttackState : StateMachineBehaviour
{
    NavMeshAgent agent;
    AttackController attackController;
    UnitMovement unitMovement;

    public float stopAttackingDistance = 1.2f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponentInParent<NavMeshAgent>();
        attackController = animator.GetComponentInParent<AttackController>();
        unitMovement = animator.GetComponentInParent<UnitMovement>();

        if (attackController != null)
        {
            attackController.SetAttackMaterial();
        }

        // Stop the unit in place while playing the attack animation
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (attackController == null || attackController.targetToAttack == null)
        {
            animator.SetBool("isAttacking", false);
            return;
        }

        if (unitMovement != null && !unitMovement.isCommandedtoMove)
        {
            LookAtTarget();

            float distanceFromTarget = Vector3.Distance(attackController.targetToAttack.position, agent.transform.position);
            if (distanceFromTarget > stopAttackingDistance)
            {
                animator.SetBool("isAttacking", false);
            }
        }
        else if (unitMovement != null && unitMovement.isCommandedtoMove)
        {
            animator.SetBool("isAttacking", false);
        }
    }

    private void LookAtTarget()
    {
        Vector3 direction = attackController.targetToAttack.position - agent.transform.position;
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            agent.transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Resume movement capability when leaving the attack state
        if (agent != null)
        {
            agent.isStopped = false;
        }
    }
}