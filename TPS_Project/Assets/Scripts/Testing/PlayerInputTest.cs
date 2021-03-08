using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerInputTest : MonoBehaviour
{
    private PlayerMovement thisPlayerMovement;
    private DS.AnimHook thisAnimHook;

    public float moveX, moveY;
    public float mouseX, mouseY;
    public bool aiming, jumping, sprinting, crouching;
    private float sensitivity = 50f;
    private float sensMultiplier = 1f;

    public CinemachineFreeLook standardCam;
    public CinemachineVirtualCamera aimCam;

    private void Awake()
    {
        thisPlayerMovement = GetComponent<PlayerMovement>();
        thisAnimHook = GetComponentInChildren<DS.AnimHook>();
    }

    private void Update()
    {
        MyInput();
        setCamera(aiming);

        //thisAnimHook.updateLocomotion(moveX, moveY, thisPlayerMovement.currentVelocity, aiming);
    }

    private void MyInput()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");

        mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        jumping = Input.GetButton("Jump");
        sprinting = Input.GetButton("Sprinting");
        crouching = Input.GetKey(KeyCode.LeftControl);
        aiming = Input.GetMouseButton(1);

        if (aiming)
            sprinting = false;
        //Crouching
        if (Input.GetKeyDown(KeyCode.LeftControl))
            thisPlayerMovement.StartCrouch();
        if (Input.GetKeyUp(KeyCode.LeftControl))
            thisPlayerMovement.StopCrouch();
    }

    private void setCamera(bool _isAiming)
    {
        if (_isAiming)
        {
            aimCam.m_Priority = 25;
            standardCam.m_Priority = 10;
        }
        else
        {
            aimCam.m_Priority = 10;
            standardCam.m_Priority = 25;
        }
    }
}
