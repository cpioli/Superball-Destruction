using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReaimingBehavior : MonoBehaviour {

    public float translationSpeed = .025f;
    public float shift = 0.05f;
    public float rotationSpeed = 0.5f;
    GameObject camObject;

    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    float minimumX = -90f;
    float maximumX = 90f;

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

    private bool reaimingActive;
    private float timeRemaining; //time left until the 
    private Vector3 ballPosition, ballVelocity, origRoomCamPos;
    private Quaternion origRoomCamAngle;
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
        roomCamera = GameObject.Find("RoomCamera");
        sbBehavior = GameObject.Find("Sphere").GetComponent<SuperballBehavior>();
        superballRBody = GameObject.Find("Sphere").GetComponent<Rigidbody>();
        arrowsBehavior = GameObject.Find("Arrows").GetComponent<ArrowsBehavior>();
	}
	
	// Update is called once per frame
	void Update () {
        if (reaimingActive)
            UpdateAiming();
        if(Input.GetKey(KeyCode.Space) && reaimingActive)
        {
            LaunchBall();
        }
	}

    //TODO: 3) orient the camera pointing to the plate's normal
    //      4) turn on the arrow pointers
    //      5) turn on mouse controls
    //      6) limit range of camera movement (might require a new superball state)
    void OnCollisionEnter(Collision collision)
    {
        print(collision.gameObject.name); //"Sphere"

        ballPosition = collision.gameObject.transform.position;
        //Debug.DrawRay(ballPosition, collision.contacts[0].normal * -5f, Color.cyan, 480f);
        
        ballVelocity = collision.gameObject.GetComponent<Rigidbody>().velocity;
        HaltSuperballMovement();
        PositionAndOrientCamera(collision);
        arrowsBehavior.AlignArrowsForAiming(ballPosition, startingRotation * Quaternion.AngleAxis(90, Vector3.right));
        reaimingActive = true;
        sbBehavior.ballState = SuperballBehavior.SuperBallState.REAIMING;
    }

    private void HaltSuperballMovement()
    {
        superballRBody.velocity = Vector3.zero;
        superballRBody.angularVelocity = Vector3.zero;
        superballRBody.useGravity = false;
    }

    private void PositionAndOrientCamera(Collision collision)
    {
        origRoomCamPos = roomCamera.transform.position;
        origRoomCamAngle = roomCamera.transform.localRotation;
        roomCamera.transform.position = collision.transform.position + new Vector3(0f, 0.05f, 0f);
        //camera must face towards the normal of the surface this script is attached to
        roomCamera.transform.localRotation = Quaternion.LookRotation(collision.contacts[0].normal * -2f);
        Debug.DrawRay(roomCamera.transform.position, roomCamera.transform.forward * 4f, Color.magenta, 480f);

        startingRotation = roomCamera.transform.rotation;
        Debug.DrawRay(ballPosition, ballVelocity * 3f, Color.red, 60f);
        arrowsBehavior.DrawDirectionalLines(roomCamera.transform.position, roomCamera.transform.rotation);

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

        roomCamera.transform.localRotation = startingRotation * xQuaternion * yQuaternion;
        arrowsBehavior.AlignArrowsForAiming(
            ballPosition,
            roomCamera.gameObject.transform.localRotation * Quaternion.AngleAxis(90f, Vector3.right));
    }

    private void LaunchBall()
    {
        sbBehavior.ballState = SuperballBehavior.SuperBallState.LIVE;
        print("Adding force!");
        superballRBody.AddForce(
            roomCamera.transform.forward.normalized * ballVelocity.magnitude, ForceMode.Impulse);
        roomCamera.transform.position = origRoomCamPos;
        roomCamera.transform.localRotation = origRoomCamAngle;
        reaimingActive = false;
        sbBehavior.ballState = SuperballBehavior.SuperBallState.LIVE;
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
