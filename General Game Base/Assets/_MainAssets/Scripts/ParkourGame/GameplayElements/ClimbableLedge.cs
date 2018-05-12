using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbableLedge : MonoBehaviour {

    private void OnTriggerStay(Collider other)
    {
        ParkourPlayerPawn pp = other.GetComponent<ParkourPlayerPawn>();
        if (pp != null)
        {
            pp.UpdateClimbableLedge(this);
        }
    }
}
