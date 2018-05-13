using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrunnableWall : MonoBehaviour {

    ParkourPlayerPawn playerPawn;
    bool localPlayerOnWall = false;

    private void Update()
    {
        if (playerPawn != null && localPlayerOnWall)
        {
            playerPawn.UpdateRunnableWall(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ParkourPlayerPawn pp = other.GetComponent<ParkourPlayerPawn>();
        PhotonView pv = other.GetComponent<PhotonView>();

        if (pp != null && pv.owner == PhotonNetwork.player)
        {
            pp.StartWallrunning(this);
            localPlayerOnWall = true;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        playerPawn = other.GetComponent<ParkourPlayerPawn>();
        PhotonView pv = other.GetComponent<PhotonView>();

        if (playerPawn != null && pv.owner == PhotonNetwork.player)
            localPlayerOnWall = true;
        else
        {
            localPlayerOnWall = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        ParkourPlayerPawn pp = other.GetComponent<ParkourPlayerPawn>();
        PhotonView pv = other.GetComponent<PhotonView>();

        if (pp != null && pv.owner == PhotonNetwork.player)
        {
            pp.ExitRunnableWall(this);
            localPlayerOnWall = false;
        }
    }
}
