using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerState
{
    GROUNDED,
    JUMPING,
    FALLING,
    CLIMBING,
    WALLRUNNING
}

[RequireComponent(typeof(CharacterController))]
public class ParkourPlayerPawn : Pawn {
    CharacterController pc;
    private Vector3 currVelocity = Vector3.zero;

    //Jumping
    public float jumpSpeed = 10;
    private float currJumpSpeed = 0;
    public float reJumpTimeframe = 0.1f;
    public float reJumpIncrement = 1.5f;
    public int reJumpMaxCount = 2;
    private int currReJumps = 0;
    private bool canReJump = false;
    private bool prevReJumpSuccess = false;
    private Vector3 velocityOnJumpStart = Vector3.zero;
    public float holdToJumpLongerTime = 0.5f;

    //gravity
    public float gravityAcceleration = 10;
    public float maxFallSpeed = 10;
    private float gravity = 1;

    //movement
    public float aerialSpeedMod = 0.4f;
    public float speed = 10;
    public float rotationSpeed = 10;
    private CollisionFlags prevColl;
    public float sprintSpeedMod = 2;
    public float reverseSpeedMod = 0.6f;
    public float sidestepSpeedMod = 0.75f;

    //wallrunning
    public float wallrunSpeed = 17;
    public float wallrunningGravity = 10;
    public float minVelocityToWallrun = 10;
    public float jumpFromWallrunTime = 0.1f;
    public float wallrunJumpMultiplier = 2;
    public float wallrunLungeMultiplier = 3;
    private bool canWallrun = false;
    private bool wallrunJumpGoing = false;
    private bool cantWallrunBeforeReset = false;
    private bool wallrunJumped = false;

    //climbing 
    public float climbForwardDistance = 0.5f;

    //private
    private Transform playerCamera;
    PlayerState currPlayerState = PlayerState.GROUNDED;
    PlayerState prevPlayerState;
    private bool landedThisFrame = false;

    //public
    public Text t;

