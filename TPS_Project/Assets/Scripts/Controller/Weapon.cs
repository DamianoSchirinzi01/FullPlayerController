using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    public class Weapon : MonoBehaviour
    {
        public int weaponID;

        public Transform rightIK_point;
        public Transform leftIK_point;

        public Transform restingPos;
        public Transform aimingPos;
        public Transform holsteredPos;

        public float damage;
        public float rateOfFire;
    }
}

