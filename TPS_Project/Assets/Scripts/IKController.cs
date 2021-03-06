using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;


public class IKController : MonoBehaviour
{
    [Header("Target references")]
    public Transform headAimTarget;
    public Transform chestAimTarget;

    public TwoBoneIKConstraint leftArmConstraint;
    public TwoBoneIKConstraint rightArmConstraint;
    public Transform leftArmTarget;
    public Transform rightArmTarget;

    public Transform leftLegTarget;
    public Transform rightLegTarget;

    public Transform leftLegAnkle;
    public Transform rightLegAnkle;

    public Transform leftLegRayPoint;
    public Transform rightLegRayPoint;

    private Vector3 originalHeadAimPos;
    private Vector3 originalChestAimPos;

    [Header("Adjustable Variables")]
    public Vector3 skinDepthLeft;
    public Vector3 leftAnkleBaseRotation;
    public Vector3 rightAnkleBaseRotation;
    public Vector3 skinDepthRight;
    public float transitionTime;
    public float rayDistance;
    public bool isOnSlope;

    [Header("Steep slope")]   
    private Vector3 headTargetGoal_up = new Vector3(0, 2.1f, .8f); //_up is if the player is going up a slope
    private Vector3 chestTargetGoal_up = new Vector3(0, 0.1f, .8f);
    private Vector3 leftTargetGoal_up = new Vector3(-.3f, .8f, 0.4f);
    private Vector3 rightTargetGoal_up = new Vector3(0.091f, .8f, 0.5f);


    [SerializeField] private LayerMask whatIsGround;
    // Start is called before the first frame update
    void Start()
    {
        originalHeadAimPos = headAimTarget.localPosition;
        originalChestAimPos = chestAimTarget.localPosition;
    }

    private void Update()
    {
        isOnSlope = checkGroundNormals();

        adjustIKforSlope();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        findFootPosition();
    }

    private void findFootPosition()
    {
        RaycastHit hitLeft;
        RaycastHit hitRight;

        if(Physics.Raycast(leftLegRayPoint.position, Vector3.down * rayDistance, out hitLeft, whatIsGround))
        {
            leftLegTarget.position = hitLeft.point + skinDepthLeft;

            Vector3 crossVector = Vector3.Cross(-leftLegTarget.right, hitLeft.normal);
            leftLegAnkle.rotation = Quaternion.LookRotation(crossVector, hitLeft.normal);
            leftLegAnkle.rotation = leftLegAnkle.transform.rotation * Quaternion.Euler(leftAnkleBaseRotation);
        }
        else
        {
            Debug.LogError( leftLegTarget + "Cannot find target");
        }

        if (Physics.Raycast(rightLegRayPoint.position, Vector3.down * rayDistance, out hitRight, whatIsGround))
        {
            rightLegTarget.position = hitRight.point + skinDepthRight;

            Vector3 crossVector = Vector3.Cross(rightLegTarget.right, hitLeft.normal);
            rightLegAnkle.rotation = Quaternion.LookRotation(crossVector, hitLeft.normal);
            rightLegAnkle.rotation = rightLegAnkle.transform.rotation * Quaternion.Euler(rightAnkleBaseRotation);
        }
        else
        {
            Debug.LogError(leftLegTarget + "Cannot find target");
        }
    }

    private void adjustIKforSlope()
    {
        if (isOnSlope)
        {
            chestAimTarget.localPosition = lerpVector(chestAimTarget.localPosition, chestTargetGoal_up, transitionTime);
            headAimTarget.localPosition = lerpVector(headAimTarget.localPosition, headTargetGoal_up, transitionTime);

            leftArmConstraint.weight = Mathf.Lerp(leftArmConstraint.weight, .7f, transitionTime * Time.deltaTime);
            rightArmConstraint.weight = Mathf.Lerp(rightArmConstraint.weight, .7f, transitionTime * Time.deltaTime);

            leftArmTarget.localPosition = lerpVector(leftArmTarget.localPosition, leftTargetGoal_up, transitionTime);
            rightArmTarget.localPosition = lerpVector(rightArmTarget.localPosition, leftTargetGoal_up, transitionTime);
        }
        else
        {
            chestAimTarget.localPosition = lerpVector(chestAimTarget.localPosition, originalChestAimPos, transitionTime);
            headAimTarget.localPosition = lerpVector(chestAimTarget.localPosition, originalHeadAimPos, transitionTime);

            leftArmConstraint.weight = Mathf.Lerp(leftArmConstraint.weight, 0f, transitionTime * Time.deltaTime);
            rightArmConstraint.weight = Mathf.Lerp(rightArmConstraint.weight, 0f, transitionTime * Time.deltaTime);
        }
    }

    private Vector3 lerpVector(Vector3 value_1, Vector3 value_2, float _lerpTime)
    {
        Vector3 lerpedValue = Vector3.zero;

        lerpedValue = Vector3.Lerp(value_1, value_2, _lerpTime * Time.deltaTime);

        return lerpedValue;
    }

    private bool checkGroundNormals()
    {
        RaycastHit hitGround;

        if(Physics.Raycast(transform.position, Vector3.down * rayDistance, out hitGround, whatIsGround))
        {
            if(hitGround.normal == Vector3.up)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        return false;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawRay(leftLegRayPoint.position, Vector3.down * rayDistance);
        Gizmos.DrawRay(rightLegRayPoint.position, Vector3.down * rayDistance);

    }
}
