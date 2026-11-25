using UnityEngine;

public class AttackController : MonoBehaviour
{
    private Unit unitInt;
    public Transform targetToAttack;
    private Animator animator;

    public Material idleStateMaterial;
    public Material followStateMaterial;
    public Material attackStateMaterial;

    public int unitDamage;

    [Header("Attack Timing")]
    [Tooltip("Enable if using a Mixamo Animation Event to call DealDamage(). Disable to use attackCooldown timer.")]
    public bool useAnimationEvent = false;
    public float attackCooldown = 1.2f;
    private float attackTimer;

    void Start()
    {
        unitInt = GetComponent<Unit>();
        if (unitInt != null && unitInt.unitInfo != null)
        {
            unitDamage = unitInt.unitInfo.unitDamage;
        }
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (useAnimationEvent) return;

        // Timer-based damage when the unit is actively in the attacking animation state
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
        if (other.CompareTag("Enemy") && targetToAttack == null)
        {
            targetToAttack = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy") && targetToAttack != null)
        {
            targetToAttack = null;
            if (animator != null) animator.SetBool("isAttacking", false);
        }
    }

    public void DealDamage()
    {
        if (targetToAttack == null)
        {
            Debug.LogWarning($"{gameObject.name}: DealDamage called, but targetToAttack is null.");
            return;
        }

        // Check parent chain first, then children (in case targetToAttack is a root wrapper or child collider)
        EnemyAI targetEnemy = targetToAttack.GetComponentInParent<EnemyAI>();
        if (targetEnemy == null)
        {
            targetEnemy = targetToAttack.GetComponentInChildren<EnemyAI>();
        }

        if (targetEnemy != null)
        {
            targetEnemy.TakeDamage(unitDamage);
            return;
        }

        Unit targetUnit = targetToAttack.GetComponentInParent<Unit>();
        if (targetUnit != null)
        {
            targetUnit.TakeDamage(unitDamage);
            return;
        }

        Debug.LogWarning($"{gameObject.name}: Hit {targetToAttack.name}, but found no EnemyAI or Unit component on it!");
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