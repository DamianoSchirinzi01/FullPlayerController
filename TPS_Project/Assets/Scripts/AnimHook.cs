using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    public class AnimHook : MonoBehaviour
    {
        private Animator thisAnimator;

        public string InputX = "InputX";
        public string InputY = "InputY";

        public string isAiming = "isAiming";
        public string moveSpeed = "moveSpeed";

        public float lerpTime;

        [Header("Climbing")]
        public float rightHand_Weight;
        public float leftHand_Weight;
        public float rightFoot_Weight;
        public float leftFoot_Weight;

        public float wallOffset;

        IKSnapshot ikBase;
        IKSnapshot currentIKsnapshot = new IKSnapshot();
        IKSnapshot nextIKsnapshot = new IKSnapshot();

        private Vector3 rightHand, leftHand, rightFoot, leftFoot;
        private Transform climbHelper;

        // Start is called before the first frame update
        void Awake()
        {
            thisAnimator = GetComponent<Animator>();
        }

        public void Init(FreeClimb freeClimb, Transform thisClimbHelper)
        {
            ikBase = freeClimb.baseIKsnapshot;
            climbHelper = thisClimbHelper;
        }      

        public void createIKpositions(Vector3 originPoint)
        {
            IKSnapshot thisIK = CreateIKsnapshot(originPoint);
            copySnapshot(ref currentIKsnapshot, thisIK);

            updateIKposition(AvatarIKGoal.LeftFoot, currentIKsnapshot.leftFoot);
            updateIKposition(AvatarIKGoal.RightFoot, currentIKsnapshot.rightFoot);
            updateIKposition(AvatarIKGoal.LeftHand, currentIKsnapshot.leftHand);
            updateIKposition(AvatarIKGoal.RightHand, currentIKsnapshot.rightHand);

            updateIKWeight(AvatarIKGoal.LeftFoot, 1);
            updateIKWeight(AvatarIKGoal.RightFoot, 1);
            updateIKWeight(AvatarIKGoal.LeftHand, 1);
            updateIKWeight(AvatarIKGoal.RightHand, 1);
        }

        public IKSnapshot CreateIKsnapshot(Vector3 originPoint)
        {
            IKSnapshot thisIKsnapshot = new IKSnapshot();

            Vector3 _rightHand = convertLocalToWorldPos(ikBase.rightHand);
            thisIKsnapshot.rightHand = getPositionActual(_rightHand);

            Vector3 _leftHand = convertLocalToWorldPos(ikBase.leftHand);
            thisIKsnapshot.leftHand = getPositionActual(_leftHand);

            Vector3 _rightFoot = convertLocalToWorldPos(ikBase.rightFoot);
            thisIKsnapshot.rightFoot = getPositionActual(_rightFoot);

            Vector3 _leftFoot = convertLocalToWorldPos(ikBase.leftFoot);
            thisIKsnapshot.leftFoot = getPositionActual(_leftFoot);

            return thisIKsnapshot;
        }

        private Vector3 getPositionActual(Vector3 origin)
        {
            Vector3 value = origin;
            Vector3 thisOrigin = origin;
            Vector3 direction = climbHelper.forward;
            RaycastHit hit;

            thisOrigin += -(direction * 0.2f);

            if(Physics.Raycast(thisOrigin, direction, out hit, 1.5f))
            {
                Vector3 _value = hit.point + (hit.normal * wallOffset);
                value = _value;
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
        private void OnAnimatorIK()
        {
            setIKpositions(AvatarIKGoal.LeftHand, leftHand, leftHand_Weight);
            setIKpositions(AvatarIKGoal.RightHand, rightHand, rightHand_Weight);
            setIKpositions(AvatarIKGoal.LeftFoot, leftFoot, leftFoot_Weight);
            setIKpositions(AvatarIKGoal.RightFoot, rightFoot, rightFoot_Weight);
        }

        private void setIKpositions(AvatarIKGoal goal, Vector3 targetPos, float weight)
        {
            thisAnimator.SetIKPositionWeight(goal, weight);
            thisAnimator.SetIKPosition(goal, targetPos);
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
        public void updateLocomotion(float _inputX, float _inputY, float _speed, bool _isAiming)
        {
            float XinputLerped = Mathf.Lerp(thisAnimator.GetFloat(InputX), _inputX, lerpTime * Time.deltaTime);
            float YinputLerped = Mathf.Lerp(thisAnimator.GetFloat(InputY), _inputY, lerpTime * Time.deltaTime);
            float speedLerped = Mathf.Lerp(thisAnimator.GetFloat(moveSpeed), _speed, lerpTime * Time.deltaTime);

            thisAnimator.SetBool(isAiming, _isAiming);
            thisAnimator.SetFloat(moveSpeed, speedLerped);

            thisAnimator.SetFloat(InputX, XinputLerped);
            thisAnimator.SetFloat(InputY, YinputLerped);
        }
    }
}

