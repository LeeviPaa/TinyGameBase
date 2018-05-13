using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTrigger : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        ParkourPlayerPawn pp = other.gameObject.GetComponent<ParkourPlayerPawn>();
        if(pp != null)
        {
            pp.FallOffMap();
        }
    }
}
