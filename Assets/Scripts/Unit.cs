using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnitSelection.Instance.allUnitsList.Add(gameObject);
    }

    private void onDestroy()
    {
        UnitSelection.Instance.allUnitsList.Remove(gameObject);
    }

}
