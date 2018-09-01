using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class QuitButtonBehavior : MonoBehaviour {

    public void QuitTheGame()
    {
        ExecuteEvents.Execute<IGameEventHandler>(GameObject.Find("GameManager"), null, (x, y) => x.GameQuit());
    }
}
