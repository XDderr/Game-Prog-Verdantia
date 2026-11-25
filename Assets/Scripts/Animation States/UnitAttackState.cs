using UnityEngine;
using UnityEngine.AI;

public class UnitAttackState : StateMachineBehaviour
{
    NavMeshAgent agent;
    AttackController attackController;
    UnitMovement unitMovement;

    public float stopAttackingDistance = 3.0f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponentInParent<NavMeshAgent>();
        attackController = animator.GetComponentInParent<AttackController>();
        unitMovement = animator.GetComponentInParent<UnitMovement>();

        if (attackController != null)
        {
            attackController.SetAttackMaterial();
        }

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
            animator.SetBool("isFollowing", false);
            return;
        }

        if (unitMovement != null && unitMovement.isCommandedtoMove)
        {
            animator.SetBool("isAttacking", false);
            animator.SetBool("isFollowing", false);
            animator.SetBool("isMoving", true);
            return;
        }

        LookAtTarget();

        float distanceFromTarget = UnitFollowState.GetHorizontalDistanceToTarget(agent.transform.position, attackController.targetToAttack);
        if (distanceFromTarget > stopAttackingDistance)
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
        if (agent != null)
        {
            agent.isStopped = false;
        }
    }
}