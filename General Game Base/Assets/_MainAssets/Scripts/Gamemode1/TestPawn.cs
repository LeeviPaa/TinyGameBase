using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPawn : Pawn
{
    CharacterController pc;
    private Vector3 currVelocity = Vector3.zero;

    //Jumping
    public float jumpSpeed = 10;
    private float currJumpSpeed = 0;

    //gravity
    public float gravityAcceleration = 10;
    public float maxFallSpeed = 10;
    private float gravity = 1;

    //movement
    public float aerialSpeedMod = 0.4f;
    public float speed = 10;
    public float rotationSpeed = 10;

    private void Start()
    {
        pc = GetComponent<CharacterController>();
    }
    private void Update()
    {
        //if this is not mine, don't do anything!
        if(!pv.isMine){ return; }

        CalculateAccelerations();
        CalculateVelocity();

        transform.Rotate(new Vector3(0, Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime, 0));

        pc.Move(currVelocity* Time.deltaTime);
        
    }
    private void CalculateAccelerations()
    {
        if(!pc.isGrounded )
        {
            if(Mathf.Abs(gravity) < maxFallSpeed)
                gravity += gravityAcceleration * Time.deltaTime;
        }
        else
        {
            gravity = 0;
        }

        if(currJumpSpeed > 0)
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
        currVelocity = (transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Rotation"));
        currVelocity *= speed;
        currVelocity += Vector3.up * currJumpSpeed;

        if (!pc.isGrounded)
        {
            currVelocity -= Vector3.up * gravity;
            currVelocity *= aerialSpeedMod;
        }

        if(Input.GetButtonDown("Jump"))
        {
            currJumpSpeed = jumpSpeed;
        }
    }
}
