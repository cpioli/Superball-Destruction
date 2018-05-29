using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReaimingBehavior : MonoBehaviour {

    public float rotationSpeed = 0.5f;
    GameObject camObject;

    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    public float minimumX = -90F;
    public float maximumX = 90F;

    public float minimumY = -60F;
    public float maximumY = 60F;

    float rotationX = 0F;
    float rotationY = 0F;

    private List<float> rotArrayX = new List<float>();
    private float rotAverageX = 0F;

    private List<float> rotArrayY = new List<float>();
    private float rotAverageY = 0F;

    public float frameCounter = 20;

    private float timeRemaining; //time left until the 
    private Vector3 ballPosition, ballVelocity;
    private SuperballBehavior sbBehavior;
    private GameObject roomCamera; //we need the GO for movement and aiming purposes

    public float timeUntilLaunch; //how long can they hold onto the ball before it launches?

	// Use this for initialization
	void Start () {
        timeRemaining = 0f;
        ballPosition = Vector3.zero;
        sbBehavior = GameObject.Find("Sphere").GetComponent<SuperballBehavior>();
        roomCamera = GameObject.Find("RoomCamera");
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
        sbBehavior.ballState = SuperballBehavior.SuperBallState.ATREST;
        PositionAndOrientCamera(collision);
    }

    private void PositionAndOrientCamera(Collision collision)
    {
        roomCamera.transform.position = collision.transform.position;
        //camera must face the direction the object is looking in
        roomCamera.transform.LookAt(ballPosition + ballVelocity);
    }

    


}
