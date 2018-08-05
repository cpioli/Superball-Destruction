using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SuperballBehavior : MonoBehaviour
{

    public enum SuperBallState
    {
        ATREST,
        LIVE,
        REAIMING,
        FALLING,
        DEAD
    }
    public SuperBallState ballState;
    public int points;
    public float velocity = 1.0f;
    public float maxSpeed = 22.352f; //meters per second (50mph)
    public float velocityThreshold; //if velocity.magnitude < velocityThreshold, gravity is on
    //0.44704 meters per second = 1 mile per hour
    public Vector3 forward = new Vector3(1f, 0f, 1f);

    private bool hitBreakableObject;
    private int collisionLayer = 1 << 8;
    private int xZeroVelocityCount, yZeroVelocityCount, zZeroVelocityCount;
    private float maxObjectDeviation = 20f; //in degrees
    private Vector3 lastCollisionLocation, nextCollisionLocation, currentDirection;
    private float previousMagnitude;
    private GameObject nextCollisionObject, CannonBarrel;
    private Rigidbody rBody;

    private bool isLastObjectBreakable, isCurrentObjectBreakable; //tracks if we increment or reduce fibonacci
    private float[] fibonacciIncrement = { 0.1f, 0.1f, 0.2f, 0.3f, 0.5f, 0.8f, 1.3f };
    private int currentFibonacci;

    private bool liveStateOverridesFallingCheck;
    private float accumulatedTime;

    private GameObject emptyGameObject;
    private GameObject collisionFolder;
    private int collisionID;

    // Use this for initialization
    void Start()
    {
        ballState = SuperBallState.DEAD;
        hitBreakableObject = false;
        xZeroVelocityCount = 0;
        yZeroVelocityCount = 0;
        zZeroVelocityCount = 0;
        forward.Normalize();

        CannonBarrel = GameObject.Find("Cannon");
        rBody = this.GetComponent<Rigidbody>();

        GameObject.Find("RoomCamera").GetComponent<Camera>().enabled = false;
        currentFibonacci = 0;
        isLastObjectBreakable = true;
        isCurrentObjectBreakable = true;
        liveStateOverridesFallingCheck = false;

        accumulatedTime = 0f;

        emptyGameObject = GameObject.Find("CollisionPoints");
        collisionFolder = Instantiate(emptyGameObject, emptyGameObject.transform.position, Quaternion.identity) as GameObject;
        collisionFolder.name = "CollisionFolder";
        collisionID = 0;

    }

    // Update is called once per frame
    void Update()
    {
        switch (ballState)
        {
            case SuperBallState.ATREST:
                BallAtRest();
                break;

            case SuperBallState.LIVE:
                break;

            case SuperBallState.FALLING:
                //TODO: run a check for DEAD state to trigger
                XZVelocityDecay();
                break;

            case SuperBallState.DEAD:
                break;

            default:
                break;
        }
    }

    public void StartupBallCannon()
    {
        ballState = SuperBallState.ATREST;
    }

    void BallAtRest()
    {
        if (!Input.GetKeyUp(KeyCode.Space))
            return;

        LaunchBall();
    }

    void LaunchBall()
    {
        ExecuteEvents.Execute<IGameEventHandler>(GameObject.Find("GameManager"), null, (x, y) => x.FiredCannon());
        print("Adding force!");
        ballState = SuperBallState.LIVE;
        rBody.AddForce(CannonBarrel.transform.up.normalized * velocity, ForceMode.Impulse);
        lastCollisionLocation = this.transform.position;
    }

    void XZVelocityDecay()
    {
        if (Mathf.Abs(rBody.velocity.y) >= 0.1f) return;

        accumulatedTime += Time.deltaTime;
        print(accumulatedTime);
        Mathf.Lerp(rBody.velocity.x, 0f, accumulatedTime);
        Mathf.Lerp(rBody.velocity.z, 0f, accumulatedTime);

        if(accumulatedTime >= 1.0f) //alternatively use if(rBody.velocity.x == 0f) or if(rBody.velocity.z == 0f)
        {
            ballState = SuperBallState.DEAD;
            rBody.velocity = Vector3.zero;
            ExecuteEvents.Execute<IGameEventHandler>(GameObject.Find("GameManager"), null, (x, y) => x.GameIsOver());
        }
    }

    //USe this method to track issues you might have with collisions
    void OnCollisionEnter(Collision other)
    {
        if (ballState != SuperBallState.LIVE && ballState != SuperBallState.FALLING)
            return;
        Debug.DrawLine(rBody.position, lastCollisionLocation, Color.yellow, 480f);
        
        for(int i = 0; i < other.contacts.Length; i++)
        {
            ContactPoint contactPoint = other.contacts[i];
            GameObject nextPoint = Instantiate(emptyGameObject, contactPoint.point, Quaternion.identity) as GameObject;
            nextPoint.name = collisionID.ToString() + "-" + i.ToString();
            nextPoint.transform.parent = collisionFolder.transform;
        }

        lastCollisionLocation = this.transform.position;
        collisionID++;

    }

    void OnCollisionExit(Collision other)
    {
        if (ballState != SuperBallState.LIVE && ballState != SuperBallState.FALLING)
            return;

        //CheckForVelocityDampening();

        previousMagnitude = rBody.velocity.magnitude;
        Debug.DrawLine(rBody.position, lastCollisionLocation, Color.yellow, 480f);

        lastCollisionLocation = this.transform.position;

        Vector3 newDirection = GetComponent<Rigidbody>().velocity;
        Vector3 origin = GameObject.Find("Sphere").transform.position;
        Ray ray = new Ray(lastCollisionLocation, this.GetComponent<Rigidbody>().velocity.normalized);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 30f, 1 << 8)) //collisions is layer 8, so 1 << 8 is necessary
        {
            nextCollisionLocation = hitInfo.point;
            nextCollisionObject = hitInfo.collider.gameObject;
        }
        else //Physics.Raycast returns "false"
        {
            Debug.Log("Error calculating next collision!");
        }

        isLastObjectBreakable = isCurrentObjectBreakable;
        isCurrentObjectBreakable = other.gameObject.GetComponent<MirrorBehavior>() != null ? true : false;


        if (isLastObjectBreakable != isCurrentObjectBreakable)
        {
            currentFibonacci = 0;
        }
        else
        {
            if (currentFibonacci < fibonacciIncrement.Length - 1)
                currentFibonacci++;
        }
        if (isCurrentObjectBreakable)
            HandleBreakableObjectCollision(fibonacciIncrement[currentFibonacci], other);
        else
            HandleUnbreakableObjectCollision(-fibonacciIncrement[currentFibonacci], other);

        //CheckForVelocityDampening();

        //next update: when speed is less than x meters-per-second turn on gravity
        if(rBody.velocity.magnitude < velocityThreshold && !liveStateOverridesFallingCheck && ballState != SuperBallState.FALLING)
        {
            Debug.Log("Now entering the FALLEN state: " + rBody.velocity);
            ballState = SuperBallState.FALLING;
            rBody.useGravity = true;
        }
    }

    private void HandleBreakableObjectCollision(float increment, Collision collision)
    {
        ExecuteEvents.Execute<IGameEventHandler>(GameObject.Find("GameManager"), null, (x, y) => x.UpdateScore(points, isCurrentObjectBreakable));

        Debug.Log((collisionID-1).ToString() + " BREAKABLE Object! At Position " + lastCollisionLocation +
            " Against object " + collision.transform.gameObject.name + "\n" +
            this.name + "'s speed: " + this.GetComponent<Rigidbody>().velocity.magnitude +
            " vector: " + GetComponent<Rigidbody>().velocity.ToString());

        this.IncrementVelocityFixed(increment);

        Debug.Log("New adjusted stats: " + this.name + "'s speed: " + 
            this.GetComponent<Rigidbody>().velocity.magnitude + " vector: " +
            GetComponent<Rigidbody>().velocity.ToString());

        if(ballState == SuperBallState.FALLING)
        {
            ballState = SuperBallState.LIVE;
            rBody.useGravity = false;
            Debug.Log("Exiting FALLEN state, reentering LIVE state!");
            liveStateOverridesFallingCheck = true;
            //the velocity might be < velocityThreshold and the state could reset to FALLING
            //this boolean allows us to override that.
        }
    }

    private void HandleUnbreakableObjectCollision(float decrement, Collision collision)
    {
        Debug.Log((collisionID - 1).ToString() + " SOLID Object! At Position " + lastCollisionLocation +
            " Against object " + collision.transform.gameObject.name + "\n" +
            this.name + "'s speed: " + this.GetComponent<Rigidbody>().velocity.magnitude +
            " vector: " + GetComponent<Rigidbody>().velocity.ToString());

        this.IncrementVelocityFixed(decrement);

        Debug.Log("New adjusted stats: " + this.name + "'s speed: " +
            this.GetComponent<Rigidbody>().velocity.magnitude + " vector: " +
            GetComponent<Rigidbody>().velocity.ToString());

        if(ballState == SuperBallState.LIVE && liveStateOverridesFallingCheck)
        {
            liveStateOverridesFallingCheck = false; //switch this back off so gravity can reactivate
            Debug.Log("Killing Falling state override");

        }
    }

    //NOTE: dampening the speed means the velocity must move to 0.0f
    //      but in a 3d coordinate system the velocity's components
    //      could be positive or negative. So I have to keep track of that.
    private void IncrementVelocityFixed(float fixedAmount)
    {
        rBody.velocity = rBody.velocity.normalized * (rBody.velocity.magnitude + fixedAmount);
    }

    /*
     * 
     *                      DEBUGGING METHODS
     * 
     */

    //Checks to see if the superball has gone past the objecct whose collision
    //was predicted by our trajectory calculations in OnCollisionExit()
    private bool ObjectPassedCollision()
    {
        return true;
    }

    private void CheckForVelocityDampening()
    {
        if (Mathf.Abs(previousMagnitude - rBody.velocity.magnitude) > fibonacciIncrement[currentFibonacci] + 0.2f)
        {
#if UNITY_EDITOR
            Debug.Log("Change in magnitude: " + Mathf.Abs(previousMagnitude -
                rBody.velocity.magnitude) + "\n" + "current fibonacci " +
                "increment: " + fibonacciIncrement[currentFibonacci]);
            UnityEditor.EditorApplication.isPaused = true;
#endif
        }
    }

    void CheckSuperballVelocity(Vector3 velocity)
    {
        int deadAxes = 0;
        if (Mathf.Abs(velocity.x) <= 0.1f)
        {
            deadAxes++;
            xZeroVelocityCount++;
        }
        else
        {
            xZeroVelocityCount = 0;
        }

        if (Mathf.Abs(velocity.x) <= 0.1f)
        {
            deadAxes++;
            yZeroVelocityCount++;
        }
        else yZeroVelocityCount = 0;

        if (Mathf.Abs(velocity.x) <= 0.1f)
        {
            deadAxes++;
            zZeroVelocityCount++;
        }
        else zZeroVelocityCount = 0;

        if (deadAxes >= 2)
        {
            Debug.LogErrorFormat("Error: Velocity in two directions has been nullified: {0}", rBody.velocity);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPaused = true;
            Debug.DrawLine(this.transform.position, lastCollisionLocation, Color.red, 480f);
#endif
        }

        else if (deadAxes == 1)
        {
            Debug.LogErrorFormat("Error: Velocity in at least one direction has been nullified: {0}", rBody.velocity);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPaused = true;
#endif
        }

    }
}