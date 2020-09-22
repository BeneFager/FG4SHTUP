using UnityEngine;
using Weapons;

/// <summary>
/// Places on an object to make it interactable and adds it to the players 
/// weapon inventory
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class InteractableObject : MonoBehaviour
{
    private float radius = 1f;
    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided with" + other.gameObject.name);
        if (other.CompareTag("Player"))
        {
            Debug.Log(other.gameObject.name + " has picked me up");
            WeaponManager.instance.AddWeaponToInventory(this);
            gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}