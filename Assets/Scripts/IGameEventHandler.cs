using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 * A list of all events that can be thrown to update the game manager
 * things like handling game lifecycle (start, game over, etc.)
 * also things like handling the high score
 */
public interface IGameEventHandler : IEventSystemHandler {
    //events: 
    //        pause, game over, game start,
    //        check high scores, check tutorial, check credits
    //Other events:
    //        ball fired, breakable collision, unbreakable collision, 
    //        ball reaiming, ball falling, ball resurrected, ball dying
    //        

    //game states
    void GameStart();
    void FiredCannon();
    void GameIsPaused();
    void GameIsResumed();
    void GameQuit();
    void GameIsOver();
    void CheckHighScoreScreen();
    void ReturnToStartMenu();

    void UpdateScore(int addition, bool isBreakable);
}
