using System;
using System.Collections;
using UnityEngine;

namespace DS
{    public class PlayerController : MonoBehaviour
    {
        [Header("References")] 
        private PlayerInput input;
        private FreeClimb thisFreeClimb;
        private CharacterController thisCC;
        public Transform camera;
        public Transform orientation;

        [Header("Locomotion Values")]
        public float slopeLimit;
        public float gravity = -9.81f;
        public float sprintSpeed;
        public float standardSpeed;
        public float crouchSpeed;
        public float acceleration;
        public float decelerationMultiplier;
        public float currentSpeed;
        private float lerpTimeElapsed = 3f;
        public bool canMove { get; set; }
        private Vector3 moveDirection;
        Vector3 velocity;

        [Header("Rotation Values")]    
        private float targetRotation;
        public float freeAimRotationSpeed = .1f;
        private float turnSmoothVelocity;

        [Header("Crouching Values")]
        private Vector3 crouchScale = new Vector3(1, .5f, 1);
        private Vector3 playerScale;

        [Header("Jumping Values")]
        public bool justJumped;
        public Transform fallDirection;
        public float jumpCooldown = 1.5f;
        private float jumpCoolDownReset;
        public float jumpForce;

        [Header("Ground & Slope Check Values")]
        public Transform groundCheckOrigin;
        public float groundCheckRadius;
        public LayerMask whatIsGround;

        public float slideTurnSpeed;
        public float slopeGravityMultiplier;
        private float maxSlideVelocity = 1300f;
        public float currentSlideVelocity;
        public float slopeRayDistance;
        RaycastHit slopeHit;
        public bool isSliding;
        private bool isOnASlope;
        public bool currentSlopeIsTooSteep;
        public bool isGrounded;

        [Header("Climbing")]
        public bool isClimbing;
        public bool stopClimbing;

        // Start is called before the first frame update
        void Awake()
        {
            input = GetComponent<PlayerInput>();
            thisFreeClimb = GetComponent<FreeClimb>();
            thisCC = GetComponent<CharacterController>();
        }

        void Start()
        {
            canMove = true;

            playerScale = transform.localScale;
            jumpCoolDownReset = jumpCooldown;
        }

        void Update()
        {
            if (!canMove)
                return;

            if (isClimbing)
            {
                if (input.isJumping)
                {
                    thisFreeClimb.cancelClimb(true);
                }
                return;
            }          

            isGrounded = checkIsGrounded();           

            Jump();
            resetJump();
            setSpeed();
        }

        private void FixedUpdate()
        {
            if (!canMove)
                return;

            if (isClimbing)
            {          
                thisFreeClimb.checkClimbingState(Time.deltaTime);
                return;
            }

            if (!isClimbing)
            {
                if (!isGrounded && !stopClimbing)
                {
                    if (thisFreeClimb.CheckForClimbableWall())
                    {
                        enableClimbing();
                    }
                }
                else if (isGrounded && stopClimbing)
                {
                    stopClimbing = false;
                }
            }

            MovePlayer();

            if (input.isAiming)
            {
                AimingLookRotation();
            }
            else
            {
                freeLookRotation();
            }
        }

        private void MovePlayer()
        {
            velocity.y += gravity + Time.deltaTime;

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            targetRotation = Mathf.Atan2(input.rawDirection.x, input.rawDirection.z) * Mathf.Rad2Deg + camera.eulerAngles.y;

            checkShouldSlide();

            if (input.rawDirection.magnitude > 0.1f)
            {
                moveDirection = Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward;
            }

            thisCC.Move(moveDirection.normalized * currentSpeed * Time.fixedDeltaTime);
            thisCC.Move(velocity * Time.fixedDeltaTime);
        }

        private void Slide()
        {
            isSliding = true;
            thisCC.SimpleMove(-fallDirection.up * (currentSlideVelocity * Time.fixedDeltaTime));
            thisCC.Move(Vector3.down * thisCC.height / 2 * slopeGravityMultiplier * Time.deltaTime); //Applies gravity downwards whe on a ramp
            moveDirection += -fallDirection.up;           
        }

        private void setSpeed()
        {
            if (input.rawDirection.magnitude != 0)
            {
                if (input.isSprinting)
                {
                    currentSpeed = HelperFunctions.smoothLerp(currentSpeed, currentSpeed, sprintSpeed, acceleration);
                    if (currentSpeed >= sprintSpeed - 0.2f) { currentSpeed = sprintSpeed; }
                }
                else if (input.isCrouching)
                {
                    currentSpeed = HelperFunctions.smoothLerp(currentSpeed, currentSpeed, crouchSpeed, acceleration);
                }
                else if (!input.isSprinting && !input.isCrouching)
                {
                    currentSpeed = HelperFunctions.smoothLerp(currentSpeed, currentSpeed, standardSpeed, acceleration);
                }
            }
            else if (input.rawDirection.magnitude == 0)
            {
                currentSpeed = HelperFunctions.smoothLerp(currentSpeed, currentSpeed, 0, (acceleration * decelerationMultiplier));
                if (currentSpeed <= 0.2f) { currentSpeed = 0f; }
            }
        }

        private void AimingLookRotation()
        {
            if (input.mouseX != 0 || input.mouseY != 0f)
            {
                orientation.transform.rotation *= Quaternion.AngleAxis(input.mouseX, Vector3.up);
                orientation.transform.rotation *= Quaternion.AngleAxis(input.mouseY, Vector3.right);
            }

            var xOrientation = orientation.localEulerAngles.x;

            xOrientation = HelperFunctions.ClampAngle(xOrientation, -20, 20);

            orientation.localEulerAngles = orientation.localEulerAngles;

            transform.rotation = Quaternion.Euler(0, orientation.eulerAngles.y, 0);
            orientation.localEulerAngles = new Vector3(xOrientation, 0, 0);
        }

        private void freeLookRotation()
        {
            if (input.rawDirection.magnitude >= .1f && !isSliding)
            {
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, freeAimRotationSpeed);
                transform.rotation = Quaternion.Euler(0, angle, 0);
            }
        }

