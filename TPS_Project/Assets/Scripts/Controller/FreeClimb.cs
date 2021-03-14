using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{    public class FreeClimb : MonoBehaviour
    {
        PlayerController thisController;
        AnimHook thisAnimHook;
        PlayerInput input;
        public float test = 2f;

        //Variables
        public bool isClimbing;
        public bool inPosition;
        public bool isLerping;
        public bool isMidTransition;

        public bool canClimbLedge;
        public bool isTouchingWall;
        public bool isTouchingWallAbove;
        public bool ledgeDetected;
       
        public float climbAngleLimit;
        public float positionOffset;      
        public float offsetFromWall;
        public float t;
        float delta;
        public float climbSpeed = 3;
        public float rotateSpeed = 5;

        public float rayForwardTowardWall = 1;
        public float rayTowardMoveDirection = 0.5f; //Increase for jump

        public float ledgeClimbXoffset1 = 0f;
        public float ledgeClimbYoffset2 = 0f;
        public Vector3 holdPosition;
        public Vector3 moveToOffset;
        public Transform moveToPoint;

        public Vector3 yOffset;
        public Vector3 startPos;
        public Vector3 targetPos;

        public Quaternion startRot;
        public Quaternion targetRot;

        private Transform climbHelper;
        public Transform orientation;

        public float miny;
        public float maxY;
        public float testMax;

        public IKSnapshot baseIKsnapshot;

        public LayerMask whatIsClimbable;
        public LayerMask whatIsGround;

        private void Awake()
        {
            thisAnimHook = GetComponentInChildren<AnimHook>();
            thisController = GetComponent<PlayerController>();
            input = GetComponent<PlayerInput>();
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
        }

        private void Update()
        {
            delta = Time.deltaTime;

            if (isClimbing)
            {
                checkForLedge();
                checkLedgeClimb();
            }
        }

        private void FixedUpdate()
        {
            if (isClimbing)
            {
                ClimbingLookRotation();
            }
        }

        private void ClimbingLookRotation()
        {
            if (input.mouseX != 0 || input.mouseY != 0f)
            {
                orientation.rotation *= Quaternion.AngleAxis(input.mouseX, Vector3.up);
                orientation.rotation *= Quaternion.AngleAxis(input.mouseY, Vector3.right);
            }

            var rotX = orientation.eulerAngles.x;
            var rotY = orientation.eulerAngles.y;

            rotX = HelperFunctions.ClampAngle(rotX, -20f, 20f);
            rotY = HelperFunctions.ClampAngle(rotY, 200f, 355f);

            orientation.eulerAngles = new Vector3(rotX, rotY, 0);
        }


        //Cast ray to detect if climbable wall is infront of player
        public bool CheckForClimbableWall()
        {
            Vector3 rayOrigin = transform.position;
            rayOrigin.y += .02f;
            Vector3 rayDir = transform.forward;
            RaycastHit rayHit;

            if (Physics.Raycast(rayOrigin, rayDir, out rayHit, 1, whatIsClimbable))
            {
                climbHelper.position = getPositionWithOffset(rayOrigin, rayHit.point);
                InitialiseClimb(rayHit);

                return true;
            }

            return false;
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
        }

        public void checkClimbingState(float _delta)
        {
            this.delta = _delta;
            
            if (!inPosition)
            {
                moveToStartPosition();
                return;
            }

            //When climbing has started, player will have two states
            if (!isLerping) //finding position
            {
                if (!thisController.canMove)
                    return;

                //Get direction relative to the climb helper
                Vector3 horizontal = climbHelper.right * input.horizontal;
                Vector3 vertical = climbHelper.up * input.vertical;
                //Store the direction
                Vector3 moveDir = (horizontal + vertical).normalized;

                if (isMidTransition)
                {
                    if (moveDir == Vector3.zero) 
                    return;
                }
                else
                {
                    //Check if we can move
                    bool canMove = checkCanMove(moveDir);
                    if (!canMove || moveDir == Vector3.zero)
                        return;
                }

                isMidTransition = !isMidTransition;               

                t = 0;
                isLerping = true;
                startPos = transform.position;
                Vector3 newTargetPos = climbHelper.position - transform.position;
                float distance = Vector3.Distance(climbHelper.position, startPos) / 2;
                newTargetPos *= positionOffset;
                newTargetPos += transform.position;
                targetPos = (isMidTransition) ? newTargetPos : climbHelper.position;

                thisAnimHook.createIKpositions(targetPos, moveDir, isMidTransition);
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

                lookForGround();
            }
        }      

        private bool checkCanMove(Vector3 moveDir) //Apply layer mask to these rayCasts!!
        {
            Vector3 rayOrigin = transform.position;
            float distance1 = rayTowardMoveDirection;
            Vector3 direction = moveDir;
            RaycastHit hit;

            //Raycast towards current move direction
           //DebugLine.instance.setLine(rayOrigin, rayOrigin + (direction * distance1), 0);
            if (Physics.Raycast(rayOrigin, direction, out hit, distance1, whatIsClimbable))
            {
                Debug.Log("false");
                return false;
            }
                       
            rayOrigin += moveDir * distance1;
            direction = climbHelper.forward;
            float distance2 = rayForwardTowardWall;

            //Raycast towards wall
           // DebugLine.instance.setLine(rayOrigin, rayOrigin + (direction * distance2), 1);
            if (Physics.Raycast(rayOrigin, direction, out hit, distance2, whatIsClimbable))
            {
                climbHelper.position = getPositionWithOffset(rayOrigin, hit.point);
                climbHelper.rotation = Quaternion.LookRotation(-hit.normal);
                return true;
            }

            rayOrigin = rayOrigin + (direction * distance2);
            direction = -moveDir;

            //DebugLine.instance.setLine(rayOrigin, rayOrigin + direction, 2);
            if (Physics.Raycast(rayOrigin, direction, out hit, rayForwardTowardWall))
            {
                climbHelper.position = getPositionWithOffset(rayOrigin, hit.point);
                climbHelper.rotation = Quaternion.LookRotation(-hit.normal);
                return true;
            }

            //return false; //When player reaches the ledge, climb up

            rayOrigin += direction * distance2;
            direction = -Vector3.up;

            //DebugLine.instance.setLine(rayOrigin, rayOrigin + direction, 3);
            if (Physics.Raycast(rayOrigin, direction, out hit, distance2, whatIsClimbable))
            {
                //Check the angle between the helper and this hits normal
                float angle = Vector3.Angle(-climbHelper.forward, hit.normal);
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
            t += delta * 10;

            if(t > 1)
            {
                t = 1f;
                inPosition = true;
                stopInput();

                //Enable IK here!!!!!!!!!!
                thisAnimHook.createIKpositions(targetPos, Vector3.zero, false);
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

        #region LedgeClimbing
        public void finishLedgeClimb() //Called by animator
        {
            canClimbLedge = false;
            transform.position = moveToPoint.position;
            ledgeDetected = false;

            Destroy(moveToPoint.gameObject);
            moveToPoint = null;
            cancelClimb(false);
        }

        private void checkLedgeClimb()
        {
            if (ledgeDetected && !canClimbLedge) //Hold position while animation plays
            {
                canClimbLedge = true;

                holdPosition = transform.position;

                moveToPoint = new GameObject().transform;
                moveToPoint.name = "!LEDGE MOVE TO POINT!";
                moveToPoint.position = transform.position + moveToOffset;

                thisController.canMove = false;

                input.startedToClimbLedge = true;
                thisAnimHook.beginLedgeClimb();
            }

            if (canClimbLedge) //Set transform to hold position
            {
                transform.position = holdPosition;
            }
        }

        private void checkForLedge()
        {
            Vector3 originPoint = transform.position;
            Vector3 originPoint2 = transform.position + yOffset;
            float rayDistance = 5f;
            Vector3 direction = transform.forward;

            DebugLine.instance.setLine(originPoint, originPoint + (direction * rayDistance), 0);
            DebugLine.instance.setLine(originPoint2, originPoint2 + (direction * rayDistance), 1);

            isTouchingWall = Physics.Raycast(originPoint, direction * rayDistance, whatIsClimbable);
            isTouchingWallAbove = Physics.Raycast(originPoint + yOffset, direction * rayDistance, whatIsClimbable);

            if (isTouchingWall && !isTouchingWallAbove && !ledgeDetected)
            {
                ledgeDetected = true; //Found ledge
            }
        }
        
        #endregion

        #region dismounting wall   
        private void lookForGround()
        {
            Vector3 rayOrigin = transform.position;
            Vector3 direction = -Vector3.up;
            RaycastHit hit;

            //DebugLine.instance.setLine(rayOrigin, rayOrigin + direction, 3);
            if(Physics.Raycast(rayOrigin, direction, out hit, 1.2f, whatIsGround))
            {
                cancelClimb(false);
            }
        }

        public void cancelClimb(bool _pushBack)
        {
            stopInput();

            if (_pushBack)
            {
                Vector3 origin = transform.position;
                Vector3 direction = transform.forward;
                RaycastHit hit;

                Physics.Raycast(origin, direction * 5f, out hit, whatIsClimbable);                
                Quaternion lookAtDir = Quaternion.LookRotation(hit.normal);

                Vector3 dir = ((-transform.forward * 2));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookAtDir, 100 * Time.deltaTime);
                transform.Translate(dir * 150f * Time.deltaTime, Space.World);
            }

            thisAnimHook.goalsHaveUpdated = false;
            isClimbing = false;
            input.startedToClimbLedge = false;

            thisController.disableClimbing();
        }

        private void stopInput()
        {
            input.horizontal = 0;
            input.vertical = 0;
        }

        #endregion
    }


    [System.Serializable]
    public class IKSnapshot
    {
        public Vector3 rightHand, leftHand, rightFoot, leftFoot;
    }
}

