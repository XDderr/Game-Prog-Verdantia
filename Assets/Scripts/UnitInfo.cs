using UnityEngine;

[CreateAssetMenu(fileName = "UnitInfo", menuName = "Scriptable Objects/UnitInfo")]
public class UnitInfo : ScriptableObject
{
    public string unitName;
    public float unitSpeed;
    public float unitDamage;
    public Sprite unitIcon;
}
