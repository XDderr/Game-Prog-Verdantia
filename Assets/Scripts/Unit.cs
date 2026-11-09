using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitInfo unitInfo;
    private float unitHealth;
    // public float unitMaxHealth = 100f;
    public HealthTracker healthTracker;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnitSelection.Instance.allUnitsList.Add(gameObject);
        unitHealth = unitInfo.health;
        UpdateHealthUI();
    }

    private void OnDestroy()
    {
        UnitSelection.Instance.allUnitsList.Remove(gameObject);
    }

    public void TakeDamage(int damageToInflict){
        unitHealth -= damageToInflict;
        UpdateHealthUI();
    }

    private void UpdateHealthUI(){
        healthTracker.UpdateSliderValue(unitHealth, unitInfo.health);
        if(unitHealth <= 0){
            //die
            Destroy(gameObject);
        }
    }

    public float GetUnitHealth(){
        return unitHealth;
    }
}
