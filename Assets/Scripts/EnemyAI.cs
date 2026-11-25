using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Data & References")]
    [SerializeField] private UnitInfo enemyInfo; // Pulls health, unitSpeed, and unitDamage
    [SerializeField] private LayerMask playerUnitLayer;
    [SerializeField] private HealthTracker healthTracker; // Optional: assign if enemy has a world-space health bar

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isMovingParam = "IsMoving";
    [SerializeField] private string isAttackingParam = "isAttacking";

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private float patrolWaitTime = 2f;

    [Header("Detection & Combat Distances")]
    [SerializeField] private float detectionRadius = 6f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float stopAttackDistance = 2.0f;

    [Header("Health & Attack Settings")]
    [SerializeField] private float fallbackMaxHealth = 100f;
    [SerializeField] private int fallbackDamage = 10;
    [SerializeField] private float attackCooldown = 1.5f;

    public Transform targetToAttack;

    private NavMeshAgent agent;
    private Vector3 spawnPosition;
    private float waitTimer;
    private float attackTimer;
    private bool wasChasing;
    private bool isAttacking;

    private float maxHealth;
    private float currentHealth;
    private int enemyDamage;

    private int isMovingHash;
    private int isAttackingHash;

    [Header("Death Settings")]
    [SerializeField] private string dieTriggerParam = "Die"; // Or set a Bool "isDead" if your Animator uses a bool
    [SerializeField] private float deathDestroyDelay = 3f; // Match the length of your Mixamo death clip
    private bool isDead = false;
    private int dieTriggerHash;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        isMovingHash = Animator.StringToHash(isMovingParam);
        isAttackingHash = Animator.StringToHash(isAttackingParam);
        dieTriggerHash = Animator.StringToHash(dieTriggerParam);
    }

    private void Start()
    {
        spawnPosition = transform.position;

        if (enemyInfo != null)
        {
            agent.speed = enemyInfo.unitSpeed;
            enemyDamage = enemyInfo.unitDamage;
            maxHealth = enemyInfo.health;
        }
        else
        {
            enemyDamage = fallbackDamage;
            maxHealth = fallbackMaxHealth;
        }

        currentHealth = maxHealth;
        UpdateHealthUI();

        PickNewPatrolPoint();
    }

    private void Update()
    {
        if (isDead) return;
        if (targetToAttack == null)
        {
            isAttacking = false;
            DetectClosestUnit();
        }
        else
        {
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
            Unit unit = hit.GetComponentInParent<Unit>();
            if (unit != null)
            {
                float distFromSpawnSqr = (unit.transform.position - spawnPosition).sqrMagnitude;
                if (distFromSpawnSqr > patrolRadiusSqr)
                {
                    continue;
                }

                float distSqr = (unit.transform.position - transform.position).sqrMagnitude;
                if (distSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distSqr;
                    closestUnit = unit.transform;
                }
            }
        }

        targetToAttack = closestUnit;
    }

    private void HandleCombatAndApproach()
    {
        if (targetToAttack == null) return;

        Collider targetCollider = targetToAttack.GetComponentInChildren<Collider>();
        Vector3 closestPoint = (targetCollider != null)
            ? targetCollider.ClosestPoint(transform.position)
            : targetToAttack.position;

        float distanceToTarget = Vector3.Distance(closestPoint, transform.position);

        Vector3 directionToTarget = (targetToAttack.position - transform.position).normalized;
        directionToTarget.y = 0f;
        if (directionToTarget != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToTarget), Time.deltaTime * 8f);
        }

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
            agent.isStopped = false;
            agent.stoppingDistance = attackRange * 0.8f;
            agent.SetDestination(targetToAttack.position);
        }
    }

    // Deals damage to the targeted player Unit (can also be called via Animation Event)
    public void DealDamageToTarget()
    {
        if (targetToAttack == null) return;

        Unit targetUnit = targetToAttack.GetComponentInParent<Unit>();
        if (targetUnit != null)
        {
            targetUnit.TakeDamage(enemyDamage);
        }
    }

    // Receives damage from player Units and logs remaining health
    public void TakeDamage(float damageToInflict)
    {
        if (isDead) return;

        currentHealth -= damageToInflict;
        currentHealth = Mathf.Max(0f, currentHealth);

        string enemyName = (enemyInfo != null && !string.IsNullOrEmpty(enemyInfo.unitName))
            ? enemyInfo.unitName
            : gameObject.name;

        Debug.Log($"{enemyName} took {damageToInflict} damage! Health remaining: {currentHealth}/{maxHealth}");

        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthTracker != null)
        {
            healthTracker.UpdateSliderValue(currentHealth, maxHealth);
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        targetToAttack = null;
        isAttacking = false;

        // 1. Halt NavMeshAgent movement immediately
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        // 2. Disable colliders & remove from 'attackable' layer so player units stop targeting the corpse
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // 3. Hide world-space health bar if assigned
        if (healthTracker != null)
        {
            healthTracker.gameObject.SetActive(false);
        }

        // 4. Play death animation and schedule cleanup
        if (animator != null)
        {
            animator.SetBool(isMovingHash, false);
            animator.SetBool(isAttackingHash, false);
            animator.SetTrigger(dieTriggerHash);
        }

        Destroy(gameObject, deathDestroyDelay);
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
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
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? spawnPosition : transform.position;
        Gizmos.DrawWireSphere(center, patrolRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopAttackDistance);
    }
}