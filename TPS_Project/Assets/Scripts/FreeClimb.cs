using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{    public class FreeClimb : MonoBehaviour
    {
        Animator thisAnim; //TEMPORARY
        //Variables
        public bool isClimbing;
        public bool inPosition;
        public bool isLerping;

        public float tempHorizontalInput;
        public float tempVerticalInput;
        public float climbAngleLimit;
        public float positionOffset;
        public float offsetFromWall;
        public float t;
        float delta;
        public float climbSpeed = 3;
        public float rotateSpeed = 5;
        public float inAngleDistance = 1;

        public Vector3 startPos;
        public Vector3 targetPos;

        public Quaternion startRot;
        public Quaternion targetRot;

        private Transform climbHelper;

        public IKSnapshot baseIKsnapshot;
        public AnimHook thisAnimHook;

        public LayerMask whatIsClimbable;

        private void Awake()
        {
            thisAnim = GetComponentInChildren<Animator>();
            thisAnimHook = GetComponentInChildren<AnimHook>();
        }

        void Start()
        {
            Init();
        }

        private void Init()
        {
            climbHelper = new GameObject().transform;
            climbHelper.name = "Climb helper";
            thisAnimHook.Init(this, climbHelper);

            CheckForClimbableWall(); //TEMPORARILY CALLED HERE FOR TESTING
        }

        private void Update()
        {
            delta = Time.deltaTime;
            checkClimbingState(delta);
        }

        //Cast ray to detect if climbable wall is infront of player
        private void CheckForClimbableWall()
        {
            Vector3 rayOrigin = transform.position;
            rayOrigin.y += 1.4f;
            Vector3 rayDir = transform.forward;
            RaycastHit rayHit;

            Debug.DrawRay(rayOrigin, rayDir * 5f, Color.green);
            if (Physics.Raycast(rayOrigin, rayDir, out rayHit, 5, whatIsClimbable))
            {
                Debug.Log("Detected wall " + rayHit.transform.name);
                climbHelper.position = getPositionWithOffset(rayOrigin, rayHit.point);
                InitialiseClimb(rayHit);
            }
        }

        //Initialise climbing variables e.g. startPos, targetPos, helperRotation, isClimbing, 
        private void InitialiseClimb(RaycastHit hit)
        {
            //We found a climbable wall
            isClimbing = true;
            climbHelper.rotation = Quaternion.LookRotation(-hit.normal);
            startPos = transform.position;
            targetPos = hit.point + (hit.normal * offsetFromWall);
            t = 0;
            inPosition = false; //Still need to move to new targetPos
            thisAnim.CrossFade("Climb_Hang", 2);
        }

        private void checkClimbingState(float delta)
        {
            if (!inPosition)
            {
                moveToStartPosition();
                return;
            }

            //When climbing has started, player will have two states
            if (!isLerping) //finding position
            {
                //Get input
                tempHorizontalInput = Input.GetAxis("Horizontal");
                tempVerticalInput = Input.GetAxis("Vertical");
                float absoluteInput = Mathf.Abs(tempHorizontalInput) + Mathf.Abs(tempVerticalInput);

                //Get direction relative to the climb helper
                Vector3 horizontal = climbHelper.right * tempHorizontalInput;
                Vector3 vertical = climbHelper.up * tempVerticalInput;
                //Store the direction
                Vector3 moveDir = (horizontal + vertical).normalized;

                //Check if we can move
                bool canMove = checkCanMove(moveDir);
                if (!canMove || moveDir == Vector3.zero)
                    return;

                t = 0;
                startPos = transform.position;
                targetPos = climbHelper.position;

                thisAnimHook.createIKpositions(targetPos);

                isLerping = true;
            }
            else //movingToPosition
            {
                t += delta * climbSpeed;

                if (t > 1)
                {
                    t = 1;
                    isLerping = false;
                }
                    Vector3 climbPos = Vector3.Lerp(startPos, targetPos, t);
                    transform.position = climbPos;
                    transform.rotation = Quaternion.Slerp(transform.rotation, climbHelper.rotation, delta * rotateSpeed);
                
            }
        } 

        private bool checkCanMove(Vector3 moveDir) //Apply layer mask to these rayCasts!!
        {
            Vector3 rayOrigin = transform.position;
            float distance1 = positionOffset;
            Vector3 direction = moveDir;
            RaycastHit hit;
            
            Debug.DrawRay(rayOrigin, direction * distance1, Color.green);
            if (Physics.Raycast(rayOrigin, direction, out hit, distance1))
            {
                return false;
            }

            rayOrigin += moveDir * distance1;
            direction = climbHelper.forward;
            float distance2 = inAngleDistance;

            Debug.DrawRay(rayOrigin, direction * distance1, Color.cyan);
            if(Physics.Raycast(rayOrigin, direction, out hit, distance1))
            {
                climbHelper.position = getPositionWithOffset(rayOrigin, hit.point);
                climbHelper.rotation = Quaternion.LookRotation(-hit.normal);
                return true;
            }

            rayOrigin += direction * distance2;
            direction = -Vector3.up;

            Debug.DrawRay(rayOrigin, direction * distance2,  Color.yellow);
            if(Physics.Raycast(rayOrigin, direction, out hit, distance2))
            {
                //Check the angle between the helper and this hits normal
                float angle = Vector3.Angle(climbHelper.up, hit.normal);
                //If angle is below threshold
                if(angle < climbAngleLimit)
                {              
                    //Set helper pos and rot
                    climbHelper.position = getPositionWithOffset(rayOrigin, hit.point);
                    climbHelper.rotation = Quaternion.LookRotation(-hit.normal);
                    return true;
                }
            }

            return false;
        }

        //Starts the climb
        private void moveToStartPosition()
        {
            t += delta;

            if(t > 1)
            {
                t = 1f;
                inPosition = true;

                //Enable IK here!!!!!!!!!!
                thisAnimHook.createIKpositions(targetPos);
            }

            Vector3 newTargetPos = Vector3.Lerp(startPos, targetPos, t);
            transform.position = newTargetPos;
            transform.rotation = Quaternion.Slerp(transform.rotation, climbHelper.rotation, delta * rotateSpeed);
        }

        //Need to get a position with an offset from the target wall
        private Vector3 getPositionWithOffset(Vector3 origin, Vector3 target)
        {
            Vector3 dir = origin - target;
            dir.Normalize(); //Dir becomes a unit vector after being normalised
            Vector3 offset = dir * offsetFromWall; 

            return target + offset;
        }
    }

    [System.Serializable]
    public class IKSnapshot
    {
        public Vector3 rightHand, leftHand, rightFoot, leftFoot;
    }
}

