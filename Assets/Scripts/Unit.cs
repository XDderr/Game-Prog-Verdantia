using UnityEngine;

public class Unit : MonoBehaviour
{
    private int unitHealth;
    public int unitMaxHealth = 100;
    public UnitInfo unitInfo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unitHealth = unitMaxHealth;
        UnitSelection.Instance.allUnitsList.Add(gameObject);
        Debug.Log($"Unit Name: {unitInfo.unitName}");
    }

    private void OnDestroy()
    {
        UnitSelection.Instance.allUnitsList.Remove(gameObject);
    }

    public void TakeDamage(int damageToInflict){
        unitHealth -= damageToInflict;
        if(unitHealth == 0){
            Destroy(gameObject);
        }
    }
}
