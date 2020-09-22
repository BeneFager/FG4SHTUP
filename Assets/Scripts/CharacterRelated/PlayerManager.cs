using UnityEngine;
using Interfaces;
using UnityEditor;

/// <summary>
/// Handles the players hp and what to do with it
/// Singleton for easy access to player object
/// </summary>
public class PlayerManager : MonoBehaviour, IDamagable
{
    #region Singleton

    public static PlayerManager instance;

    private void Awake()
    {
        instance = this;
    }
    #endregion

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("Player died");
        EditorApplication.ExitPlaymode();
        
    }

    public GameObject player;

    [SerializeField]
    float hp = 100f;
}
