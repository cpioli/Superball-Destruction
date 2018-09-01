using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ResumeButtonBehavior : MonoBehaviour {

    public void ResumeTheGame()
    {
        ExecuteEvents.Execute<IGameEventHandler>(GameObject.Find("GameManager"), null, (x, y) => x.GameIsResumed());
    }
}
