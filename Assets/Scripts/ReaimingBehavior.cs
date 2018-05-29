using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReaimingBehavior : MonoBehaviour {

    private float timeRemaining; //time left until the 
    private Vector3 ballPosition;
    private SuperballBehavior superballBehavior;
    private GameObject roomCamera; //we need the GO for movement and aiming purposes

    public float timeUntilLaunch; //how long can they hold onto the ball before it launches?

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

    //TODO: 1) set superball behavior state to AT_REST
    //      2) move the room camera to the ball's position
    //      3) orient the camera pointing to the plate's normal
    //      4) turn on the arrow pointers
    //      5) turn on mouse controls
    //      6) limit range of camera movement (might need a new superball state)
    void OnCollisionEnter(Collision collision)
    {
        
    }

}
