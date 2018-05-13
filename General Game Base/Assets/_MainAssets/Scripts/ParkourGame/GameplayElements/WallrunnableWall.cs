using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrunnableWall : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        ParkourPlayerPawn pp = other.GetComponent<ParkourPlayerPawn>();
        if (pp != null)
        {
            pp.StartWallrunning(this);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        ParkourPlayerPawn pp = other.GetComponent<ParkourPlayerPawn>();
        if (pp != null)
        {
            pp.UpdateRunnableWall(this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        ParkourPlayerPawn pp = other.GetComponent<ParkourPlayerPawn>();
        if (pp != null)
        {
            pp.ExitRunnableWall(this);
        }
    }
}
