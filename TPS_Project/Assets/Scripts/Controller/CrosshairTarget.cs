using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairTarget : MonoBehaviour
{
    Camera mainCam;
    Ray ray;
    RaycastHit hit;
    public LayerMask ignoreMask;

    // Start is called before the first frame update
    void Awake()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        ray.origin = mainCam.transform.position;
        ray.direction = mainCam.transform.forward;
        Physics.Raycast(ray, out hit, ignoreMask);
        transform.position = hit.point;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, .02f);
    }
}
