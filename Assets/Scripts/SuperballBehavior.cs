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
    private Vector3 currentDirection;
    private Rigidbody rBody;
    private GameObject CannonBarrel;
    public Collider ConeCast;

    private Vector3 lastCollisionLocation;
    private Vector3 nextCollisionLocation; //the destination of the superball
    private GameObject nextCollisionObject;
    private int collisionLayer = 1 << 8;
    private float maxObjectDeviation = 20f; //in degrees
    private bool hitBreakableObject;
    private int xZeroVelocityCount, yZeroVelocityCount, zZeroVelocityCount;
    public Vector3 forward = new Vector3(1f, 0f, 1f);
    public float velocity = 1.0f;
    public float maxSpeed = 22.352f; //meters per second (50mph)

    // Use this for initialization
    void Start()
    {
        ballState = SuperBallState.ATREST;
        CannonBarrel = GameObject.Find("Cannon");
        rBody = this.GetComponent<Rigidbody>();
        forward.Normalize();
        hitBreakableObject = false;
        xZeroVelocityCount = 0;
        yZeroVelocityCount = 0;
        zZeroVelocityCount = 0;
        GameObject.Find("RoomCamera").GetComponent<Camera>().enabled = false ;

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

        GameObject.Find("Main Camera").GetComponent<Camera>().enabled = false;
        GameObject.Find("RoomCamera").GetComponent<Camera>().enabled = true;
        print("Adding force!");
        ballState = SuperBallState.LIVE;
        rBody.AddForce(CannonBarrel.transform.up.normalized * velocity, ForceMode.Impulse);
        lastCollisionLocation = this.transform.position;
        //Debug.DrawLine(rBody.position, rBody.position + CannonBarrel.transform.up.normalized * 5f, Color.cyan, 480f);
    }


    //USe this method to track issues you might have with collisions
    void OnCollisionEnter(Collision other)
    {
        Debug.DrawLine(this.transform.position, lastCollisionLocation, Color.yellow, 480f);
        
        lastCollisionLocation = this.transform.position;

    }

    void OnCollisionExit(Collision other)
    {
        //actual ball motion
        Debug.DrawLine(this.transform.position, lastCollisionLocation, Color.yellow, 480f);

        lastCollisionLocation = this.transform.position;
        //print(this.name + "'s velocity: " + this.GetComponent<Rigidbody>().velocity);
        print(this.name + "'s speed: " + this.GetComponent<Rigidbody>().velocity.magnitude);

        Vector3 newDirection = GetComponent<Rigidbody>().velocity;

        //The line representing the velocity of our ball
        Vector3 origin1 = GameObject.Find("Sphere").transform.position;
        Debug.DrawLine(origin1, origin1 + newDirection, Color.white, 480f);
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
        //just a quick copy and paste to see if separating these two control blocks have to be together
        if (other.gameObject.GetComponent<MirrorBehavior>() != null)
        {
            HandleBreakableObjectCollision();
        }
        else
        {
            HandleUnbreakableObjectCollision();
        }

        CheckSuperballVelocity(this.GetComponent<Rigidbody>().velocity);
        /* This block is designed to reorient the ball's trajectory to get as close to another breakable object as possible
         * It's like a "smart ball" that will try to adjust itself mid collision.
         * It's not designed to be perfect. It won't hit everything.
         */

        Vector3 upVector = ConeCast.transform.forward;
        Debug.DrawLine(origin1, origin1 + upVector * 100f, Color.grey, 480f);
        //ConeCast.transform.rotation = Quaternion.LookRotation(newDirection + origin1, upVector);
        ConeCast.transform.LookAt(newDirection + origin1);
        Debug.DrawLine(transform.position, nextCollisionLocation, Color.magenta);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = true;
#endif
        /*if (!NextCollisionIsBreakable())
        {
            Vector3 nextBestBreakableObject = FindClosestBreakableObject(newDirection);
            if (nextBestBreakableObject != Vector3.zero)
            {
                Debug.Log("Readjusting superball's trajectory!");
                ReadjustSuperballTrajectory(nextBestBreakableObject);
            }
        }*/



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
     *                  SMART BALL TRACKING METHODS
     * 
     */

    // checks to see if the next collideable object is breakable.
    private bool NextCollisionIsBreakable()
    {
        return (nextCollisionObject.layer == collisionLayer);
    }

    // Returns a vector colliding with the closest breakable object 
    // within the radial bounds of the superball's forward vector.
    // Returns the zero Vector if no breakable object is found.
    private Vector3 FindClosestBreakableObject(Vector3 newDirection)
    {
        Vector3 closestObject = Vector3.zero;
        
        ConeCast.GetComponent<ConeCastColliderCollector>().ClearColliderContainer();
        ConeCast.transform.rotation = Quaternion.LookRotation(newDirection);
        List<Collider> colliders = ConeCast.GetComponent<ConeCastColliderCollector>().GetColliderContainer();
        float closestObjectTheta = maxObjectDeviation;
        for(int i = 0; i < colliders.Count; i++)
        {
            if (colliders[i].gameObject.GetComponent<MirrorBehavior>() == null)
                continue;
            Vector3 colliderPosition = colliders[i].transform.position;
            Vector3 colliderVector = colliderPosition - this.transform.position;
            //TODO: check to see if the collision point from the Overlap sphere is suitable for
            //the next collision of the superball. It's possible I may need to do a SphereCast
            //in the object's direction to determine the precise location of the target.
            // otherwise, trajectories could be off by a few hundredths of a unit and mess up the physics.
            Vector3 currentVector = GetComponent<Rigidbody>().velocity;
            float theta = Vector3.Angle(currentVector, colliderVector);
            if(theta < closestObjectTheta)
            {
                closestObject = colliders[i].ClosestPointOnBounds(transform.position);
                closestObjectTheta = theta;
            }
        }
        
        return closestObject;
    }

    private void ReadjustSuperballTrajectory(Vector3 nextCollisionPoint)
    {
        rBody = GetComponent<Rigidbody>();
        float speed = rBody.velocity.magnitude;
        Vector3 newForward = (nextCollisionPoint - transform.position).normalized;
        rBody.velocity = newForward * speed;
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