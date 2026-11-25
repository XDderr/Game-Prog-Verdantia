using UnityEngine;
using UnityEngine.AI;

public class UnitFollowState : StateMachineBehaviour
{
    AttackController attackController;
    NavMeshAgent agent;
    UnitMovement unitMovement;

    public float attackingDistance = 2.0f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        attackController = animator.GetComponentInParent<AttackController>();
        agent = animator.GetComponentInParent<NavMeshAgent>();
        unitMovement = animator.GetComponentInParent<UnitMovement>();

        if (attackController != null)
        {
            attackController.SetFollowMaterial();
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (attackController == null || attackController.targetToAttack == null)
        {
            animator.SetBool("isFollowing", false);
            animator.SetBool("isAttacking", false);
            return;
        }

        // Clear any lingering move command while actively pursuing a target
        if (unitMovement != null && unitMovement.isCommandedtoMove)
        {
            unitMovement.isCommandedtoMove = false;
        }

        agent.isStopped = false;
        agent.stoppingDistance = attackingDistance * 0.8f;
        agent.SetDestination(attackController.targetToAttack.position);

        Vector3 lookPos = new Vector3(
            attackController.targetToAttack.position.x,
            agent.transform.position.y,
            attackController.targetToAttack.position.z
        );
        agent.transform.LookAt(lookPos);

        Collider targetCollider = attackController.targetToAttack.GetComponentInChildren<Collider>();
        Vector3 closestPoint = (targetCollider != null)
            ? targetCollider.ClosestPoint(agent.transform.position)
            : attackController.targetToAttack.position;

        float distanceFromTarget = Vector3.Distance(closestPoint, agent.transform.position);

        if (distanceFromTarget <= attackingDistance)
        {
            agent.isStopped = true;
            agent.ResetPath();
            animator.SetBool("isAttacking", true);
        }
    }
}