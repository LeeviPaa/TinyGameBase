using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class' purpose is to manage the whole game at the lowest level
/// </summary>
public class GameManager : MonoBehaviour {

    //[SerializeField]
    public GameMode currentGameMode;
    private GameMode gameModeObject;

    private void Awake()
    {
        if(currentGameMode.CurrentPlayerController != null)
        {
            
        }
        else
        {
            Debug.LogError(currentGameMode.objectName + " GameMode does not have default player controller set!");
        }
        if(currentGameMode.CurrentPawn != null)
        {

        }
        else
        {
            Debug.LogError(currentGameMode.objectName + " GameMode does not have default pawn set!");
        }
        if (currentGameMode.CurrentPlayerHud != null)
        {

        }
        else
        {
            Debug.LogError(currentGameMode.objectName + " GameMode does not have default player HUD set!");
        }

        if(currentGameMode != null)
        {
            gameModeObject = Instantiate(currentGameMode, transform);
            gameModeObject.GetComponent<GameMode>().StartGame();
        }
    }

}
