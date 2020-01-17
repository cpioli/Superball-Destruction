using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHUDBehavior : MonoBehaviour, IGameHUDEvent {

    public Text BallSpeed;
    public Text CurrentScore;

    private float mpsToMPH = 2.23694f;

    // Use this for initialization
    void Start () {
        CurrentScore.text = "0";
        BallSpeed.text = "0 mph";
	}

    public void UpdateScore(int newScore)
    {
        //print("Receiving " + newScore + " and updating score!");
        CurrentScore.text = newScore.ToString();
    }

    public void UpdateSpeed(float metersPerSecond)
    {
        float milesPerHour = metersPerSecond * mpsToMPH;
        BallSpeed.text = string.Format("{0:##.#} mph", milesPerHour);
    }
}
