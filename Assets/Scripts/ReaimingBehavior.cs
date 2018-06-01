﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReaimingBehavior : MonoBehaviour {

    public float translationSpeed = .025f;
    public float shift = 0.05f;
    public float rotationSpeed = 0.5f;
    GameObject camObject;

    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    float minimumX = -180f;
    float maximumX = 180f;

    float minimumY = -60F;
    float maximumY = 60F;

    float rotationX = 0F;
    float rotationY = 0F;
     
    private List<float> rotArrayX = new List<float>();
    float rotAverageX = 0F;

    private List<float> rotArrayY = new List<float>();
    float rotAverageY = 0F;

    public float frameCounter = 20;

    static Quaternion startingRotation;
    static Quaternion rigRotation;

    private bool reaimingActive;
    private float timeRemaining; //time left until the 
    private Vector3 ballPosition, ballVelocity;
    private GameObject roomCameraRig; //
    private GameObject roomCamera; 
    private Rigidbody superballRBody;
    private SuperballBehavior sbBehavior;
    private ArrowsBehavior arrowsBehavior;

    public float timeUntilLaunch; //how long can they hold onto the ball before it launches?

    // Use this for initialization
    void Start () {

        reaimingActive = false;
        timeRemaining = 0f;
        ballPosition = Vector3.zero;
        roomCameraRig = GameObject.Find("RoomCameraRig");
        rigRotation = roomCameraRig.transform.localRotation;
        roomCamera = GameObject.Find("RoomCamera");
        sbBehavior = GameObject.Find("Sphere").GetComponent<SuperballBehavior>();
        superballRBody = GameObject.Find("Sphere").GetComponent<Rigidbody>();
        arrowsBehavior = GameObject.Find("Arrows").GetComponent<ArrowsBehavior>();
	}
	
	// Update is called once per frame
	void Update () {
        if (reaimingActive)
            UpdateAiming();
	}

    //TODO: 3) orient the camera pointing to the plate's normal
    //      4) turn on the arrow pointers
    //      5) turn on mouse controls
    //      6) limit range of camera movement (might require a new superball state)
    void OnCollisionEnter(Collision collision)
    {
        print(collision.gameObject.name); //"Sphere"

        ballPosition = collision.gameObject.transform.position;
        //Debug.DrawRay(ballPosition, collision.contacts[0].normal * 1.05f, Color.cyan, 480f);
        
        ballVelocity = collision.gameObject.GetComponent<Rigidbody>().velocity;
        HaltSuperballMovement();
        PositionAndOrientCamera(collision);
        arrowsBehavior.AlignArrowsForAiming(ballPosition, startingRotation * Quaternion.AngleAxis(90, Vector3.right));
        arrowsBehavior.DrawDirectionalLines(ballPosition, GameObject.Find("Arrows").transform.rotation);
        //arrowsBehavior.DrawDirectionalLines(ballPosition, rigRotation);
        reaimingActive = true;
    }

    private void HaltSuperballMovement()
    {
        sbBehavior.ballState = SuperballBehavior.SuperBallState.DEAD;
        superballRBody.velocity = Vector3.zero;
        superballRBody.angularVelocity = Vector3.zero;
        superballRBody.useGravity = false;
    }

    private void PositionAndOrientCamera(Collision collision)
    {
        roomCamera.transform.position = collision.transform.position;
        //camera must face the direction the object is looking in
        roomCamera.transform.LookAt(ballPosition + collision.contacts[0].normal * -1f);

        startingRotation = Quaternion.LookRotation(ballVelocity, Vector3.up);
        Debug.DrawRay(ballPosition, ballVelocity * 3f, Color.red, 60f);

    }

    //https://forum.unity.com/threads/simple-first-person-camera-script.417611/
    //taken from the website above
    private void UpdateAiming()
    {
        rotAverageY = 0f;
        rotAverageX = 0f;

        rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
        rotationX += Input.GetAxis("Mouse X") * sensitivityX;

        rotArrayY.Add(rotationY);
        rotArrayX.Add(rotationX);

        if (rotArrayY.Count >= frameCounter)
        {
            rotArrayY.RemoveAt(0);
        }
        if (rotArrayX.Count >= frameCounter)
        {
            rotArrayX.RemoveAt(0);
        }

        for (int j = 0; j < rotArrayY.Count; j++)
        {
            rotAverageY += rotArrayY[j];
        }
        for (int i = 0; i < rotArrayX.Count; i++)
        {
            rotAverageX += rotArrayX[i];
        }

        rotAverageY /= rotArrayY.Count;
        rotAverageX /= rotArrayX.Count;

        rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);
        rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

        Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
        Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);//Vector3.up);

        roomCamera.transform.localRotation = rigRotation * xQuaternion * yQuaternion;
        arrowsBehavior.AlignArrowsForAiming(
            this.transform.position,
            roomCamera.gameObject.transform.localRotation * Quaternion.AngleAxis(90f, Vector3.right));
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        angle = angle % 360;
        if ((angle >= -360F) && (angle <= 360F))
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }
        }
        return Mathf.Clamp(angle, min, max);
    }

}
