using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitInfo unitInfo;
    private float unitHealth;
    // public float unitMaxHealth = 100f;
    public HealthTracker healthTracker;

    [Header("Death Settings")]
    [SerializeField] private string dieTriggerParam = "Die";
    [SerializeField] private float deathDestroyDelay = 3f;
    private bool isDead = false;
    private Animator animator;  
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        UnitSelection.Instance.allUnitsList.Add(gameObject);
        unitHealth = unitInfo.health;
        UpdateHealthUI();
    }

    private void OnDestroy()
    {
        if (UnitSelection.Instance != null)
        {
            UnitSelection.Instance.allUnitsList.Remove(gameObject);
            UnitSelection.Instance.unitsSelected.Remove(gameObject);
            UnitSelection.Instance.leadersSelected.Remove(gameObject);
        }
    }

    public void TakeDamage(int damageToInflict){
        if (isDead) return;

        unitHealth -= damageToInflict;
        UpdateHealthUI();
    }

    private void UpdateHealthUI(){
        if (healthTracker != null)
        {
            healthTracker.UpdateSliderValue(unitHealth, unitInfo.health);
        }

        if (unitHealth <= 0)
        {
            Die();
        }
    }

    public float GetUnitHealth(){
        return unitHealth;
    }

    public void Heal(float healAmount)
    {
        if (unitHealth < unitInfo.health)
        {
            unitHealth = Mathf.Min(unitHealth + healAmount, unitInfo.health);
            UpdateHealthUI();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. Immediately remove from selection lists & UI so player can't command a dying unit
        if (UnitSelection.Instance != null)
        {
            UnitSelection.Instance.allUnitsList.Remove(gameObject);
            UnitSelection.Instance.unitsSelected.Remove(gameObject);
            UnitSelection.Instance.leadersSelected.Remove(gameObject);
        }

        // 2. Disable movement, combat, and selection indicator
        if (TryGetComponent<UnitMovement>(out var movement)) movement.enabled = false;
        if (TryGetComponent<AttackController>(out var attackCtrl))
        {
            attackCtrl.ClearTarget();
            attackCtrl.enabled = false;
        }

        if (TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent) && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        // 3. Disable colliders so enemies immediately switch to another living target
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        if (healthTracker != null)
        {
            healthTracker.gameObject.SetActive(false);
        }

        // 4. Play death animation and destroy after clip finishes
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetBool("isFollowing", false);
            animator.SetBool("isAttacking", false);
            animator.SetTrigger(dieTriggerParam);
        }

        Destroy(gameObject, deathDestroyDelay);
    }
}
