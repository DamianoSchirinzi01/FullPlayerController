using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    public class AnimHook : MonoBehaviour
    {
        private Animator thisAnimator;
        private FreeClimb thisFreeClimb;

        private string InputX = "InputX";
        private string InputY = "InputY";

        private string isAiming = "isAiming";
        private string moveSpeed = "moveSpeed";
        private string isJumping = "isJumping";
        private string isSliding = "isSliding";
        private string isGrounded = "isGrounded";
        private string isCrouching = "isCrouching";
        private string isClimbing = "isClimbing";
        private string isClimbingLedge = "isClimbingLedge";

        public float lerpTime;
        float delta;
        public float climbLerpSpeed;

        [Header("Climbing")]
        public float rightHand_Weight;
        public float leftHand_Weight;
        public float rightFoot_Weight;
        public float leftFoot_Weight;

        public float wallOffset;

        IKSnapshot ikBase;
        IKSnapshot currentIKsnapshot = new IKSnapshot();
        IKSnapshot nextIKsnapshot = new IKSnapshot();
        IKGoals goals = new IKGoals();

        private Vector3 rightHand, leftHand, rightFoot, leftFoot;
        private Vector3 previousMoveDir;

        private Transform climbHelper;

        public bool goalsHaveUpdated;
        public bool isMirroringAnimation;
        private bool isMovingLeft;

        public LayerMask whatIsClimbable;

        // Start is called before the first frame update
        void Awake()
        {
            thisAnimator = GetComponent<Animator>();
            thisFreeClimb = GetComponent<FreeClimb>();
        }

        //Will refactor this
        public void updateAnimatorStates(float _inputX, float _inputY, float _speed, bool _isAiming, bool _isJumping, bool _isCrouching, bool _isGrounded, bool _isSliding, bool _isClimbing)
        {
            float XinputLerped = Mathf.Lerp(thisAnimator.GetFloat(InputX), _inputX, lerpTime * Time.deltaTime);
            float YinputLerped = Mathf.Lerp(thisAnimator.GetFloat(InputY), _inputY, lerpTime * Time.deltaTime);
            float speedLerped = Mathf.Lerp(thisAnimator.GetFloat(moveSpeed), _speed, lerpTime * Time.deltaTime);

            thisAnimator.SetBool(isAiming, _isAiming); 
            thisAnimator.SetBool(isJumping, _isJumping);
            thisAnimator.SetBool(isCrouching, _isCrouching);
            thisAnimator.SetBool(isGrounded, _isGrounded);
            thisAnimator.SetBool(isSliding, _isSliding);
            thisAnimator.SetBool(isClimbing, _isClimbing);

            thisAnimator.SetFloat(moveSpeed, speedLerped);

            thisAnimator.SetFloat(InputX, XinputLerped);
            thisAnimator.SetFloat(InputY, YinputLerped);
        }

        #region Climbing     
        public void beginLedgeClimb()
        {
            thisAnimator.SetBool(isClimbingLedge, true);
            resetIK();
        }
        public void callFinishLedgeClimb()
        {
            thisFreeClimb.finishLedgeClimb();
            thisAnimator.SetBool(isClimbingLedge, false);
        }

        public void resetIK()
        {
            updateIKWeight(AvatarIKGoal.LeftFoot, 0);
            updateIKWeight(AvatarIKGoal.RightFoot, 0);
            updateIKWeight(AvatarIKGoal.LeftHand, 0);
            updateIKWeight(AvatarIKGoal.RightHand, 0);
        }

        public void Init(FreeClimb freeClimb, Transform thisClimbHelper)
        {
            thisFreeClimb = freeClimb;
            ikBase = freeClimb.baseIKsnapshot;
            climbHelper = thisClimbHelper;
        }      

        public void createIKpositions(Vector3 originPoint, Vector3 moveDir, bool _isMidTransition)
        {
            delta = Time.deltaTime;
            HandleCimbAnimations(moveDir, _isMidTransition);

            if (!_isMidTransition)
            {
                updateGoals(moveDir);
                previousMoveDir = moveDir;
            }
            else
            {
                updateGoals(previousMoveDir);
            }

            IKSnapshot thisIK = CreateIKsnapshot(originPoint);
            copySnapshot(ref currentIKsnapshot, thisIK);

            setIKpositions(_isMidTransition, goals.leftFoot, currentIKsnapshot.leftFoot, AvatarIKGoal.LeftFoot);
            setIKpositions(_isMidTransition, goals.rightFoot, currentIKsnapshot.rightFoot, AvatarIKGoal.RightFoot);
            setIKpositions(_isMidTransition, goals.leftHand, currentIKsnapshot.leftHand, AvatarIKGoal.LeftHand);
            setIKpositions(_isMidTransition, goals.rightHand, currentIKsnapshot.rightHand, AvatarIKGoal.RightHand);

            updateIKWeight(AvatarIKGoal.LeftFoot, 1);
            updateIKWeight(AvatarIKGoal.RightFoot, 1);
            updateIKWeight(AvatarIKGoal.LeftHand, 1);
            updateIKWeight(AvatarIKGoal.RightHand, 1);
        }

        private void updateGoals(Vector3 moveDir) //ERROR WHERE IK IS NOT BEING SET AT START OF CLIMB. Maybe update goals twice on first move??
        {
            isMovingLeft = (moveDir.x <= 0);

            if(moveDir.x != 0)
            {
                goals.leftHand = isMovingLeft;
                goals.rightHand = !isMovingLeft;
                goals.leftFoot = isMovingLeft;
                goals.rightFoot = !isMovingLeft;
            }
            else
            {
                bool isEnabled = !isMirroringAnimation;
                if (moveDir.y < 0)
                {
                    isEnabled = !isEnabled;
                }
                if (!goalsHaveUpdated)
                {
                    goals.leftHand = isEnabled;
                    goals.rightHand = isEnabled;
                    goals.leftFoot = isEnabled;
                    goals.rightFoot = isEnabled;

                    goalsHaveUpdated = true;
                }
                else
                {
                    goals.leftHand = isEnabled;
                    goals.rightHand = !isEnabled;
                    goals.leftFoot = isEnabled;
                    goals.rightFoot = !isEnabled;
                }               
            }
        }

        private void HandleCimbAnimations(Vector3 moveDir , bool isMidTransition)
        {
            if (isMidTransition)
            {
                if(moveDir.y != 0)
                {
                    if(moveDir.x == 0)
                    {
                        isMirroringAnimation = !isMirroringAnimation;
                    }
                    else
                    {
                        if (moveDir.y < 0)
                        {
                            isMirroringAnimation = (moveDir.x > 0);

                        }
                        else
                        {
                            isMirroringAnimation = (moveDir.x < 0);
                        }
                    }                 
                }
            }           
        }

        public IKSnapshot CreateIKsnapshot(Vector3 originPoint)
        {
            IKSnapshot thisIKsnapshot = new IKSnapshot();

            Vector3 _rightHand = convertLocalToWorldPos(ikBase.rightHand);
            thisIKsnapshot.rightHand = getPositionActual(_rightHand, AvatarIKGoal.RightHand);

            Vector3 _leftHand = convertLocalToWorldPos(ikBase.leftHand);
            thisIKsnapshot.leftHand = getPositionActual(_leftHand, AvatarIKGoal.LeftHand);

            Vector3 _rightFoot = convertLocalToWorldPos(ikBase.rightFoot);
            thisIKsnapshot.rightFoot = getPositionActual(_rightFoot, AvatarIKGoal.RightFoot);

            Vector3 _leftFoot = convertLocalToWorldPos(ikBase.leftFoot);
            thisIKsnapshot.leftFoot = getPositionActual(_leftFoot, AvatarIKGoal.LeftFoot);

            return thisIKsnapshot;
        }

        private Vector3 getPositionActual(Vector3 origin, AvatarIKGoal goal)
        {
            Vector3 value = origin;
            Vector3 thisOrigin = origin;
            Vector3 direction = climbHelper.forward;

            thisOrigin += -(direction * 0.2f);
            RaycastHit hit;

            bool hasHit = false;
            if(Physics.Raycast(thisOrigin, direction, out hit, 1.5f, whatIsClimbable))
            {
                Vector3 _value = hit.point + (hit.normal * wallOffset);
                value = _value;
                hasHit = true;

                if(goal == AvatarIKGoal.LeftFoot || goal == AvatarIKGoal.RightFoot)
                {
                    if(hit.point.y > transform.position.y - 0.1f)
                    {
                        hasHit = false;
                    }
                }               
            }

            if (!hasHit)
            {
                switch (goal)
                {
                    case AvatarIKGoal.LeftFoot:
                        value = convertLocalToWorldPos(ikBase.leftFoot);
                        break;
                    case AvatarIKGoal.RightFoot:
                        value = convertLocalToWorldPos(ikBase.rightFoot);
                        break;
                    case AvatarIKGoal.LeftHand:
                        value = convertLocalToWorldPos(ikBase.leftHand);
                        break;
                    case AvatarIKGoal.RightHand:
                        value = convertLocalToWorldPos(ikBase.rightHand);
                        break;
                    default:
                        break;
                }

            }

            return value;
        }

        private Vector3 convertLocalToWorldPos(Vector3 position)
        {
            Vector3 currentHelperPos = climbHelper.position;
            currentHelperPos += climbHelper.right * position.x;
            currentHelperPos += climbHelper.forward * position.z;
            currentHelperPos += climbHelper.up * position.y;

            return currentHelperPos;
        }

        public void copySnapshot(ref IKSnapshot to, IKSnapshot from)
        {
            to.rightHand = from.rightHand;
            to.leftHand = from.leftHand;
            to.rightFoot = from.rightFoot;
            to.leftFoot = from.leftFoot;
        }

        private void setIKpositions(bool isMidTransition, bool isTrue, Vector3 position, AvatarIKGoal thisGoal)
        {
            if (isMidTransition)
            {
                Vector3 newPos = getPositionActual(position, thisGoal);

                if (isTrue)
                {
                    updateIKposition(thisGoal, newPos);
                }
                else
                {
                    if(thisGoal == AvatarIKGoal.LeftFoot || thisGoal == AvatarIKGoal.RightFoot)
                    {
                        if(position.y > transform.position.y - -0.5f)
                        {
                           // updateIKposition(thisGoal, position);
                        }
                    }
                }
            }
            else
            {
                if (!isTrue)
                {
                    Vector3 newPos = getPositionActual(position, thisGoal);
                    updateIKposition(thisGoal, newPos);
                }
            }
        }

        private void OnAnimatorIK()
        {
            delta = Time.deltaTime;

            setAnimatorIKpositions(AvatarIKGoal.LeftHand, leftHand, leftHand_Weight);
            setAnimatorIKpositions(AvatarIKGoal.RightHand, rightHand, rightHand_Weight);
            setAnimatorIKpositions(AvatarIKGoal.LeftFoot, leftFoot, leftFoot_Weight);
            setAnimatorIKpositions(AvatarIKGoal.RightFoot, rightFoot, rightFoot_Weight);
        }

        private void setAnimatorIKpositions(AvatarIKGoal goal, Vector3 targetPos, float weight)
        {
            IKstates thisIKstate = getIKstates(goal);
            if(thisIKstate == null)
            {
                thisIKstate = new IKstates();
                thisIKstate.goal = goal;
                IKstatesList.Add(thisIKstate);
            }

            if(weight == 0)
            {
                thisIKstate.isSet = false;
            }

            if (!thisIKstate.isSet)
            {
                thisIKstate.position = getGoalToBodyBones(goal).position;
                thisIKstate.isSet = true;
            }

            thisIKstate.positionWeight = weight;
            thisIKstate.position = Vector3.Lerp(thisIKstate.position, targetPos, delta * climbLerpSpeed);

            thisAnimator.SetIKPositionWeight(goal, thisIKstate.positionWeight);
            thisAnimator.SetIKPosition(goal, thisIKstate.position);
        }

        private Transform getGoalToBodyBones(AvatarIKGoal goal)
        {
            switch (goal)
            {
                case AvatarIKGoal.LeftFoot:
                    return thisAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
                case AvatarIKGoal.RightFoot:
                    return thisAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
                case AvatarIKGoal.LeftHand:
                    return thisAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
                default:
                case AvatarIKGoal.RightHand:
                    return thisAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            }
        }

        public void updateIKposition(AvatarIKGoal goal, Vector3 targetPos)
        {
            switch (goal)
            {
                case AvatarIKGoal.LeftFoot:
                    leftFoot = targetPos;
                    break;
                case AvatarIKGoal.RightFoot:
                    rightFoot = targetPos;
                    break;
                case AvatarIKGoal.LeftHand:
                    leftHand = targetPos;
                    break;
                case AvatarIKGoal.RightHand:
                    rightHand = targetPos;
                    break;
                default:
                    break;
            }
        }

        public void updateIKWeight(AvatarIKGoal goal, float targetWeight)
        {
            switch (goal)
            {
                case AvatarIKGoal.LeftFoot:
                    leftFoot_Weight = targetWeight;
                    break;
                case AvatarIKGoal.RightFoot:
                    rightFoot_Weight = targetWeight;
                    break;
                case AvatarIKGoal.LeftHand:
                    leftHand_Weight = targetWeight;
                    break;
                case AvatarIKGoal.RightHand:
                    rightHand_Weight = targetWeight;
                    break;
                default:
                    break;
            }
        }

        List<IKstates> IKstatesList = new List<IKstates>();

        private IKstates getIKstates(AvatarIKGoal goal)
        {
            IKstates thisIKstates = null;

            foreach(IKstates current in IKstatesList)
            {
                if(current.goal == goal)
                {
                    thisIKstates = current;
                    break;
                }
            }

            return thisIKstates;
        }

        class IKstates
        {
            public AvatarIKGoal goal;
            public Vector3 position;
            public float positionWeight;
            public bool isSet = false;
        }       
    }

    public class IKGoals
    {
        public bool rightHand;
        public bool leftHand;
        public bool rightFoot;
        public bool leftFoot;
    }
    #endregion
}

