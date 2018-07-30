using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    enum GameState
    {
        STARTMENU,
        INPLAY,
        PAUSED,
        GAMEOVER
    };

    GameState currentGameState;

	// Use this for initialization
	void Start () {
		
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
}
