using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrunnableWall : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        ParkourPlayerPawn pp = other.GetComponent<ParkourPlayerPawn>();
        PhotonView pv = other.GetComponent<PhotonView>();

        if (pp != null && pv.owner == PhotonNetwork.player)
        {
            pp.StartWallrunning(this);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        ParkourPlayerPawn playerPawn = other.GetComponent<ParkourPlayerPawn>();
        PhotonView pv = other.GetComponent<PhotonView>();

        if (playerPawn != null && pv.owner == PhotonNetwork.player)
        {
            playerPawn.UpdateRunnableWall(this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        ParkourPlayerPawn pp = other.GetComponent<ParkourPlayerPawn>();
        PhotonView pv = other.GetComponent<PhotonView>();

        if (pp != null && pv.owner == PhotonNetwork.player)
        {
            pp.ExitRunnableWall(this);
        }
    }
}
