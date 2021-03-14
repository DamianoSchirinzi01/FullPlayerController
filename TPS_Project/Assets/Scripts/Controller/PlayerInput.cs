using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace DS
{    public class PlayerInput : MonoBehaviour
    {
        private PlayerController thisPlayer;
        private WeaponManager thisWeaponManager;
        private AnimHook thisAnimHook;

        [HideInInspector] public Vector3 rawDirection;
        [HideInInspector] public float horizontal, vertical;
        [HideInInspector] public float mouseX, mouseY;

        public bool isAiming, isCrouching, isJumping, isSprinting, isGrounded, isSliding, isFiring, isClimbing, startedToClimbLedge;
        private bool canSprint = true;

        public float mouseSensitivity;

        public CinemachineVirtualCamera climbCam;
        public CinemachineVirtualCamera ledgeClimbCam;
        public CinemachineVirtualCamera aimCam;
        public CinemachineFreeLook freeCam;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            thisAnimHook = GetComponentInChildren<AnimHook>();

            thisPlayer = GetComponent<PlayerController>();
            thisWeaponManager = GetComponent<WeaponManager>();
        }

        private void Update()
        {
            captureInput();
            swapWeaponsInput();
            toggleCameras();
            checkSomeStates();
            cancelOutCurrentAction();

            thisAnimHook.updateAnimatorStates(horizontal, vertical, thisPlayer.currentSpeed, isAiming, isJumping, isCrouching , isGrounded, isSliding, isClimbing);
        }

        private void captureInput()
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

            if (isClimbing) mouseSensitivity = 50f;
            else { mouseSensitivity = 150f; }

            mouseX = Input.GetAxisRaw("Mouse X") * (Time.fixedDeltaTime * mouseSensitivity);
            mouseY = Input.GetAxisRaw("Mouse Y") * (Time.fixedDeltaTime * mouseSensitivity);

            if(!isClimbing && !isSliding) isAiming = Input.GetMouseButton(1);
            if (canSprint && !isAiming) isSprinting = Input.GetButton("Sprinting");
            isJumping = Input.GetButtonDown("Jump");
            if (Input.GetKeyDown(KeyCode.C)) { toggleCrouch(); };

            if(thisWeaponManager.currentHandState == handStates.GunEquipped_Aiming)
            {
                if (Input.GetButton("Fire1")) thisWeaponManager.startFiring();
                else if (Input.GetButtonUp("Fire1")) thisWeaponManager.stopFiring();
            }         

            rawDirection = new Vector3(horizontal, 0, vertical).normalized;
        }

        private void swapWeaponsInput()
        {
            int inputIndex;

            if (Input.GetKeyDown(KeyCode.Alpha1) && thisWeaponManager.currentWeaponIndex != 0)
            {
                Debug.Log("Calling");
                inputIndex = 0;
                thisAnimHook.startWeaponSwap(inputIndex); //Rifle
            }

            if (Input.GetKeyDown(KeyCode.Alpha2) && thisWeaponManager.currentWeaponIndex != 1)
            {               
                inputIndex = 1;
                thisAnimHook.startWeaponSwap(inputIndex); //Pistol
            }

            if (Input.GetKeyDown(KeyCode.Alpha3) && thisWeaponManager.currentWeaponIndex != 2)
            {
                inputIndex = 2;
                thisWeaponManager.setCurrentWeaponIndex(inputIndex);
                thisWeaponManager.stashAllWeapons();
            }
        }

        private void checkSomeStates()
        {
            isGrounded = thisPlayer.isGrounded;
            isSliding = thisPlayer.isSliding;
            isClimbing = thisPlayer.isClimbing;
            isFiring = thisWeaponManager.isFiring;

            if (thisPlayer.stopClimbing)
            {
                thisAnimHook.resetIK();
            }
        }

        private void cancelOutCurrentAction()
        {
            if (isCrouching)
            {
                if (isSprinting) { toggleCrouch(); };
                if (isJumping) { toggleCrouch(); };
            }

            if (isSprinting)
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    canSprint = false;
                    isSprinting = false;
                    isCrouching = false;

                    StartCoroutine(pauseSprinting());

                    toggleCrouch();
                }

                if (isAiming)
                {
                    isSprinting = false;
                }
            }

            if (isJumping)
            {
                isCrouching = false;
            }

            if (isClimbing)
            {
                isSprinting = false;
                isCrouching = false;

                thisWeaponManager.setCurrentWeaponIndex(2);
                thisWeaponManager.stashAllWeapons();
            }           
        }

        private void toggleCrouch()
        {
            if (isCrouching)
            {
                isCrouching = false;
                thisPlayer.StopCrouch();
            }
            else
            {
                isCrouching = true;
                thisPlayer.StartCrouch();
            }
        }

        private IEnumerator pauseSprinting()
        {
            yield return new WaitForSeconds(1f);

            canSprint = true;
        }

        private void toggleCameras() //Temp, should only call on state changed
        {
            if (isAiming) //Aiming cam
            {
                aimCam.m_Priority = 25;
                freeCam.m_Priority = 8;
                climbCam.m_Priority = 8;
                ledgeClimbCam.m_Priority = 8;
            }

            if (!isAiming) //Free look cam
            {
                freeCam.m_Priority = 25;
                aimCam.m_Priority = 8;
                climbCam.m_Priority = 8;
                ledgeClimbCam.m_Priority = 8;
            }

            if (isClimbing) //Climbing cam
            {
                climbCam.m_Priority = 25;
                ledgeClimbCam.m_Priority = 8;
                freeCam.m_Priority = 8;
                aimCam.m_Priority = 8;
            }           

            if(startedToClimbLedge) //ledge climbing cam
            {
                ledgeClimbCam.m_Priority = 25;
                climbCam.m_Priority = 8;
                freeCam.m_Priority = 8;
                aimCam.m_Priority = 8;
            }
        }      
    }
}

