using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CannonBehavior : MonoBehaviour, ISuperballInstantiatedEvent {

    private bool ballBehaviorTrackable;
    private SphereCollider sphereCollider;
    private RaycastHit hitInfo;
    private SuperballBehavior sbBehavior;
    private ArrowsBehavior arrowsBehavior;
    private List<float> rotArrayX = new List<float>();
    private List<float> rotArrayY = new List<float>();

    float rotationX = 0F;
    float rotationY = 0F;
    float rotAverageX = 0F;
    float rotAverageY = 0F;
    Quaternion originalRotation;

    public bool isCannonMovable;
    public float translationSpeed = .025f;
    public float shift = 80f;
    public float rotationSpeed = 0.5f;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -60F;
    public float maximumY = 60F;
    public float frameCounter = 20;
    public GameObject RicochetArrows;
    public GameObject CannonBarrel;

    // Use this for initialization
    void Start () {

        originalRotation = transform.localRotation;
        arrowsBehavior = GameObject.Find("Arrows").GetComponent<ArrowsBehavior>();
        isCannonMovable = false;
        ballBehaviorTrackable = false;
    }

    //event method
    public void SuperballIsBuilt()
    {
        sphereCollider = GameObject.Find("Sphere").GetComponent<SphereCollider>();
        sbBehavior = GameObject.Find("Sphere").GetComponent<SuperballBehavior>();
        ballBehaviorTrackable = true;
        print("Ball behavior is trackable now!");
    }

    public void SuperballIsDestroyed()
    {
        ballBehaviorTrackable = false;
    }

    // Update is called once per frame
    void LateUpdate () {
        if (!ballBehaviorTrackable) return;

        if (sbBehavior.ballState == SuperballBehavior.SuperBallState.ATREST)
        {
            UpdateControls();
        }
        if(sbBehavior.ballState == SuperballBehavior.SuperBallState.ATREST ||
            sbBehavior.ballState == SuperballBehavior.SuperBallState.REAIMING)
        {
            UpdateAim();
        }
    }

    //only runs if the ball is in the ATREST state
    void UpdateControls()
    {
        float currentSpeed = translationSpeed;
        bool goFaster = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (goFaster) currentSpeed += shift;
        //WASD input controls
        if (Input.GetKey(KeyCode.Q) /*|| Input.GetKeyDown(KeyCode.UpArrow)*/)
        {
            transform.Translate(Vector3.up * currentSpeed);
        }
        if (Input.GetKey(KeyCode.E) /*|| Input.GetKeyDown(KeyCode.DownArrow)*/)
        {
            transform.Translate(Vector3.down * currentSpeed);
        }
        if( Input.GetKey(KeyCode.S) )
        {
            transform.Translate(Vector3.left * currentSpeed);
        }
        if( Input.GetKey(KeyCode.W) )
        {
            transform.Translate(Vector3.right * currentSpeed);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(Vector3.forward * currentSpeed);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(Vector3.back * currentSpeed);
        }
    }

    //https://forum.unity.com/threads/simple-first-person-camera-script.417611/
    //taken from the website above
    // Works in the REAIMING state and the ATREST state
    void UpdateAim()
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

        Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.forward);
        Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);

        transform.localRotation = originalRotation * xQuaternion * yQuaternion;
        arrowsBehavior.AlignArrowsForAiming(
            sphereCollider.transform.position, 
            CannonBarrel.transform.rotation);

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
