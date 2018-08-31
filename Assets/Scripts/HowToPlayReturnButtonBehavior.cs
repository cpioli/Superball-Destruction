using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HowToPlayReturnButtonBehavior : MonoBehaviour {

	public void ReturnToStartMenu()
    {
        ExecuteEvents.Execute<IGameEventHandler>(GameObject.Find("GameManager"), null, (x, y) => x.ReturnToStartMenu());
    }
}
