using Interfaces;
using UnityEngine;

/// <summary>
/// Poorly named
/// Handles damage and health of the enemies
/// </summary>
public class EnemyStats : MonoBehaviour, IDamagable
{
    public float health = 10f;
    
    public void Die()
    {
        Debug.Log("I am dead " + this.gameObject.name);
        this.gameObject.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("taking " + damage + " damage");
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }
}
