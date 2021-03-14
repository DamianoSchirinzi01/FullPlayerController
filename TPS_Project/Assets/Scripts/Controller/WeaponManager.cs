using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    public class WeaponManager : MonoBehaviour
    {
        public handStates currentHandState;

        private PlayerInput input;
        private IKManager thisIKmanager;

        public List<Weapon> weaponsList;
        private Weapon currentWeapon;
        public Transform currentWeaponTransform;

        public int currentWeaponIndex;

        [Header("Shooting")]
        public LayerMask ignoreLayer;
        public Transform rayDestination;
        public bool isFiring;
        Ray ray;
        RaycastHit hit;

        private void Awake()
        {
            input = GetComponent<PlayerInput>();
            thisIKmanager = GetComponentInChildren<IKManager>();
        }

        private void Start()
        {
            currentWeaponIndex = 2;
            stashAllWeapons();
        }        

        private void Update()
        {
            handleHandStates();
        }

        public void startFiring()
        {
            isFiring = true;


            currentWeapon.emitMuzzleFlash();
            shoot();
        }

        public void stopFiring()
        {
            isFiring = false;
        }

        private void shoot()
        {
            ray.origin = currentWeapon.firePoint.position;
            ray.direction = rayDestination.position - ray.origin;

            var tracer = Instantiate(currentWeapon.bulletTracer, currentWeapon.firePoint.position, Quaternion.identity);
            tracer.AddPosition(ray.origin);



            if (Physics.Raycast(ray, out hit, ignoreLayer))
            {
                currentWeapon.hitEffect.transform.position = hit.point;
                currentWeapon.hitEffect.transform.forward = hit.normal;
                currentWeapon.hitEffect.Emit(1);

                tracer.transform.position = hit.point;
            }
        }

        #region Weapon Switching

        public void setCurrentWeaponIndex(int inputIndex)
        {
            currentWeaponIndex = inputIndex;
        }

        private void setWeaponToAimingPos(Weapon thisWeapon)
        {
            currentWeaponTransform.position = thisWeapon.aimingPos.position;
            currentWeaponTransform.rotation = thisWeapon.aimingPos.rotation;
        }
        private void setWeaponToRestingPos(Weapon thisWeapon)
        {
            currentWeaponTransform.position = thisWeapon.restingPos.position;
            currentWeaponTransform.rotation = thisWeapon.restingPos.rotation;
        }

        private void setWeaponToHolsteredPos(Weapon thisWeapon)
        {
            thisWeapon.transform.position = thisWeapon.holsteredPos.position;
            thisWeapon.transform.rotation = thisWeapon.holsteredPos.rotation;
        }

        public void swapWeapon(int inputIndex)
        {
            setCurrentWeaponIndex(inputIndex);

            //Check if animation is complete before
            equipWeapon(weaponsList[currentWeaponIndex]);
        }

        private void equipWeapon(Weapon thisWeapon)
        {
            currentWeaponTransform = thisWeapon.gameObject.transform;
            currentWeapon = thisWeapon;

            foreach(Weapon w in weaponsList) //If w is not "thisWeapon"
            {
                if(w != thisWeapon)
                {
                    setWeaponToHolsteredPos(w); //Holster w
                }
            }

            thisIKmanager.updateCurrentIKpoints(thisWeapon);
            thisIKmanager.stopLerpingHandIK();
        }

        //Hide all weapons
        public void stashAllWeapons()
        {
            foreach(Weapon w in weaponsList)
            {
                setWeaponToHolsteredPos(w);
            }

            currentWeaponTransform = null;
            currentWeapon = null;
        }

        private void handleHandStates()
        {
            if(currentWeaponIndex == 0 && !input.isAiming || currentWeaponIndex == 1 && !input.isAiming) //If holding gun but not aiming
            {
                currentHandState = handStates.GunEquipped_Resting;
                if (!isFiring)
                {
                    setWeaponToRestingPos(currentWeapon);
                }
            }

            if (currentWeaponIndex == 0 && input.isAiming || currentWeaponIndex == 1 && input.isAiming) //if holding gun and aiming
            {
                currentHandState = handStates.GunEquipped_Aiming;
                setWeaponToAimingPos(currentWeapon);
            }

            if (currentWeaponIndex == 2) //If no gun is equipped
            {
                currentHandState = handStates.GunsHolstered;
            }
        }
    }

    #endregion

    public enum handStates
    {
        GunsHolstered,
        GunEquipped_Resting,
        GunEquipped_Aiming
    }

}
