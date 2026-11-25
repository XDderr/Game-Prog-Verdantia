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

        if (unitMovement != null && unitMovement.isCommandedtoMove)
        {
            animator.SetBool("isFollowing", false);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isMoving", true);
            return;
        }

        agent.isStopped = false;
        agent.stoppingDistance = attackingDistance * 0.75f;
        agent.SetDestination(attackController.targetToAttack.position);

        Vector3 lookPos = new Vector3(
            attackController.targetToAttack.position.x,
            agent.transform.position.y,
            attackController.targetToAttack.position.z
        );
        agent.transform.LookAt(lookPos);

        float distanceFromTarget = GetHorizontalDistanceToTarget(agent.transform.position, attackController.targetToAttack);

        if (distanceFromTarget <= attackingDistance)
        {
            agent.isStopped = true;
            agent.ResetPath();
            animator.SetBool("isAttacking", true);
        }
    }

    public static float GetHorizontalDistanceToTarget(Vector3 unitPos, Transform target)
    {
        // Prefer root collider first, then child collider, ignoring triggers
        Collider targetCol = target.GetComponent<Collider>();
        if (targetCol == null || targetCol.isTrigger)
        {
            targetCol = target.GetComponentInChildren<Collider>();
        }

        Vector3 targetPoint = target.position;

        // ClosestPoint only supports Box, Sphere, Capsule, or Convex MeshColliders
        if (targetCol != null && !targetCol.isTrigger &&
           (targetCol is BoxCollider || targetCol is SphereCollider || targetCol is CapsuleCollider ||
           (targetCol is MeshCollider meshCol && meshCol.convex)))
        {
            targetPoint = targetCol.ClosestPoint(unitPos);
        }

        // Measure flat XZ distance so tall buildings/trees don't inflate distance via Y height
        Vector2 flatUnit = new Vector2(unitPos.x, unitPos.z);
        Vector2 flatTarget = new Vector2(targetPoint.x, targetPoint.z);
        return Vector2.Distance(flatUnit, flatTarget);
    }
}