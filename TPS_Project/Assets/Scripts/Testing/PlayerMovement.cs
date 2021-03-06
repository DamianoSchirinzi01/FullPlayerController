// Some stupid rigidbody based movement by Dani

using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    //Assingables
    //public Transform playerCam;
    public Transform orientation;
    public Camera mainCam;

    //Other
    private PlayerInputTest input;
    private Rigidbody rb;

    //Rotation and look
    private float targetAngle;
    public float turnSpeed;
    private float rotationVelocity;
    public float rotationLerp;
    private Vector3 nextPosition;
    private Quaternion nextRotation;
    private float xRotation;

    //Movement
    private Vector3 desiredMoveDirection;
    public float currentVelocity;
    public float moveSpeed = 4500;
    public float maxSpeed = 20;
    public bool grounded;
    public LayerMask whatIsGround;
    
    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    //Crouch & Slide
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;

    //Jumping
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 550f;    
  
    //Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<PlayerInputTest>();
    }
    
    void Start() {
        playerScale =  transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
        
    private void FixedUpdate() {
        Movement();
    }

    private void LateUpdate()
    {
        if (input.aiming)
        {
            AimingLookRotation();
        }
        else
        {
            FreeLookRotation();
        }
    }

    /// <summary>
    /// Find user input. Should put this in its own class but im lazy
    /// </summary>
   

    public void StartCrouch() {
        transform.localScale = crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        if (rb.velocity.magnitude > 0.5f) {
            if (grounded) {
                rb.AddForce(orientation.transform.forward * slideForce);
            }
        }
    }

    public void StopCrouch() {
        transform.localScale = playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }

    private void Movement() {
        currentVelocity = rb.velocity.magnitude;
        //Extra gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);
        
        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(input.moveX, input.moveY, mag);
        
        //If holding jump && ready to jump, then jump
        if (readyToJump && input.jumping) Jump();

        //Set max speed
        float maxSpeed = this.maxSpeed;

        if (input.sprinting)
        {
            this.maxSpeed = 10f;
        }
        else
        {
            this.maxSpeed = 6f;
        }


        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (input.crouching && grounded && readyToJump) {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }
        
        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (input.moveX > 0 && xMag > maxSpeed) input.moveX = 0;
        if (input.moveX < 0 && xMag < -maxSpeed) input.moveX = 0;
        if (input.moveY > 0 && yMag > maxSpeed) input.moveY = 0;
        if (input.moveY < 0 && yMag < -maxSpeed) input.moveY = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;
       
        // Movement in air
        if (!grounded) {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }
        
        // Movement while sliding
        if (grounded && input.crouching) multiplierV = 0f;

        //Apply forces to move player
        rb.AddForce(transform.forward * input.moveY * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(transform.right * input.moveX * moveSpeed * Time.deltaTime * multiplier);
    }

    private void Jump() {
        if (grounded && readyToJump) {
            readyToJump = false;

            //Add jump forces
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);
            
            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0) 
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
            
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    
    private void ResetJump() {
        readyToJump = true;
    }
    
    private float desiredX;

    private void AimingLookRotation() {         
        orientation.transform.rotation *= Quaternion.AngleAxis(input.mouseX, Vector3.up);
        orientation.transform.rotation *= Quaternion.AngleAxis(input.mouseY, Vector3.right);

        var angles = orientation.transform.localEulerAngles;
        angles.z = 0;

        var angle = orientation.transform.localEulerAngles.x;

        if(angle > 340 && angle < 355)
        {
            angles.x = 355;
        }
        else if(angle < 180 && angle > 20)
        {
            angles.x = 20;
        }
        orientation.transform.localEulerAngles = angles;

        transform.rotation = Quaternion.Euler(0, orientation.transform.rotation.eulerAngles.y, 0);
        orientation.transform.localEulerAngles = new Vector3(angles.x, 0, 0);       
    }   

    private void FreeLookRotation()
    {
        if(input.mouseX > 0.1f || input.mouseY > 0.1f)
        {
            targetAngle = Mathf.Atan2(input.mouseX, input.mouseY) * Mathf.Rad2Deg + mainCam.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, turnSpeed);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }   
    }

    private void CounterMovement(float x, float y, Vector2 mag) {
        if (!grounded || input.jumping) return;

        //Slow down sliding
        if (input.crouching) {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0)) {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0)) {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }
        
        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed) {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    public Vector2 FindVelRelativeToLook() {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);
        
        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v) {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;
    
    /// <summary>
    /// Handle ground detection
    /// </summary>
    private void OnCollisionStay(Collision other) {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++) {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal)) {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded) {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded() {
        grounded = false;
    }
    
}
