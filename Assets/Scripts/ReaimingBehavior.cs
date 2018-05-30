using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReaimingBehavior : MonoBehaviour {

    private float timeRemaining; //time left until the 
    private Vector3 ballPosition, ballVelocity;
    private GameObject roomCamera; //we need the GO for movement and aiming purposes
    private Rigidbody superballRBody;
    private SuperballBehavior sbBehavior;

    public float timeUntilLaunch; //how long can they hold onto the ball before it launches?

	// Use this for initialization
	void Start () {
        timeRemaining = 0f;
        ballPosition = Vector3.zero;
        roomCamera = GameObject.Find("RoomCamera");
        sbBehavior = GameObject.Find("Sphere").GetComponent<SuperballBehavior>();
        superballRBody = GameObject.Find("Sphere").GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {

	}

    //TODO: 1) set superball behavior state to AT_REST
    //      2) move the room camera to the ball's position
    //      3) orient the camera pointing to the plate's normal
    //      4) turn on the arrow pointers
    //      5) turn on mouse controls
    //      6) limit range of camera movement (might require a new superball state)
    void OnCollisionEnter(Collision collision)
    {
        ballPosition = GameObject.Find("Sphere").transform.position;
        ballVelocity = GameObject.Find("Sphere").GetComponent<Rigidbody>().velocity;
        HaltSuperballMovement();

        PositionAndOrientCamera(collision);
    }

    private void HaltSuperballMovement()
    {
        sbBehavior.ballState = SuperballBehavior.SuperBallState.DEAD;
        superballRBody.velocity = Vector3.zero;
        superballRBody.angularVelocity = Vector3.zero;
    }

    private void PositionAndOrientCamera(Collision collision)
    {
        roomCamera.transform.position = collision.transform.position;
        //camera must face the direction the object is looking in
        roomCamera.transform.LookAt(ballPosition + ballVelocity);
    }

    


}
