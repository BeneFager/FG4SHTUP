using Interfaces;
using UnityEngine;

/// <summary>
/// Missile shot from a firepoint on the prefab
/// </summary>
namespace Weapons
{
    public class MissileLauncher : MonoBehaviour, IWeapon
    {
        [SerializeField] Transform firePoint = default;
        public LayerMask enemyLayer;
        Camera ShootRefcamera;
        private float timeBetweenShots;
        float startTime;
        ObjectPooler objectpooler;
        RaycastHit hit;

        private void Start()
        {
            enabled = false;
            objectpooler = ObjectPooler.instance;
        }

        void OnEnable()
        {
            ShootRefcamera = Camera.main;
        }

        void Update()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
            //Todo implement this
            /*
            if (Input.GetKeyDown(KeyCode.Mouse1)) // not implemented
            {
                startTime = Time.time;
                Debug.Log("pressed 2nd mb");
            }
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                Debug.Log("Released 2nd mb");
                float pressedTime = Time.time - startTime;
                bool lockedOn = pressedTime > 2f;
                if (lockedOn &&
                    Physics.Raycast(ShootRefcamera.transform.position, ShootRefcamera.transform.forward, out hit, Mathf.Infinity, enemyLayer)
                    )
                {
                    Debug.Log("Locked ON");
                    ShootHomingMissile();
                }
            }
            */
        }

        void ShootHomingMissile()
        {
            Debug.Log("Shooting homingmisslie");
        }

        /// <summary>
        /// Spawns object from objectpool
        /// </summary>
        public void Shoot()
        {
            Debug.Log("shoot missile");
            objectpooler.SpawnFromPool("Missile", firePoint.position, firePoint.rotation);
        }
    }
}