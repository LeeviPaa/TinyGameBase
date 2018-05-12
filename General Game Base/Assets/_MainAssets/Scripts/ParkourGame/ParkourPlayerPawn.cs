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

    //gravity
    public float gravityAcceleration = 10;
    public float maxFallSpeed = 10;
    private float gravity = 1;

    //movement
    public float aerialSpeedMod = 0.4f;
    public float speed = 10;
    public float rotationSpeed = 10;
    private CollisionFlags prevColl;

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
        if(currPlayerState == PlayerState.CLIMBING) { return; }

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
        t.text = currPlayerState.ToString() + "\n" + "Prev re-jump: " + currReJumps.ToString();
    }
    private void LateUpdate()
    {
        prevPlayerState = currPlayerState;
    }
    #endregion

    #region Checks and calculations
    private void CheckCollisions()
    {
        switch(prevColl)
        {
            case CollisionFlags.Above:
                currJumpSpeed = 0;
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

            if (currVelocity.y < 1f)
            {
                currPlayerState = PlayerState.FALLING;
            }
            else if (currVelocity.y > 0)
            {
                currPlayerState = PlayerState.JUMPING;
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
            }
            else
            {
                landedThisFrame = false;
            }
        }
    }
    private void CalculateAccelerations()
    {
        if (!pc.isGrounded)
        {
            if (Mathf.Abs(gravity) < maxFallSpeed)
                gravity += gravityAcceleration * Time.deltaTime;
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
        //Jumping   
        if (Input.GetButtonDown("Jump"))
        {
            if (currPlayerState != PlayerState.JUMPING && currPlayerState != PlayerState.FALLING)
            {
                Jump();
            }
            else if(!bridgeToReJumpCooldown)
            {
                StartCoroutine(BridgeToReJump());
            }
        }

        //Movement
        currVelocity = (transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal"));
        currVelocity *= speed;

        //Movement in air is impaired
        if (!IsGrounded())
        {
            currVelocity *= aerialSpeedMod;
        }

        //Movement + jumping = vector3
        currVelocity += Vector3.up * currJumpSpeed;
        currVelocity -= Vector3.up * gravity;

        
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
            canReJump = false;
        }
        else
        {
            currJumpSpeed = jumpSpeed;
            prevReJumpSuccess = false;
            currReJumps = 0;
        }
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
    IEnumerator ClimbALedge(Vector3 startPos, Vector3 midway, Vector3 destination)
    {
        float target = 1;
        float position = 0;
        float climbSpeed = 1;

        while(position < target && Input.GetButton("Jump"))
        {
            currPlayerState = PlayerState.CLIMBING;

            Vector3 lerpAB = Vector3.Lerp(startPos, midway, position);
            Vector3 lerpBC = Vector3.Lerp(midway, destination, position);

            transform.position = Vector3.Lerp(lerpAB, lerpBC, position);
            position += Time.deltaTime*climbSpeed;
            yield return null;
        }
        currPlayerState = PlayerState.FALLING;
        
    }
    IEnumerator ReJumpTimer()
    {
        yield return new WaitForSeconds(reJumpTimeframe);
        canReJump = false;
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
}
