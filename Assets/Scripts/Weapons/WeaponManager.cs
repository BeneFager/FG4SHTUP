using UnityEngine;
/// <summary>
/// Holds the weapons that the player picks up
/// this is not scalable at all and im not happy with this
/// </summary>
namespace Weapons
{
    public class WeaponManager : MonoBehaviour
    {
        public int selectedWeapon = 0;
        private bool pickedUpMissile, pickedUpAutoGun;

        public static WeaponManager instance;
        private void Awake()
        {
            instance = this;
        }

        /// <summary>
        /// Scrolling to select weapon if it is in the inventory
        /// </summary>
        private void Update()
        {
            int previousSelectedWeapon = selectedWeapon;
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                if (selectedWeapon >= transform.childCount - 1)
                {
                    selectedWeapon = 0;
                }
                else
                {
                    selectedWeapon++;
                }
            }

            if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                if (selectedWeapon <= 0)
                {
                    selectedWeapon = transform.childCount - 1;
                }
                else
                {
                    selectedWeapon--;
                }
            }

            if (previousSelectedWeapon != selectedWeapon)
            {
                SetActiveWeapon();
            }

        }

        /// <summary>
        /// Checks in the interactable object matches the weapon
        /// </summary>
        /// <param name="interactableObject"></param>
        public void AddWeaponToInventory(InteractableObject interactableObject)
        {
            if (interactableObject.CompareTag("Missile"))
            {
                Debug.Log("picked up missile");
                pickedUpMissile = true;
            }

            if (interactableObject.CompareTag("AutoGun"))
            {
                Debug.Log("picked up AutoGun");
                pickedUpAutoGun = true;
            }
        }

        /// <summary>
        /// this is shit
        /// Could show on UI which weapon is selected
        /// </summary>
        private void SetActiveWeapon()
        {
            if (!pickedUpMissile && !pickedUpAutoGun)
            {
                return;
            }

            int i = 0;
            foreach (Transform weapon in transform)
            {
                if (i == selectedWeapon)
                {
                    if (weapon.CompareTag("AutoGun") && !pickedUpAutoGun)
                    {
                        continue;
                    }

                    if (weapon.CompareTag("Missile") && !pickedUpMissile)
                    {
                        continue;
                    }

                    weapon.gameObject.SetActive(true);

                    if (weapon.CompareTag("Missile"))
                    {
                        var childScript = weapon.GetComponentsInChildren<MissileLauncher>();
                        foreach (var script in childScript)
                        {
                            script.enabled = true;
                        }
                    }
                    if (weapon.CompareTag("AutoGun"))
                    {
                        var childScript = weapon.GetComponentsInChildren<AutomaticBullets>();
                        foreach (var script in childScript)
                        {
                            script.enabled = true;
                        }
                    }
                }
                else
                {
                    weapon.gameObject.SetActive(false);
                }

                i++;
            }
        }
    }
}