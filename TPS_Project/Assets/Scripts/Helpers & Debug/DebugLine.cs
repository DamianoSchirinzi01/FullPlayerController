using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS {   
    public class DebugLine : MonoBehaviour
    {
        public static DebugLine instance;
        public int maxRenderers;

        List<LineRenderer> lines = new List<LineRenderer>();

        private void Awake()
        {
            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        private void CreateLine(int i)
        {
            GameObject thisGO = new GameObject();
            lines.Add(thisGO.AddComponent<LineRenderer>());
            lines[i].widthMultiplier = 0.05f;
        }

        public void setLine(Vector3 startPosition, Vector3 endPosition, int index)
        {
            if (index > lines.Count - 1)
            {
                CreateLine(index);
            }

            lines[index].SetPosition(0, startPosition);
            lines[index].SetPosition(1, endPosition);
        }
    }
}

