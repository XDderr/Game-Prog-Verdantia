using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Data & References")]
    [SerializeField] private UnitInfo enemyInfo; // Pulls unitSpeed and unitDamage
    [SerializeField] private LayerMask playerUnitLayer;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isMovingParam = "IsMoving";
    [SerializeField] private string isAttackingParam = "isAttacking"; // Matches AttackController

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private float patrolWaitTime = 2f;

    [Header("Detection & Combat Distances")]
    [SerializeField] private float detectionRadius = 6f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float stopAttackDistance = 2.0f; // Buffer distance before breaking off attack

    [Header("Attack Settings")]
    [SerializeField] private int fallbackDamage = 10;
    [SerializeField] private float attackCooldown = 1.5f;

    public Transform targetToAttack; // Mirrors AttackController.targetToAttack

    private NavMeshAgent agent;
    private Vector3 spawnPosition;
    private float waitTimer;
    private float attackTimer;
    private bool wasChasing;
    private bool isAttacking;
    private int enemyDamage;

    private int isMovingHash;
    private int isAttackingHash;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        isMovingHash = Animator.StringToHash(isMovingParam);
        isAttackingHash = Animator.StringToHash(isAttackingParam);
    }

    private void Start()
    {
        spawnPosition = transform.position;

        if (enemyInfo != null)
        {
            agent.speed = enemyInfo.unitSpeed;
            enemyDamage = enemyInfo.unitDamage;
        }
        else
        {
            enemyDamage = fallbackDamage;
        }

        PickNewPatrolPoint();
    }

    private void Update()
    {
        // Clear reference if the target unit died and was destroyed
        if (targetToAttack == null)
        {
            isAttacking = false;
            DetectClosestUnit();
        }
        else
        {
            // Verify the target is still inside the enemy's patrol territory
            float targetDistFromSpawnSqr = (targetToAttack.position - spawnPosition).sqrMagnitude;
            if (targetDistFromSpawnSqr > patrolRadius * patrolRadius)
            {
                targetToAttack = null;
                isAttacking = false;
            }
        }

        if (targetToAttack != null)
        {
            wasChasing = true;
            HandleCombatAndApproach();
        }
        else
        {
            isAttacking = false;

            if (wasChasing)
            {
                wasChasing = false;
                waitTimer = 0f;
                PickNewPatrolPoint();
            }

            Patrol();
        }

        UpdateAnimation();
    }

    private void DetectClosestUnit()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerUnitLayer, QueryTriggerInteraction.Ignore);

        Transform closestUnit = null;
        float closestDistanceSqr = Mathf.Infinity;
        float patrolRadiusSqr = patrolRadius * patrolRadius;

        foreach (Collider hit in hits)
        {
            Unit unit = hit.GetComponent<Unit>();
            if (unit != null)
            {
                float distFromSpawnSqr = (hit.transform.position - spawnPosition).sqrMagnitude;
                if (distFromSpawnSqr > patrolRadiusSqr)
                {
                    continue;
                }

                float distSqr = (hit.transform.position - transform.position).sqrMagnitude;
                if (distSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distSqr;
                    closestUnit = hit.transform;
                }
            }
        }

        targetToAttack = closestUnit;
    }

    private void HandleCombatAndApproach()
    {
        if (targetToAttack == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, targetToAttack.position);

        // Smoothly rotate toward the target whether approaching or attacking
        Vector3 directionToTarget = (targetToAttack.position - transform.position).normalized;
        directionToTarget.y = 0f;
        if (directionToTarget != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToTarget), Time.deltaTime * 8f);
        }

        // Hysteresis check: enter attack at attackRange, exit attack only if target moves past stopAttackDistance
        if (!isAttacking && distanceToTarget <= attackRange)
        {
            isAttacking = true;
        }
        else if (isAttacking && distanceToTarget > stopAttackDistance)
        {
            isAttacking = false;
        }

        if (isAttacking)
        {
            // Stop moving while swinging/attacking
            agent.isStopped = true;
            agent.ResetPath();

            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                DealDamageToTarget();
            }
        }
        else
        {
            // Resume pursuing the target
            agent.isStopped = false;
            agent.stoppingDistance = attackRange * 0.9f;
            agent.SetDestination(targetToAttack.position);
        }
    }

    public void DealDamageToTarget()
    {
        if (targetToAttack == null) return;

        Unit targetUnit = targetToAttack.GetComponent<Unit>();
        if (targetUnit != null)
        {
            targetUnit.TakeDamage(enemyDamage);
        }
    }

    private void Patrol()
    {
        agent.isStopped = false;
        agent.stoppingDistance = 0f;

        if (!agent.pathPending && agent.remainingDistance <= 0.25f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolWaitTime)
            {
                waitTimer = 0f;
                PickNewPatrolPoint();
            }
        }
    }

    private void PickNewPatrolPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        Vector3 randomDirection = spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        bool isMoving = !isAttacking && agent.velocity.sqrMagnitude > 0.05f;
        animator.SetBool(isMovingHash, isMoving);
        animator.SetBool(isAttackingHash, isAttacking);
    }

    private void OnDrawGizmosSelected()
    {
        // Yellow = Patrol Territory
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? spawnPosition : transform.position;
        Gizmos.DrawWireSphere(center, patrolRadius);

        // Cyan = Detection Radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Red = Attack Range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Blue = Stop Attack Distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopAttackDistance);
    }
}