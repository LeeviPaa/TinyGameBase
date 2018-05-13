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

    //wallrunning
    public float wallrunningGravity = 10;
    public float minVelocityToWallrun = 10;

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
        playerCamera = GetComponentInChildren<Camera>().transform.parent;
        pc = GetComponent<CharacterController>();
        Cursor.visible = false;
    }
    private void Update()
    {
        //if this is not mine, don't do anything!
        if (!pv.isMine) { return; }

        //always can rotate camera
        if(!Cursor.visible)
            playerCamera.Rotate(new Vector3(-Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime, 0));

        //if we are climbing, don't do anything
        if (currPlayerState == PlayerState.CLIMBING) { return; }

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
    private void CalculateVelocity()
    {
        //Movement normally when grounded(+the time of the re jump delay). 
        if (currPlayerState == PlayerState.GROUNDED || canReJump || currPlayerState == PlayerState.WALLRUNNING)
        {
            //Movement
            currVelocity = (transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal"));
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
                Jump();
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
    private void Jump()
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

            currJumpSpeed = jumpSpeed + jumpSpeed * reJumpIncrement * currReJumps;
            prevReJumpSuccess = true;
            reJumpedInTimeWindow = true;
            canReJump = false;
        }
        else
        {
            currJumpSpeed = jumpSpeed;
            StartCoroutine(HoldToJumpLonger());
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
            currPlayerState = PlayerState.CLIMBING;
            StartCoroutine(ClimbALedge(transform.position,transform.position + new Vector3(0, 2, 0), ledge.transform.position + new Vector3(0,2,0)));
        }
    }
    public void FallOffMap()
    {
        SpawnPosition[] sp = FindObjectsOfType<SpawnPosition>();
        transform.position = sp[0].transform.position;
    }
    public void StartWallrunning(WallrunnableWall wall)
    {
        Vector3 currvelocityWithoutY = pc.velocity;
        currvelocityWithoutY.y = 0;
        
        if (Input.GetButton("Jump") && currvelocityWithoutY.magnitude > minVelocityToWallrun)
        {
            currJumpSpeed = 0;
            velocityOnJumpStart = Vector3.zero;
        }
    }
    public void UpdateRunnableWall(WallrunnableWall wall)
    {
        Debug.Log("can Wallrun!");

        Vector3 currvelocityWithoutY = pc.velocity;
        currvelocityWithoutY.y = 0;

        if (Input.GetButton("Jump") && currvelocityWithoutY.magnitude > minVelocityToWallrun)
        {
            currPlayerState = PlayerState.WALLRUNNING;
        }
        else
        {
            currPlayerState = PlayerState.FALLING;
        }
    }
    public void ExitRunnableWall(WallrunnableWall wall)
    {
        currPlayerState = PlayerState.FALLING;
    }
    IEnumerator ClimbALedge(Vector3 startPos, Vector3 midway, Vector3 destination)
    {
        float target = 1;
        float position = 0;
        float climbSpeed = 1;
        velocityOnJumpStart = Vector3.zero;

        while(position < target && Input.GetButton("Jump"))
        {
            currPlayerState = PlayerState.CLIMBING;

            Vector3 lerpAB = Vector3.Lerp(startPos, midway, position);
            Vector3 lerpBC = Vector3.Lerp(midway, destination, position);

            transform.position = Vector3.Lerp(lerpAB, lerpBC, position);
            position += Time.deltaTime*climbSpeed;
            velocityOnJumpStart = Vector3.zero;
            yield return null;
        }
        currPlayerState = PlayerState.FALLING;
        
    }
    bool hitCelingWhileHoldingJump = false;
    IEnumerator HoldToJumpLonger()
    {
        hitCelingWhileHoldingJump = false;
        float time = 0;
        while(time < holdToJumpLongerTime 
            && Input.GetButton("Jump") 
            && !hitCelingWhileHoldingJump 
            && currPlayerState != PlayerState.CLIMBING 
            && currPlayerState != PlayerState.WALLRUNNING)
        {
            time += Time.deltaTime;
            currJumpSpeed = jumpSpeed;
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
                Jump();
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
