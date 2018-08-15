using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAI : MonoBehaviour, ISuperballInstantiatedEvent {

    private bool ballBehaviorTrackable;
    private GameObject ball;
    private SuperballBehavior sbBehavior;

    void Start () {
        ballBehaviorTrackable = false;
    }
	
	// Update is called once per frame
	void LateUpdate () {
        if (!ballBehaviorTrackable) return;
        print("ball state should be LIVE. It's currently: " + sbBehavior.ballState);
        if(sbBehavior.ballState == SuperballBehavior.SuperBallState.LIVE
            || sbBehavior.ballState == SuperballBehavior.SuperBallState.FALLING)
            transform.LookAt(ball.transform.position);
	}

    public void SuperballIsBuilt()
    {
        ball = GameObject.Find("Sphere");
        sbBehavior = ball.GetComponent<SuperballBehavior>() ;
        ballBehaviorTrackable = true;
    }

    public void SuperballIsDestroyed()
    {
        ballBehaviorTrackable = false;
    }


}
