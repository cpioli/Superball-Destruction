using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAI : MonoBehaviour {

    private GameObject ball;

    void Start () {
        ball = GameObject.Find("Sphere");
	}
	
	// Update is called once per frame
	void LateUpdate () {
        if(ball.GetComponent<SuperballBehavior>().ballState == SuperballBehavior.SuperBallState.LIVE)
            transform.LookAt(ball.transform.position);
	}


}
