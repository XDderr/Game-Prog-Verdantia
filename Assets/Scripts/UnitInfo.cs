using UnityEngine;

[CreateAssetMenu(fileName = "UnitInfo", menuName = "Scriptable Objects/UnitInfo")]
public class UnitInfo : ScriptableObject
{
    public string unitName;
    public float health;
    public float unitSpeed;
    public int unitDamage;
    public Sprite unitIcon;
}
