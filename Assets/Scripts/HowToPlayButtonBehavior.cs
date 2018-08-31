using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HowToPlayButtonBehavior : MonoBehaviour {

	public void GoToHowToPlay()
    {
        GameObject gameManager = GameObject.Find("GameManager");
        ExecuteEvents.Execute<IGameEventHandler>(gameManager, null, (x, y) => x.SeeInstructions());
    }
}
