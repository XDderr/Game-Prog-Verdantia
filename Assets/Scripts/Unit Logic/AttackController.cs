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

    void Start()
    {
        unitInt = GetComponent<Unit>();
        unitDamage = unitInt.unitInfo.unitDamage;
        animator = GetComponentInChildren<Animator>(); // Finds Animator on child Mixamo model
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

    // Called by the Animation Event on the exact impact frame of the attack clip
    public void DealDamage()
    {
        if (targetToAttack == null) return;

        Unit targetUnit = targetToAttack.GetComponent<Unit>();
        if (targetUnit != null)
        {
            targetUnit.TakeDamage(unitDamage);
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