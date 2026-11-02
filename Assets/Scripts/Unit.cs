using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public UnitInfo unitInfo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnitSelection.Instance.allUnitsList.Add(gameObject);
        Debug.Log($"Unit Name: {unitInfo.unitName}");
    }

    private void OnDestroy()
    {
        UnitSelection.Instance.allUnitsList.Remove(gameObject);
    }
}
