using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    public class Weapon : MonoBehaviour
    {
        public Transform firePoint;
        public Transform rightIK_point;
        public Transform leftIK_point;

        public TrailRenderer bulletTracer;
        public ParticleSystem hitEffect;
        public ParticleSystem muzzleFlash;

        public Transform restingPos;
        public Transform aimingPos;
        public Transform holsteredPos;

        public float damage;
        public float rateOfFire;

        public void emitMuzzleFlash()
        {
            muzzleFlash.Emit(1);
        }
    }
}

