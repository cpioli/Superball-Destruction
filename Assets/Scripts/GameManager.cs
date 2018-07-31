using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour, IGameEventHandler {

    enum GameState
    {
        STARTMENU,
        INPLAY,
        PAUSED,
        GAMEOVER
    };

    GameState currentGameState;

    public Canvas root;
    public Canvas StartMenu;
    public Canvas GameHUD;
    public Canvas PauseMenu;
    public Canvas PointsTally;
    public Canvas HighScores;

    public Camera StartMenuCamera;
    public Camera CannonCamera;
    public Camera RoomCamera;

    // Use this for initialization
    void Start () {
        GameStart();
        root.worldCamera = StartMenuCamera;
	}
	
    void Awake()
    {

    }

	// Update is called once per frame
	void Update () {
		switch(currentGameState)
        {
            case GameState.STARTMENU:
                break;

            case GameState.INPLAY:
                break;

            case GameState.PAUSED:
                break;

            case GameState.GAMEOVER:
                break;
        }
	}

    //triggered by: clicking on BeginGame in the Start Menu
    //TODO: switch to Cannon's Camera (titled: "Main Camera")
    //TODO: set main Canvas's world camera to "Main Camera")
    //TODO: turn on music
    public void GameStart()
    {
        currentGameState = GameState.INPLAY;
        StartMenu.gameObject.SetActive(true);
    }

    //triggered by: keyboard input
    public void GameIsPaused()
    {
        currentGameState = GameState.PAUSED;
    }

    //triggered by: button input or keyboard input
    //precondition: currentGameState == GameState.PAUSED, paused menu onscreen
    //postcondition: currentGameState == GameState.INPLAY, ingameui onscreen,
    //               player either has control over cannon or watches collisions
    public void GameIsResumed()
    {
        currentGameState = GameState.INPLAY;
    }

    //triggered by: button input
    //pre-condition: currentGameState == GameState.PAUSED
    //post-condition: exited game, returned to Start Menu,
    //                currentGameState == GameState.STARTMENU
    public void GameQuit()
    {

    }

    //triggered by: SuperballBehavior.ballState.DEAD and XZDegradation method
    //TODO: switch to Points Tally menu
    //TODO: after points Tally is over, put score on high-scores screen
    //TODO: switch to high score screen
    public void GameIsOver()
    {

    }

    //triggered by: "Clicking to Continue" in high score menu after game over
    public void ReturnToStartMenu()
    {

    }
    
    //triggered by: Any time a collision occurs. Can be implemented in object scripts
    //or superball behavior
    public void UpdateScore(int addition, bool isBreakable)
    {

    }
}
