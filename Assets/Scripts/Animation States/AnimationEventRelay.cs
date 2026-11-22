using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private AttackController unitAttack;
    private EnemyAI enemyAI;

    private void Awake()
    {
        unitAttack = GetComponentInParent<AttackController>();
        enemyAI = GetComponentInParent<EnemyAI>();
    }

    // Select this function in your Mixamo clip's Animation Event dropdown
    public void TriggerAttackHit()
    {
        if (unitAttack != null) unitAttack.DealDamage();
        if (enemyAI != null) enemyAI.DealDamageToTarget(); // Make DealDamageToTarget() public in EnemyAI.cs
    }
}
