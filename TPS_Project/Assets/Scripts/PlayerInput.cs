﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace DS
{
    public class PlayerInput : MonoBehaviour
    {
        private PlayerController thisPlayer;
        private AnimHook thisAnimHook;

        [HideInInspector] public Vector3 rawDirection;
        [HideInInspector] public float horizontal, vertical;
        [HideInInspector] public float mouseX, mouseY;

        public bool isAiming, isCrouching, isJumping, isSprinting, isGrounded, isSliding, isClimbing;
        private bool canSprint = true;

        public float mouseSensitivity;

        public CinemachineVirtualCamera climbCam;
        public CinemachineVirtualCamera aimCam;
        public CinemachineFreeLook freeCam;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            thisPlayer = GetComponent<PlayerController>();
            thisAnimHook = GetComponentInChildren<AnimHook>();
        }

        private void Update()
        {
            captureInput();
            toggleCameras();
            checkSomeStates();
            cancelOutCurrentAction();

            thisAnimHook.updateAnimatorStates(horizontal, vertical, thisPlayer.currentSpeed, isAiming, isJumping, isGrounded, isSliding, isClimbing);
        }

        private void captureInput()
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

            mouseX = Input.GetAxisRaw("Mouse X") * (Time.fixedDeltaTime * mouseSensitivity);
            mouseY = Input.GetAxisRaw("Mouse Y") * (Time.fixedDeltaTime * mouseSensitivity);

            if(!isClimbing) isAiming = Input.GetMouseButton(1);
            if (canSprint && !isAiming) isSprinting = Input.GetButton("Sprinting");
            isJumping = Input.GetButtonDown("Jump");
            if (Input.GetKeyDown(KeyCode.C)) { toggleCrouch(); };

            rawDirection = new Vector3(horizontal, 0, vertical).normalized;
        }

        private void checkSomeStates()
        {
            isGrounded = thisPlayer.isGrounded;
            isSliding = thisPlayer.isSliding;
            isClimbing = thisPlayer.isClimbing;

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
            }

            if (isJumping)
            {
                isCrouching = false;
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
            if (isAiming)
            {
                aimCam.m_Priority = 25;
                freeCam.m_Priority = 8;
                climbCam.m_Priority = 8;
            }

            if (!isAiming)
            {
                freeCam.m_Priority = 25;
                aimCam.m_Priority = 8;
                climbCam.m_Priority = 8;
            }

            if(!isAiming && isClimbing)
            {
                climbCam.m_Priority = 25;
                freeCam.m_Priority = 8;
                aimCam.m_Priority = 8;
            }
        }
    }
}

