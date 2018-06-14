using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SuperballBehavior : MonoBehaviour
{

    public enum SuperBallState
    {
        ATREST,
        LIVE,
        DEAD
    }
    public SuperBallState ballState;
    public float velocity = 1.0f;
    public float maxSpeed = 22.352f; //meters per second (50mph)
    public Vector3 forward = new Vector3(1f, 0f, 1f);
    public Collider ConeCast;

    private bool hitBreakableObject;
    private int collisionLayer = 1 << 8;
    private int xZeroVelocityCount, yZeroVelocityCount, zZeroVelocityCount;
    private float maxObjectDeviation = 20f; //in degrees
    private Vector3 lastCollisionLocation, nextCollisionLocation, currentDirection;
    private GameObject nextCollisionObject, CannonBarrel;
    private Rigidbody rBody;

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

        Debug.DrawLine(this.transform.position, lastCollisionLocation, Color.yellow, 480f);

        lastCollisionLocation = this.transform.position;
        print(this.name + "'s speed: " + this.GetComponent<Rigidbody>().velocity.magnitude);

        Vector3 newDirection = GetComponent<Rigidbody>().velocity;

        //The line representing the velocity of our ball
        Vector3 origin = GameObject.Find("Sphere").transform.position;
        //Debug.DrawLine(origin, origin + newDirection, Color.white, 480f);

        Ray ray = new Ray(lastCollisionLocation, this.GetComponent<Rigidbody>().velocity.normalized);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 30f, 1 << 8)) //collisions is layer 8, so 1 << 8 is necessary
        {
            nextCollisionLocation = hitInfo.point;
            nextCollisionObject = hitInfo.collider.gameObject;
        }
        else
        {
            Debug.Log("Error calculating next collision!");
        }

        if (other.gameObject.GetComponent<MirrorBehavior>() != null)
        {
            HandleBreakableObjectCollision();
        }
        else
        {
            HandleUnbreakableObjectCollision();
        }

        CheckSuperballVelocity(this.GetComponent<Rigidbody>().velocity);

        Vector3 upVector = ConeCast.transform.forward;
        //Debug.DrawLine(origin, origin + upVector * 100f, Color.grey, 480f);
        ConeCast.transform.LookAt(newDirection + origin);
        //Debug.DrawLine(transform.position, nextCollisionLocation, Color.magenta);
    }

    private void HandleBreakableObjectCollision()
    {
        Debug.Log("Handling BREAKABLE Object collision!");
        this.IncrementVelocityFixed(0.05f);
    }

    private void HandleUnbreakableObjectCollision()
    {
        Debug.Log("Handling a SOLID Object collision!");
        this.DecrementVelocityFixed(-0.1f);
    }

    //NOTE: dampening the speed means the velocity must move to 0.0f
    //      but in a 3d coordinate system the velocity's components
    //      could be positive or negative. So I have to keep track of that.
    private void IncrementVelocityFixed(float fixedAmount)
    {
        float increment = Mathf.Abs(fixedAmount);
        float sumOfComponents = Mathf.Abs(rBody.velocity.x) + Mathf.Abs(rBody.velocity.y) + Mathf.Abs(rBody.velocity.z);
        float xIncrement = rBody.velocity.x / sumOfComponents * increment;
        float yIncrement = rBody.velocity.y / sumOfComponents * increment;
        float zIncrement = rBody.velocity.z / sumOfComponents * increment;

        rBody.velocity = new Vector3(rBody.velocity.x + xIncrement,
                                 rBody.velocity.y + yIncrement,
                                 rBody.velocity.z + zIncrement);
    }

    private void DecrementVelocityFixed(float fixedAmount)
    {
        float increment = Mathf.Abs(fixedAmount);
        float sumOfComponents = Mathf.Abs(rBody.velocity.x) + Mathf.Abs(rBody.velocity.y) + Mathf.Abs(rBody.velocity.z);
        float xIncrement = rBody.velocity.x / sumOfComponents * increment;
        float yIncrement = rBody.velocity.y / sumOfComponents * increment;
        float zIncrement = rBody.velocity.z / sumOfComponents * increment;

        rBody.velocity = new Vector3(rBody.velocity.x - xIncrement,
                                 rBody.velocity.y - yIncrement,
                                 rBody.velocity.z - zIncrement);
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