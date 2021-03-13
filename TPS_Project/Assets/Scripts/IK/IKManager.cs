using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    public class IKManager : MonoBehaviour
    {
        private PlayerInput input;
        //Store weapon
        public List<IKpoints> weaponIKpoints;

        public Vector3 assaultRifleRestingPos;
        public Quaternion assaultRifleRestingRot;

        public Vector3 AssaultRifleAimingPos;
        public Quaternion AssaultRifleAimingRot;

        public Transform currentRightIKpoint { get; private set; }
        public Transform currentLeftIKpoint {get; private set; }

        private void Awake()
        {
            input = GetComponentInParent<PlayerInput>();
        }

        private void Start()
        {
            setCurrentIKpoints(weaponIKpoints[0]);
        }

        private void Update()
        {
            setWeaponPosition();
        }

        public void setCurrentIKpoints(IKpoints currentWeaponIKpoints)
        {
            currentRightIKpoint = currentWeaponIKpoints.rightIKpoint;
            currentLeftIKpoint = currentWeaponIKpoints.leftIKpoint;
        }

        public void setWeaponPosition()
        {
            if (input.isAiming)
            {
                weaponIKpoints[0].transform.position = AssaultRifleAimingPos;
                weaponIKpoints[0].transform.rotation = AssaultRifleAimingRot;
            }
            else
            {
                weaponIKpoints[0].transform.position = assaultRifleRestingPos;
                weaponIKpoints[0].transform.rotation = assaultRifleRestingRot;
            }
        }



        //Get positions for weapon when resting and when aiming
        //Eventually lerp weight for smooth transition between states
        //Get IKpoints
        //Pass IKpoints to Anim hook


        //Handle weapon Pos & Rot
        //If weapon is equipped but not aiming, put in resting position
        //else if weapon is equipped and player is aiming, put in aim position



    }

}
