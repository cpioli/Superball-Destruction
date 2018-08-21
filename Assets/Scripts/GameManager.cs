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

    public GameObject Sphere;
    public GameObject Cannon;
    public GameObject Arrows;

    private SuperballBehavior sbBehavior;
    private Transform ballCannonTransform;
    private Transform ballTransform;
    private Vector3 sphereStartPos = new Vector3(0.0f, 0.033f, 0.0f);
    private int score;
    private int itemsBroken;
    private bool lastObjectWasBreakable;

    private Stack<GameObject> destroyedObjects; //keep this to restart things

    void Start () {
        destroyedObjects = new Stack<GameObject>();

        ballCannonTransform = Cannon.transform;
        ballTransform = Sphere.transform;
        Sphere = GameObject.Instantiate(this.Sphere, Cannon.transform, false);
        Sphere.gameObject.name = "Sphere";
        sbBehavior = Sphere.GetComponent<SuperballBehavior>();
        ExecuteEvents.Execute<ISuperballInstantiatedEvent>(Cannon, null, (x, y) => x.SuperballIsBuilt());
        ExecuteEvents.Execute<ISuperballInstantiatedEvent>(Arrows, null, (x, y) => x.SuperballIsBuilt());
        ExecuteEvents.Execute<ISuperballInstantiatedEvent>(GameObject.Find("RoomCamera"), null, (x, y) => x.SuperballIsBuilt());

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
        currentGameState = GameState.INPLAY;
        
        score = 0;
        itemsBroken = 0;
        lastObjectWasBreakable = false;
        LoadBall();
        SetupCameraAndUI();
        SetCannonToActiveState();

        RestoreBrokenItems();
    }

    #region GameStartMethods!
    private void LoadBall()
    {
        if(!Sphere.gameObject.activeInHierarchy)
        {
            Sphere.SetActive(true);
        }
        sbBehavior.StartupBallCannon();
        Sphere.transform.position = ballCannonTransform.position;
        Sphere.transform.rotation = ballCannonTransform.rotation;
        Sphere.transform.parent = Cannon.transform;
        Sphere.transform.localPosition = sphereStartPos;

    }
    private void SetupCameraAndUI() //can also add the Pause Menu screen here
    {
        StartMenuCamera.GetComponent<Camera>().enabled = false;
        CannonCamera.GetComponent<Camera>().enabled = true;
        root.worldCamera = CannonCamera;
        StartMenu.gameObject.SetActive(false);
        GameHUD.gameObject.SetActive(true);
    }

    private void SetCannonToActiveState()
    {
        if (!Cannon.activeInHierarchy)
        {
            Cannon.gameObject.SetActive(true);
        }
        if (!Arrows.activeInHierarchy)
        {
            Arrows.gameObject.SetActive(true);
        }
    }

    private void RestoreBrokenItems()
    {
        //restore all broken elements
        while (destroyedObjects.Count > 0)
        {
            GameObject go = destroyedObjects.Pop();
            go.GetComponent<MeshRenderer>().enabled = true;
            if (go.GetComponent<BoxCollider>() != null)
                go.GetComponent<BoxCollider>().enabled = true;
            else
            {
                go.transform.GetChild(0).gameObject.GetComponent<MeshCollider>().enabled = true;
            }
        }
    }
    #endregion

    public void FiredCannon()
    {
        CannonCamera.GetComponent<Camera>().enabled = false;
        RoomCamera.GetComponent<Camera>().enabled = true;
        CannonCamera.GetComponent<AudioSource>().enabled = false;
        RoomCamera.GetComponent<AudioSource>().enabled = true;
        root.worldCamera = RoomCamera;
        RemoveBallFromCannon();
        ResetCannonPosition();
        DeactivateCannonAndArrows();
    }

    #region FiredCannonMethods
    private void ResetCannonPosition()
    {
        Cannon.transform.position = ballCannonTransform.position;
        Cannon.transform.rotation = ballCannonTransform.rotation;
    }

    private void DeactivateCannonAndArrows()
    {
        Cannon.SetActive(false);
        Arrows.SetActive(false);
    }

    private void RemoveBallFromCannon()
    {
        Sphere.transform.parent = null;
    }
    #endregion

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

    public void RegisterGameObjectDestroyed(GameObject go)
    {
        destroyedObjects.Push(go);
    }

    public GameState GetCurrentGameState()
    {
        return currentGameState;
    }
}
