using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// Movement based on navmesh not exactly what i had in mind.
/// </summary>
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] Transform firePoint = default;
    public float attackRange = 10f;
    float nextTimeToFire = 0f;
    float fireRate = 2f;
    Transform target;
    NavMeshAgent agent;
    Rigidbody body;
    ObjectPooler objectpooler;

    void Start()
    {
        target = PlayerManager.instance.player.transform;
        agent = GetComponent<NavMeshAgent>();
        body = GetComponent<Rigidbody>();
        objectpooler = ObjectPooler.instance;
    }

    /// <summary>
    /// If the npc is in range it attacks and dodges
    /// </summary>
    void Update()
    {
        float distance = Vector3.Distance(target.position, transform.position);

        if (distance > attackRange)
        {
            agent.SetDestination(target.position);
            
        }
        else if (ShouldDodge())
        {
            Dodge();
            body.velocity = Vector3.zero;
        }
        else
        {
            if (Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1 / fireRate;
                Attack();
            }
        }
        FaceTarget();
    }

    void Dodge()
    {
        body.AddForce(transform.right * 5, ForceMode.Impulse);
    }

    void Attack()
    {
            Debug.Log("Enemy shoot missile");
            objectpooler.SpawnFromPool("Missile", firePoint.position, firePoint.rotation);
    }

    /// <summary>
    /// This is not good
    /// </summary>
    /// <returns></returns>
    bool ShouldDodge()
    {
        int randomNmr = Random.Range(1, 1000);
        if (randomNmr > 900)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Rotates the target towards the player
    /// </summary>
    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}
