using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Data & References")]
    [SerializeField] private UnitInfo enemyInfo; // Optional: pulls unitSpeed if assigned
    [SerializeField] private LayerMask playerUnitLayer; // Set to the layer your player units are on

    [Header("Animation")]
    [SerializeField] private Animator animator; // Assign the Mixamo model's Animator (auto-detected in children if left empty)
    [SerializeField] private string isMovingParam = "IsMoving"; // Boolean parameter name in your Animator Controller

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private float patrolWaitTime = 2f;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 6f;
    [SerializeField] private float stoppingDistanceFromTarget = 1.5f;

    private NavMeshAgent agent;
    private Vector3 spawnPosition;
    private Transform currentTarget;
    private float waitTimer;
    private bool wasChasing;
    private int isMovingHash;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Automatically find the Animator on the child Mixamo model if not manually assigned
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // Convert parameter name to hash for better performance
        isMovingHash = Animator.StringToHash(isMovingParam);
    }

    private void Start()
    {
        spawnPosition = transform.position;

        if (enemyInfo != null)
        {
            agent.speed = enemyInfo.unitSpeed;
        }

        PickNewPatrolPoint();
    }

    private void Update()
    {
        DetectClosestUnit();

        if (currentTarget != null)
        {
            wasChasing = true;
            ApproachTarget();
        }
        else
        {
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

    private void UpdateAnimation()
    {
        if (animator == null) return;

        // True when the agent has a velocity (patrolling or chasing), false when waiting or stopped at target
        bool isMoving = agent.velocity.sqrMagnitude > 0.05f;
        animator.SetBool(isMovingHash, isMoving);
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
                // Ignore units outside the enemy's patrol radius
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

        currentTarget = closestUnit;
    }

    private void ApproachTarget()
    {
        if (currentTarget == null) return;

        agent.stoppingDistance = stoppingDistanceFromTarget;
        agent.SetDestination(currentTarget.position);

        Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
        directionToTarget.y = 0f;
        if (directionToTarget != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToTarget), Time.deltaTime * 8f);
        }
    }

    private void Patrol()
    {
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? spawnPosition : transform.position;
        Gizmos.DrawWireSphere(center, patrolRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}