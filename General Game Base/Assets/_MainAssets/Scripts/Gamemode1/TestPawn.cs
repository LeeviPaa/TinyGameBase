using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPawn : Pawn
{
    private void Update()
    {
        //if this is not mine, don't do anything!
        if(!pv.isMine){ return; }

        MoveDirection((transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Rotation")));
        RotateYDirection(Input.GetAxis("Horizontal"));
    }
}
