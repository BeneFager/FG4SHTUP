using UnityEngine;

/// <summary>
/// Missile that is shot out of missle launcher
/// Adds a force in the forward direction to make it fly
/// and resets it for future use
/// </summary>
public class Missile : MonoBehaviour
{
    public float proplulsionForce = default;
    public int damage = default;
    Rigidbody body;

    private void OnEnable()
    {
        body = this.gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        body.velocity = Vector3.zero;
        body.AddForce(transform.forward * proplulsionForce, ForceMode.Impulse);
    }

    void OnDisable()
    {
        body.velocity = Vector3.zero;
    }

    /// <summary>
    /// Checks if what the missile has hit.
    /// Currently enemies can kill eachother with this implementation
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Missile Collieded with something " + other.name);
        this.gameObject.SetActive(false);
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyStats>().TakeDamage(damage);
        }
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerManager>().TakeDamage(damage);
        }
    }
    
}
