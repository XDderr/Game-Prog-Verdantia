using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health;
    internal void RecieveDamage(int damageToInflict){
        health -= damageToInflict;
    }
}
