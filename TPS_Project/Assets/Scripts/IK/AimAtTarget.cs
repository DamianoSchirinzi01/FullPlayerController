using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{    public class AimAtTarget : MonoBehaviour
    {
        public Transform aimAtTarget;
        private void FixedUpdate()
        {
            transform.LookAt(aimAtTarget);
        }
    }
}
