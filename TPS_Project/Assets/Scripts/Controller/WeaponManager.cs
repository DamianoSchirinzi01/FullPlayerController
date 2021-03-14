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
        public Transform currentWeaponTransform;

        public int currentWeaponIndex;

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
        }

        private void handleHandStates()
        {
            if(currentWeaponIndex == 0 && !input.isAiming || currentWeaponIndex == 1 && !input.isAiming) //If holding gun but not aiming
            {
                currentHandState = handStates.GunEquipped_Resting;
                setWeaponToRestingPos(currentWeaponTransform.GetComponent<Weapon>());
            }

            if (currentWeaponIndex == 0 && input.isAiming || currentWeaponIndex == 1 && input.isAiming) //if holding gun and aiming
            {
                currentHandState = handStates.GunEquipped_Aiming;
                setWeaponToAimingPos(currentWeaponTransform.GetComponent<Weapon>());
            }

            if (currentWeaponIndex == 2) //If no gun is equipped
            {
                currentHandState = handStates.GunsHolstered;
            }
        }
    }

    public enum handStates
    {
        GunsHolstered,
        GunEquipped_Resting,
        GunEquipped_Aiming
    }

}
