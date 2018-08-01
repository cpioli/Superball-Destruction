using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StartGameButtonBehavior : MonoBehaviour {

    public void StartTheGame()
    {
        GameObject gameManager = GameObject.Find("GameManager");
        ExecuteEvents.Execute<IGameEventHandler>(gameManager, null, (x, y) => x.GameStart());
    }
}