    #region Monobehaviour
    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>().transform;
        pc = GetComponent<CharacterController>();
        Cursor.visible = false;
    }
    private void Update()
    {
        //if this is not mine, don't do anything!
        if (!pv.isMine) { return; }

        //always can rotate camera and get input
        CheckInputForNonUpdate();

        if (!Cursor.visible)
            playerCamera.parent.Rotate(new Vector3(-Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime, 0));

        //if we are climbing, don't do anything
        if (currPlayerState == PlayerState.CLIMBING) { return; }
        //if we are wallrunning successfully, don't do anything
        if(canWallrun && Wallrun()){ return; }

        DoubleCheckGrounded();
        CheckCollisions();
        CalculateAccelerations();
        CalculateVelocity();

        if (!Cursor.visible)
            transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime, 0));

        if (Input.GetMouseButtonDown(2))
        {
            Cursor.visible = !Cursor.visible;
        }

        prevColl = pc.Move(currVelocity * Time.deltaTime);
    }
    private void LateUpdate()
    {
        prevPlayerState = currPlayerState;

        //debug text
        t.text = currPlayerState.ToString() + "\n" + "Prev re-jump: " + currReJumps.ToString();
        Debug.Log(Input.GetAxis("HorizontalWallJump"));
    }
    private void CheckInputForNonUpdate()
    {
        getButtonDownJump = Input.GetButtonDown("Jump");
        getButtonJump = Input.GetButton("Jump");
        getbuttonUpJump = Input.GetButtonUp("Jump");

    }
    #endregion

    #region Checks and calculations
    private void CheckCollisions()
    {
        switch(prevColl)
        {
            case CollisionFlags.Above:
                currJumpSpeed = 0;
                hitCelingWhileHoldingJump = true;
                break;
            case CollisionFlags.Below:
                //currPlayerState = PlayerState.GROUNDED;
                break;
            case CollisionFlags.Sides:
                break;
            case CollisionFlags.None:
                break;
            default:
                break;
        }

        if (!IsGrounded())
        {
            landedThisFrame = false;

            if(prevPlayerState == PlayerState.GROUNDED)
            {
                //detatched from ground 
                velocityOnJumpStart = currVelocity;
            }

            if (currPlayerState != PlayerState.WALLRUNNING && currPlayerState != PlayerState.CLIMBING)
            {
                if (currVelocity.y < 1f)
                {
                    currPlayerState = PlayerState.FALLING;
                }
                else if (currVelocity.y > 0)
                {
                    currPlayerState = PlayerState.JUMPING;
                }
            }

        }
        else
        {
            currPlayerState = PlayerState.GROUNDED;

            //if we just hit the ground
            if (prevPlayerState == PlayerState.FALLING)
            {
                StartCoroutine(ReJumpTimer());
                canReJump = true;
                landedThisFrame = true;
                cantWallrunBeforeReset = false;
                velocityOnJumpStart = Vector3.zero;
            }
            else
            {
                landedThisFrame = false;
            }
        }
    }
    private void CalculateAccelerations()
    {
        if (!pc.isGrounded && currPlayerState != PlayerState.CLIMBING)
        {
            if (currPlayerState != PlayerState.WALLRUNNING)
            {
                if (Mathf.Abs(gravity) < maxFallSpeed)
                    gravity += gravityAcceleration * Time.deltaTime;
            }
            else
            {
                gravity = wallrunningGravity;
            }
        }
        else
        {
            gravity = 0;
        }

        if (currJumpSpeed > 0)
        {
            currJumpSpeed -= gravityAcceleration * Time.deltaTime;
        }
        else
        {
            currJumpSpeed = 0;
        }
    }
    private bool Wallrun()
    {
        if (currWallrunWall != null && Input.GetButton("Jump") && !wallrunJumpGoing)
        {
            currPlayerState = PlayerState.WALLRUNNING;
            wallrunJumped = false;

            if (currJumpSpeed > 0)
            {
                currJumpSpeed -= gravityAcceleration * Time.deltaTime * 2;
            }
            else
            {
                currJumpSpeed = 0;
            }

            Plane P = new Plane(currWallrunWall.transform.up, currWallrunWall.transform.position);

            Vector3 moveVector = (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")) * wallrunSpeed * Time.deltaTime;
            Vector3 point = transform.position + moveVector;

            float delta = P.GetDistanceToPoint(point);

            moveVector -= P.normal * delta;
            moveVector -= Vector3.up * wallrunningGravity*Time.deltaTime;
            moveVector += Vector3.up * currJumpSpeed * Time.deltaTime;
            currVelocity = moveVector;

            pc.Move(moveVector);

            if (!Cursor.visible)
            {
                transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime, 0));
            }

            return true;
        }
        else if (Input.GetButtonUp("Jump"))
        {
            Debug.LogWarning("Stopped wallrunning");
            StartCoroutine(WallrunJump());
            wallrunJumpGoing = true;
            return true;
        }
        else if (wallrunJumpGoing && !cantWallrunBeforeReset)
        {
            pc.Move(currVelocity);
            return true;
        }
        else
        {
            if(currPlayerState == PlayerState.WALLRUNNING)
                currPlayerState = PlayerState.FALLING;

            return false;
        }
    }
    private void CalculateVelocity()
    {
        //Movement normally when grounded(+the time of the re jump delay). 
        if (currPlayerState == PlayerState.GROUNDED || canReJump || currPlayerState == PlayerState.WALLRUNNING)
        {
            //Movement

            Vector3 vertical = transform.forward * Input.GetAxis("Vertical");
            if(Input.GetAxis("Vertical") < 0)
            {
                vertical *= reverseSpeedMod;
            }

            Vector3 horizontal = transform.right * Input.GetAxis("Horizontal") * sidestepSpeedMod;
            //sprinting
            if (Input.GetButton("Sprint"))
            {
                if ( Input.GetAxis("Vertical") > 0)
                    vertical *= sprintSpeedMod;

                horizontal *= sprintSpeedMod;
            }

            currVelocity = (vertical + horizontal);
            currVelocity *= speed; 
        }
        else if(currPlayerState == PlayerState.JUMPING || currPlayerState == PlayerState.FALLING) // Movement in air is impaired
        {
            currVelocity = new Vector3(velocityOnJumpStart.x, 0, velocityOnJumpStart.z);
            currVelocity += (transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal")) * speed * aerialSpeedMod;
        }

        //Jumping   
        if (Input.GetButtonDown("Jump"))
        {
            if (currPlayerState != PlayerState.JUMPING && currPlayerState != PlayerState.FALLING)
            {
                Jump(1);
            }
            else if (!bridgeToReJumpCooldown)
            {
                StartCoroutine(BridgeToReJump());
            }
        }

        //Movement + jumping = vector3
        if (currPlayerState == PlayerState.WALLRUNNING)
        {
            currVelocity -= Vector3.up * gravity;
        }
        else
        {
            currVelocity += Vector3.up * currJumpSpeed;
            currVelocity -= Vector3.up * gravity;
        }

        
    }
    private float aerialTime = 0;
    private const float const_maxAerialTime = 0.1f;
    void DoubleCheckGrounded()
    {
        if (pc.isGrounded)
        {
            aerialTime = 0;
        }
        else
        {
            aerialTime += Time.deltaTime;
        }
    }
    public bool IsGrounded()
    {
        return aerialTime < const_maxAerialTime;
    }
    #endregion

    #region actions
    bool reJumpedInTimeWindow = false;
    private void Jump(float jumpMultiplier)
    {
        if (canReJump)
        {
            if (currReJumps < reJumpMaxCount)
            {
                currReJumps++;
            }
            else
            {
                currReJumps = 0;
            }

            currJumpSpeed = jumpSpeed + jumpSpeed * reJumpIncrement * currReJumps * jumpMultiplier;
            prevReJumpSuccess = true;
            reJumpedInTimeWindow = true;
            canReJump = false;
        }
        else
        {
            currJumpSpeed = jumpSpeed * jumpMultiplier;
            StartCoroutine(HoldToJumpLonger(1));
            prevReJumpSuccess = false;
            currReJumps = 0;
        }

        currPlayerState = PlayerState.JUMPING;
        velocityOnJumpStart = currVelocity;
    }
    public void UpdateClimbableLedge(ClimbableLedge ledge)
    {
        Debug.Log("can climb!");
        if(Input.GetButton("Jump") && currPlayerState != PlayerState.CLIMBING)
        {
            float upOffset = pc.height / 2;
            float distYToLedgeY = Mathf.Abs(ledge.transform.position.y - transform.position.y) + 0.05f; ;

            currPlayerState = PlayerState.CLIMBING;
            StartCoroutine(ClimbALedge(transform.position,
                transform.position + new Vector3(0, upOffset + distYToLedgeY, 0), 
                transform.position + new Vector3(0, upOffset + distYToLedgeY, 0) + transform.forward * climbForwardDistance));
        }
    }
    public void FallOffMap()
    {
        SpawnPosition[] sp = FindObjectsOfType<SpawnPosition>();
        transform.position = sp[0].transform.position;
    }
    public void StartWallrunning(WallrunnableWall wall)
    {
        if (wall != currWallrunWall)
        {
            cantWallrunBeforeReset = false;
            wallrunJumpGoing = false;
        }

        Vector3 currvelocityWithoutY = pc.velocity;
        currWallrunWall = wall;
        currvelocityWithoutY.y = 0;
        
        if (getButtonJump && currvelocityWithoutY.magnitude > minVelocityToWallrun)
        {
            velocityOnJumpStart = Vector3.zero;
            canWallrun = true;
            gravity = 0;
        }
    }

    bool getButtonDownJump = false;
    bool getButtonJump = false;
    bool getbuttonUpJump = false;
    WallrunnableWall currWallrunWall;
    public void UpdateRunnableWall(WallrunnableWall wall)
    {
        if (wall != currWallrunWall)
            cantWallrunBeforeReset = false;

        currWallrunWall = wall;

        Vector3 currvelocityWithoutY = pc.velocity;
        currvelocityWithoutY.y = 0;

        if (currvelocityWithoutY.magnitude > minVelocityToWallrun && !cantWallrunBeforeReset)
        {
            canWallrun = true;
            currWallrunWall = wall;
        }
        else
        {
            canWallrun = false;
            cantWallrunBeforeReset = true;

            currPlayerState = PlayerState.FALLING;

            if(!wallrunJumped)
                velocityOnJumpStart = pc.velocity;
        }
    }
    public void ExitRunnableWall(WallrunnableWall wall)
    {
        canWallrun = false;
        currPlayerState = PlayerState.FALLING;

        if(!wallrunJumped)
            velocityOnJumpStart = pc.velocity;
    }
    IEnumerator WallrunJump()
    {
        float t = 0;
        while (t < jumpFromWallrunTime)
        {
            if(Input.GetButton("Jump") && wallrunJumpGoing)
            {
                Debug.LogWarning("execute jump!");

                //Jump(wallrunJumpMultiplier);
                StartCoroutine(HoldToJumpLonger(wallrunJumpMultiplier));
                velocityOnJumpStart = (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("HorizontalWallJump")) * speed * wallrunLungeMultiplier;
                wallrunJumpGoing = false;
                canWallrun = false;
                wallrunJumped = true;
                gravity = 0;
                currPlayerState = PlayerState.JUMPING;
                break;
            }
            t += Time.deltaTime;
            yield return null;
        }
        wallrunJumpGoing = false;
        cantWallrunBeforeReset = true;
    }
    IEnumerator ClimbALedge(Vector3 startPos, Vector3 midway, Vector3 destination)
    {
        float target = 1;
        float position = 0;
        float climbSpeed = 1;
        velocityOnJumpStart = Vector3.zero;
        currJumpSpeed = 0;

        while(position < target && Input.GetButton("Jump"))
        {
            currPlayerState = PlayerState.CLIMBING;

            Vector3 lerpAB = Vector3.Lerp(startPos, midway, position);
            Vector3 lerpBC = Vector3.Lerp(midway, destination, position);

            transform.position = Vector3.Lerp(lerpAB, lerpBC, position);
            position += Time.deltaTime*climbSpeed;
            velocityOnJumpStart = Vector3.zero;
            currJumpSpeed = 0;
            gravity = 0;
            yield return null;
        }
        currJumpSpeed = 0;
        currPlayerState = PlayerState.FALLING;
        
    }
    bool hitCelingWhileHoldingJump = false;
    IEnumerator HoldToJumpLonger(float jumpMultiplier)
    {
        hitCelingWhileHoldingJump = false;
        float time = 0;
        while(time < holdToJumpLongerTime 
            && Input.GetButton("Jump") 
            && !hitCelingWhileHoldingJump 
            && currPlayerState != PlayerState.CLIMBING)
        {
            time += Time.deltaTime;

            if (currPlayerState != PlayerState.WALLRUNNING && !canWallrun)
                currJumpSpeed = jumpSpeed * jumpMultiplier;

            yield return null;
        }
    }
    IEnumerator ReJumpTimer()
    {
        reJumpedInTimeWindow = false;
        float t = 0;
        while (t < reJumpTimeframe)
        {
            t += Time.deltaTime;
            velocityOnJumpStart = currVelocity;
            yield return null;
        }
        canReJump = false;
        if(!reJumpedInTimeWindow)
        {
            currReJumps = 0;
        }
    }
    bool bridgeToReJumpCooldown = false;
    IEnumerator BridgeToReJump()
    {
        bridgeToReJumpCooldown = true;
        float time = 0;
        while (time < reJumpTimeframe)
        {
            if(landedThisFrame && Input.GetButton("Jump"))
            {
                canReJump = true;
                Jump(1);
            }
            time += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(reJumpTimeframe);
        bridgeToReJumpCooldown = false;
    }
    #endregion

    /*
     * todo: 
     * Jumping from wallrun to a direction
     */
}
