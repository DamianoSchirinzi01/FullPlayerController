using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{    public class IKpoints : MonoBehaviour
    {
        public Transform leftIKpoint;
        public Transform rightIKpoint;

        private void Start()
        {
            if (leftIKpoint == null || rightIKpoint == null)
            {
                Debug.LogError("No IK point set for " + transform.name);
            }
        }
    }
}

