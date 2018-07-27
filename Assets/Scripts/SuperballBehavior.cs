using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public float velocity = 1.0f;
    public float maxSpeed = 22.352f; //meters per second (50mph)
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

    // Use this for initialization
    void Start()
    {
        ballState = SuperBallState.ATREST;
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

            case SuperBallState.DEAD:
                break;

            default:
                break;
        }
    }

    void BallAtRest()
    {
        if (!Input.GetKeyUp(KeyCode.Space))
            return;

        LaunchBall();
    }

    void LaunchBall()
    {
        GameObject.Find("Main Camera").GetComponent<Camera>().enabled = false;
        GameObject.Find("RoomCamera").GetComponent<Camera>().enabled = true;
        print("Adding force!");
        ballState = SuperBallState.LIVE;
        rBody.AddForce(CannonBarrel.transform.up.normalized * velocity, ForceMode.Impulse);
        lastCollisionLocation = this.transform.position;
    }


    //USe this method to track issues you might have with collisions
    void OnCollisionEnter(Collision other)
    {
        if (ballState != SuperBallState.LIVE)
            return;
        Debug.DrawLine(this.transform.position, lastCollisionLocation, Color.yellow, 480f);
        
        lastCollisionLocation = this.transform.position;

    }

    void OnCollisionExit(Collision other)
    {
        if (ballState != SuperBallState.LIVE)
            return;

        CheckForVelocityDampening();

        previousMagnitude = rBody.velocity.magnitude;
        Debug.DrawLine(this.transform.position, lastCollisionLocation, Color.yellow, 480f);

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
            currentFibonacci++;
            if (currentFibonacci > fibonacciIncrement.Length - 1)
                currentFibonacci = fibonacciIncrement.Length - 1;
        }
        if (isCurrentObjectBreakable)
            HandleBreakableObjectCollision(fibonacciIncrement[currentFibonacci], other);
        else
            HandleUnbreakableObjectCollision(fibonacciIncrement[currentFibonacci], other);

        CheckForVelocityDampening();

        //next update: when speed is less than x meters-per-second turn on gravity
    }

    private void HandleBreakableObjectCollision(float increment, Collision collision)
    {

        Debug.Log("BREAKABLE Object! At Position " + lastCollisionLocation +
            " Against object " + collision.transform.gameObject.name + "\n" +
            this.name + "'s speed: " + this.GetComponent<Rigidbody>().velocity.magnitude +
            " vector: " + GetComponent<Rigidbody>().velocity.ToString());

        this.IncrementVelocityFixed(increment);

        Debug.Log("New adjusted stats: " + this.name + "'s speed: " + 
            this.GetComponent<Rigidbody>().velocity.magnitude + " vector: " +
            GetComponent<Rigidbody>().velocity.ToString());
    }

    private void HandleUnbreakableObjectCollision(float decrement, Collision collision)
    {
        Debug.Log("SOLID Object! At Position " + lastCollisionLocation +
            " Against object " + collision.transform.gameObject.name + "\n" +
            this.name + "'s speed: " + this.GetComponent<Rigidbody>().velocity.magnitude +
            " vector: " + GetComponent<Rigidbody>().velocity.ToString());

        this.IncrementVelocityFixed(decrement);

        Debug.Log("New adjusted stats: " + this.name + "'s speed: " +
            this.GetComponent<Rigidbody>().velocity.magnitude + " vector: " +
            GetComponent<Rigidbody>().velocity.ToString());


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
        if (Mathf.Abs(previousMagnitude - rBody.velocity.magnitude) > 0.2f)
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