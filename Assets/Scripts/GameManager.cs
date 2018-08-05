using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour, IGameEventHandler {

    public enum GameState
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

    private SuperballBehavior sbBehavior;
    private int score;
    private int itemsBroken;
    private bool lastObjectWasBreakable;

    void Start () {
        sbBehavior = GameObject.Find("Sphere").GetComponent<SuperballBehavior>();
        root.worldCamera = StartMenuCamera;
        currentGameState = GameState.STARTMENU;
        CannonCamera.GetComponent<Camera>().enabled = false;
        StartMenuCamera.GetComponent<Camera>().enabled = true;
        StartMenu.gameObject.SetActive(true);
    }

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
    //TODO: turn on music
    public void GameStart()
    {
        score = 0;
        itemsBroken = 0;
        lastObjectWasBreakable = false;
        currentGameState = GameState.INPLAY;
        StartMenuCamera.GetComponent<Camera>().enabled = false;
        CannonCamera.GetComponent<Camera>().enabled = true;
        root.worldCamera = CannonCamera;
        StartMenu.gameObject.SetActive(false);
        GameHUD.gameObject.SetActive(true);
        sbBehavior.StartupBallCannon();
    }

    public void FiredCannon()
    {
        CannonCamera.GetComponent<Camera>().enabled = false;
        RoomCamera.GetComponent<Camera>().enabled = true;
        root.worldCamera = RoomCamera;
    }

    //triggered by: keyboard input
    public void GameIsPaused()
    {
        currentGameState = GameState.PAUSED;
        GameHUD.gameObject.SetActive(false);
        PauseMenu.gameObject.SetActive(true);
    }

    //triggered by: button input or keyboard input
    //precondition: currentGameState == GameState.PAUSED, paused menu onscreen
    //postcondition: currentGameState == GameState.INPLAY, ingameui onscreen,
    //               player either has control over cannon or watches collisions
    public void GameIsResumed()
    {
        currentGameState = GameState.INPLAY;
        StartMenu.gameObject.SetActive(true);
        PauseMenu.gameObject.SetActive(false);
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
        currentGameState = GameState.GAMEOVER;
        GameHUD.gameObject.SetActive(false);
        PointsTally.gameObject.SetActive(true);
        PointsTally.GetComponent<PointsTallyBehavior>().BeginTallySequence(itemsBroken, score);
        root.worldCamera = StartMenuCamera;
        RoomCamera.GetComponent<Camera>().enabled = false;
        StartMenuCamera.GetComponent<Camera>().enabled = true;
        
    }

    public void CheckHighScoreScreen()
    {

    }

    //triggered by: "Clicking to Continue" in high score menu after game over
    public void ReturnToStartMenu()
    {
        //code to teardown the PointsTally screen.
        //this block will be moved to CheckHighScoreScreen once it's implemented
        PointsTally.GetComponent<PointsTallyBehavior>().Disassemble();
        PointsTally.gameObject.SetActive(false);
        StartMenu.gameObject.SetActive(true);
    }
    
    //triggered by: Any time a collision occurs. Can be implemented in object scripts
    //or superball behavior
    public void UpdateScore(int addition, bool isBreakable)
    {
        /* string message = "";
         if(lastObjectWasBreakable && isBreakable)
         {
             message += "Rally ";
         }
         if(lastObjectWasBreakable && !isBreakable)
         {
             message += "Broken ";
         }
         lastObjectWasBreakable = isBreakable;
         message += addition.ToString();
         ExecuteEvents.Execute<IGameHUDEvent>(GameHUD.gameObject, null, (x, y) => x.AddNewMessage(message));*/
        itemsBroken++;
        score += addition;
        ExecuteEvents.Execute<IGameHUDEvent>(GameHUD.gameObject, null, (x, y) => x.UpdateScore(score));
    }

    public GameState GetCurrentGameState()
    {
        return currentGameState;
    }
}
