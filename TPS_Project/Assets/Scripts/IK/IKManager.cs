using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    public class IKManager : MonoBehaviour
    {
        private WeaponManager thisWeaponManager;

        public Transform currentRightHand_IK_point;
        public Transform currentLeftHand_IK_point;
        public Transform lookAtPoint;

        public float IK_lerpMultiplier;
        public float IK_handWeight;
        public float IK_lookWeight, IK_chestWeight, IK_headWeight, IK_eyeWeight, IK_clampedWeight;

        public bool lerpHandIK;
        public bool isGunEquipped;
        public bool isLookIK_enabled = true;

        private void Awake()
        {
            thisWeaponManager = GetComponentInParent<WeaponManager>();
        }

        private void Update()
        {
            isGunEquipped = checkIfGunIsEquipped();
            lerpHandIKweight();
        }

        public void updateCurrentIKpoints(Weapon thisWeapon)
        {
            currentRightHand_IK_point = thisWeapon.rightIK_point;
            currentLeftHand_IK_point = thisWeapon.leftIK_point;
        }

        public void resetWeight()
        {
            IK_handWeight = .2f;
        }

        public void startLerpingHandIK()
        {
            lerpHandIK = true;
        }
        public void stopLerpingHandIK()
        {
            lerpHandIK = false;
        }

        private void lerpHandIKweight()
        {
            if (isGunEquipped)
            {
                if (lerpHandIK)
                {
                    IK_handWeight += Time.deltaTime * IK_lerpMultiplier;

                    if (IK_handWeight >= .99f)
                    {
                        IK_handWeight = 1;
                    }
                }
            }
            else
            {
                IK_handWeight -= Time.deltaTime;

                if (IK_handWeight <= .01f)
                {
                    IK_handWeight = 0;
                }
            }            
        }

        public bool checkIfGunIsEquipped()
        {
            if (thisWeaponManager.currentHandState == handStates.GunEquipped_Resting || thisWeaponManager.currentHandState == handStates.GunEquipped_Aiming)
            {
                

                return true;
            }
            else
            {
               

                return false;
            }
        }
    }

}
