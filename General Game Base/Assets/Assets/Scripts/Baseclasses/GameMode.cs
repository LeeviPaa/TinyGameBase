using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class' purpose is to contain all the gameplay related classes and manage the game flow and scene transistions
/// </summary>
public class GameMode : Photon.MonoBehaviour
{
    protected Pawn Player;
    protected SpawnPosition[] spawnPositions;

    public virtual void StartGame()
    {
        
    }

    public string objectName = "GameMode name";
    [SerializeField]
    protected PlayerController playerController;
    public PlayerController CurrentPlayerController
    {
        get
        {
            return playerController;
        }
    }
    [SerializeField]
    protected Pawn defaultPawn;
    public Pawn CurrentPawn
    {
        get
        {
            return defaultPawn;
        }
    }
    [SerializeField]
    protected PlayerHUD playerHud;
    public PlayerHUD CurrentPlayerHud
    {
        get
        {
            return playerHud;
        }
    }
    protected virtual void OnEnable()
    {
        Debug.Log("this object is enabled!");
    }
    protected virtual void OnDisable()
    {
        Debug.Log("this object is disabled!");
    }
    protected virtual void OnDestroy()
    {
        Debug.Log("this object is destroyed!");
    }
    protected virtual void Awake()
    {
        Debug.Log("this object exists!");
    }
}