        #region Crouching
        public void StartCrouch()
        {
            Debug.Log("Crouch Started");
            //transform.localScale = crouchScale;
            //transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        }

        public void StopCrouch()
        {
            Debug.Log("Crouch Stopped");
            //transform.localScale = playerScale;
            //transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        }

        #endregion

        #region Jumping
        private void Jump()
        {
            if (!justJumped && !isClimbing)
            {
                if (input.isJumping && isGrounded && !currentSlopeIsTooSteep)
                {
                    justJumped = true;
                    velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                }
            }               
        }

        private void resetJump()
        {
            if (justJumped)
            {            
                jumpCooldown -= Time.deltaTime;

                if(jumpCooldown <= 0)
                {
                    justJumped = false;
                    jumpCooldown = jumpCoolDownReset;
                }
            }
            
        }

        //Add cooldown after jump

        #endregion

        #region Checks
        private bool checkIsGrounded()
        {
            bool checkGrounded = Physics.CheckSphere(groundCheckOrigin.position, groundCheckRadius, whatIsGround);

            if (checkGrounded)
            {
                currentSlopeIsTooSteep = isSlopeTooSteep();
            }
            else
            {
                currentSlopeIsTooSteep = false;
            }

            return checkGrounded;
        }

        private bool isSlopeTooSteep()
        {
            Physics.Raycast(groundCheckOrigin.position, Vector3.down * slopeRayDistance, out slopeHit, whatIsGround);
            Debug.DrawRay(groundCheckOrigin.position, Vector3.down * slopeRayDistance);

            if (Vector3.Angle(Vector3.up, slopeHit.normal) == 0)
            {
                currentSlideVelocity = 0;
                currentSlopeIsTooSteep = false;
                isOnASlope = false;

                //Debug.Log("Flat ground");
                return false;

            }
            else if (Vector3.Angle(Vector3.up, slopeHit.normal) >= 1f && Vector3.Angle(Vector3.up, slopeHit.normal) <= slopeLimit)
            {
                currentSlopeIsTooSteep = false;
                isOnASlope = true;
                //Debug.Log("On slope but not steep");
                return false;
            }
            else
            {
                currentSlopeIsTooSteep = true;
                isOnASlope = true;
                //Debug.Log("On steep slope");
                return true;
            }
        }

        private void checkShouldSlide()
        {
            if (isOnASlope)
            {
                if (currentSlopeIsTooSteep)
                {
                    fallDirection.rotation = setToGroundNormal();

                    if (!input.isCrouching)
                    {
                        if (currentSlideVelocity < maxSlideVelocity)
                        {
                            currentSlideVelocity += 350 * Time.deltaTime;
                        }
                    }
                    else if (input.isCrouching)
                    {
                        if (currentSlideVelocity < maxSlideVelocity)
                        {
                            currentSlideVelocity += 800 * Time.deltaTime;
                        }
                    }

                    Slide();
                }
                else
                {
                    if (input.isCrouching) //And player crouches                
                    {
                        //Player should begin sliding
                        fallDirection.rotation = setToGroundNormal();

                        if (currentSlideVelocity < maxSlideVelocity)
                        {
                            currentSlideVelocity += 800 * Time.deltaTime;
                        }

                        Slide();
                    }
                }
            }
            else
            {
                fallDirection.rotation = Quaternion.FromToRotation(Vector3.up, slopeHit.normal);

                if (isSliding) 
                {
                    input.isCrouching = false;
                    isSliding = false;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (isGrounded) Gizmos.color = Color.green;
            else Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(groundCheckOrigin.position, groundCheckRadius);
        }
        #endregion

        #region helper functions  
        private Quaternion setToGroundNormal()
        {
            Vector3 groundCross = Vector3.Cross(slopeHit.normal, Vector3.up);
            Quaternion newRot = Quaternion.FromToRotation(transform.up, Vector3.Cross(groundCross, slopeHit.normal));

            Vector3 targetRot = Quaternion.Euler(0, newRot.y, 0) * Vector3.right;
            Quaternion lookAtDir = Quaternion.LookRotation(-targetRot);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookAtDir, slideTurnSpeed * Time.deltaTime);

            return newRot;
        }

        public void enableClimbing()
        {
            isClimbing = true;
            
            thisCC.enabled = false;
        }

        public void disableClimbing()
        {
            isClimbing = false;

            canMove = true;
            stopClimbing = true;
            thisCC.enabled = true;
        }
        #endregion
    }
}

