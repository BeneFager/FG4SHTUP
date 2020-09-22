using Interfaces;
using UnityEngine;

/// <summary>
/// Shoots when MB1 is held down but only damages if you are aiming at an enemy
/// </summary>
namespace Weapons
{
    public class AutomaticBullets : MonoBehaviour, IWeapon
    {
        public LayerMask enemyLayer;
        Camera ShootRefcamera;
        RaycastHit hit;
        int damage = 5;
        float fireRate = 10f;
        float nextTimeToFire = 0f;

        private void Start()
        {
            enabled = false;
        }
        
        private void OnEnable()
        {
            ShootRefcamera = Camera.main;
        }

        void Update()
        {
            if (Input.GetButton("Fire1") && Time.time  >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1 / fireRate;
                Debug.Log("shoot Auto");

                Shoot();
            }
        }

        public void Shoot()
        {
            //muzzleflash?
            if (Physics.Raycast(ShootRefcamera.transform.position, ShootRefcamera.transform.forward, out hit, Mathf.Infinity, enemyLayer))
            {
                Debug.Log(hit.transform.name);
                EnemyStats enemyStats = hit.transform.GetComponent<EnemyStats>();
                if (enemyStats != null)
                {
                    enemyStats.TakeDamage(damage);
                }
            }

            //lägg till paricle för hit effect?
        }
    }
}