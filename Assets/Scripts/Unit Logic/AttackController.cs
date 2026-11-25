using UnityEngine;

public class AttackController : MonoBehaviour
{
    private Unit unitInt;
    private UnitMovement unitMovement;
    public Transform targetToAttack;
    private Animator animator;

    public Material idleStateMaterial;
    public Material followStateMaterial;
    public Material attackStateMaterial;

    public int unitDamage;

    [Header("Attack Timing")]
    [Tooltip("Enable if using a Mixamo Animation Event to call DealDamage(). Disable to use attackCooldown timer.")]
    public bool useAnimationEvent = true;
    public float attackCooldown = 1.2f;
    private float attackTimer;

    void Start()
    {
        unitInt = GetComponent<Unit>();
        unitMovement = GetComponent<UnitMovement>();

        if (unitInt != null && unitInt.unitInfo != null)
        {
            unitDamage = unitInt.unitInfo.unitDamage;
        }
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // 1. Always check if the current target died or was destroyed (even when using Animation Events)
        if (targetToAttack != null)
        {
            EnemyAI enemy = targetToAttack.GetComponentInParent<EnemyAI>();
            if (enemy != null && enemy.GetCurrentHealth() <= 0f)
            {
                ClearTarget();
                return;
            }

            WorldRootHeart heart = targetToAttack.GetComponentInParent<WorldRootHeart>();
            if (heart == null) heart = targetToAttack.GetComponentInChildren<WorldRootHeart>();
            if (heart != null && heart.GetCurrentHealth() <= 0f)
            {
                ClearTarget();
                return;
            }
        }
        else if (animator != null && (animator.GetBool("isAttacking") || animator.GetBool("isFollowing")))
        {
            ClearTarget();
            return;
        }

        // 2. Stop here if Animation Events handle DealDamage()
        if (useAnimationEvent) return;

        // 3. Fallback timer-based damage when Animation Events are disabled
        if (animator != null && animator.GetBool("isAttacking") && targetToAttack != null)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                DealDamage();
            }
        }
        else
        {
            attackTimer = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (unitMovement != null && unitMovement.isCommandedtoMove) return;

        if (other.CompareTag("Enemy") && targetToAttack == null)
        {
            targetToAttack = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy") && targetToAttack != null && other.transform == targetToAttack)
        {
            ClearTarget();
        }
    }

    public void DealDamage()
    {
        if (targetToAttack == null) return;

        // 1. Check EnemyAI
        EnemyAI targetEnemy = targetToAttack.GetComponentInParent<EnemyAI>();
        if (targetEnemy == null) targetEnemy = targetToAttack.GetComponentInChildren<EnemyAI>();

        if (targetEnemy != null)
        {
            targetEnemy.TakeDamage(unitDamage);
            if (targetEnemy.GetCurrentHealth() <= 0f) ClearTarget();
            return;
        }

        // 2. Check WorldRootHeart
        WorldRootHeart targetHeart = targetToAttack.GetComponentInParent<WorldRootHeart>();
        if (targetHeart == null) targetHeart = targetToAttack.GetComponentInChildren<WorldRootHeart>();

        if (targetHeart != null)
        {
            targetHeart.TakeDamage(unitDamage);
            if (targetHeart.GetCurrentHealth() <= 0f) ClearTarget();
            return;
        }

        // 3. Check Unit
        Unit targetUnit = targetToAttack.GetComponentInParent<Unit>();
        if (targetUnit != null)
        {
            targetUnit.TakeDamage(unitDamage);
            if (targetUnit.GetUnitHealth() <= 0f) ClearTarget();
            return;
        }

        Debug.LogWarning($"{gameObject.name}: Attacked {targetToAttack.name}, but no EnemyAI, WorldRootHeart, or Unit component was found!");
    }

    public void ClearTarget()
    {
        targetToAttack = null;
        attackTimer = 0f;
        if (animator != null)
        {
            animator.SetBool("isAttacking", false);
            animator.SetBool("isFollowing", false);
        }
    }

    public void CancelAttackForMove()
    {
        targetToAttack = null;
        attackTimer = 0f;
        if (animator != null)
        {
            animator.SetBool("isAttacking", false);
            animator.SetBool("isFollowing", false);
            animator.SetBool("isMoving", true);
        }
    }

    public void SetIdleMaterial()
    {
        if (TryGetComponent<Renderer>(out var rend)) rend.material = idleStateMaterial;
    }
    public void SetFollowMaterial()
    {
        if (TryGetComponent<Renderer>(out var rend)) rend.material = followStateMaterial;
    }
    public void SetAttackMaterial()
    {
        if (TryGetComponent<Renderer>(out var rend)) rend.material = attackStateMaterial;
    }
}