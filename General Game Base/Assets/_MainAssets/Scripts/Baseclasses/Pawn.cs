using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a class that the player controller can possess and unposses to interact with the world
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class Pawn : Photon.MonoBehaviour {

    public bool Possessable = true;

    //private
    private bool possessed = false;
    protected PhotonView pv;
    protected PlayerController pc;

    protected virtual void Awake()
    {
        pv = transform.GetOrAddComponent<PhotonView>();
        if (!pv.isMine)
        {
            NotLocal();
        }
    }

    public virtual bool TryPossessPawn(PlayerController plc)
    {
        if (Possessable && !possessed)
        {
            if (pv != null)
            {
                pv.RPC("Possessed", PhotonTargets.All, true);
                pc = plc;
            }
            return true;
        }
        else
        {
            if (pv != null)
                pv.RPC("Possessed", PhotonTargets.All, false);
            return false;
        }
    }

    [PunRPC]
    protected virtual void Possessed(bool success)
    {
        possessed = success;
    }

    protected virtual void NotLocal()
    {
        //if this pawn is not mine
        foreach (Camera c in GetComponentsInChildren<Camera>())
        {
            c.gameObject.SetActive(false);
        }
        //rb.isKinematic = true;
        //thisCollider.enabled = false;
    }

}
