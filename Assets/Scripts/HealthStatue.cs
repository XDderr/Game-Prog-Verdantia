using System.Collections.Generic;
using UnityEngine;

public class HealingStatue : MonoBehaviour
{
    [Header("Healing Settings")]
    [SerializeField] private float healAmount = 10f;
    [SerializeField] private float healInterval = 1f;

    private List<Unit> unitsInRange = new List<Unit>();
    private float healTimer;

    private void Update()
    {
        if (unitsInRange.Count == 0) return;

        healTimer += Time.deltaTime;
        if (healTimer >= healInterval)
        {
            healTimer = 0f;
            HealUnitsInRange();
        }
    }

    private void HealUnitsInRange()
    {
        // Iterate backwards in case a unit is destroyed or removed mid-loop
        for (int i = unitsInRange.Count - 1; i >= 0; i--)
        {
            if (unitsInRange[i] == null)
            {
                unitsInRange.RemoveAt(i);
                continue;
            }

            Unit unitToHeal = unitsInRange[i];
            unitToHeal.Heal(healAmount);

            // Grab the unit's name from UnitInfo, or default to the GameObject name
            string healedUnitName = (unitToHeal.unitInfo != null && !string.IsNullOrEmpty(unitToHeal.unitInfo.unitName))
                ? unitToHeal.unitInfo.unitName
                : unitToHeal.gameObject.name;

            Debug.Log($"Healing unit: {healedUnitName}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Unit unit = other.GetComponent<Unit>();
        if (unit != null && !unitsInRange.Contains(unit))
        {
            unitsInRange.Add(unit);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Unit unit = other.GetComponent<Unit>();
        if (unit != null && unitsInRange.Contains(unit))
        {
            unitsInRange.Remove(unit);
        }
    }
}