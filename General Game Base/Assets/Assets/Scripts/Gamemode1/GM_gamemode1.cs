﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GM_gamemode1 : GameMode {

    public bool isOnlineEnabled = true;
    public Text debugText;

    private PhotonView thisPhotonView;
    private PlayerController thisPlayerController;

    public override void StartGame()
    {
        base.StartGame();

        debugText = FindObjectOfType<Text>();

        if(!isOnlineEnabled)
        {
            OfflineSetup();
        }
        else
        {
            OnlineSetup();
        }
    }

    private void OfflineSetup()
    {
        Player = Instantiate(defaultPawn);

        spawnPositions = FindObjectsOfType<SpawnPosition>();
        Player.transform.position = spawnPositions[0].transform.position;

        if (Player.TryPossessPawn(playerController))
        {
            Debug.Log("Possessed: " + defaultPawn.name + " with " + playerController.name);
        }
        else
        {
            Debug.LogError("Unable to possess pawn");
        }
    }

    private void OnlineSetup()
    {
        thisPhotonView = transform.GetComponent<PhotonView>();
        PhotonNetwork.ConnectUsingSettings("0.1");
        InvokeRepeating("UpdateConnectionStatus", 1, 1);
    }

    public virtual void OnJoinedLobby()
    {
        Debug.Log("joined to lobby");
        PhotonNetwork.JoinOrCreateRoom("VRtest1", null, null);
    }

    public virtual void OnJoinedRoom()
    {
        if (PhotonNetwork.player.IsLocal)
        {
            thisPlayerController = (PlayerController)Instantiate(playerController, transform.parent);

            spawnPositions = FindObjectsOfType<SpawnPosition>();
            int i = PhotonNetwork.playerList.Length;
            int index = i - ((int)(i / 4) * 4);

            GameObject Player = PhotonNetwork.Instantiate(defaultPawn.name, spawnPositions[i].transform.position, spawnPositions[0].transform.rotation, 0);

            bool possessed = false;
            if (photonView.isMine)
                 possessed = Player.GetComponent<Pawn>().TryPossessPawn(thisPlayerController.GetComponent<PlayerController>());

            Debug.Log("Possessed: " + possessed);

            FindObjectOfType<LobbyCamera>().gameObject.SetActive(false);
        }

    }

    private void UpdateConnectionStatus()
    {
        if (debugText != null)
        {
            debugText.text = (PhotonNetwork.connectionState.ToString());
        }
    }
}
