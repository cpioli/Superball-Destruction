using UnityEngine;
using System.Collections;

public class SuperballBehavior : MonoBehaviour
{

    enum SuperBallState
    {
        ATREST,
        LIVE,
        DEAD
    }
    private SuperBallState ballState;
    private Vector3 currentDirection;
    private Rigidbody rBody;

    private Vector3 lastCollisionLocation;
    private Vector3 nextCollisionLocation; //the destination of the superball
    private GameObject nextCollisionObject;
    private int collisionLayer = 1 << 8;
    private float maxObjectDeviation = 20f; //in degrees
    private bool hitBreakableObject;
    private int xZeroVelocityCount, yZeroVelocityCount, zZeroVelocityCount;
    public Vector3 forward = new Vector3(1f, 0f, 1f);
    public float velocity = 1.0f;

    // Use this for initialization
    void Start()
    {
        ballState = SuperBallState.ATREST;
        rBody = this.GetComponent<Rigidbody>();
        forward.Normalize();
        hitBreakableObject = false;
        xZeroVelocityCount = 0;
        yZeroVelocityCount = 0;
        zZeroVelocityCount = 0;
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

        print("Adding force!");
        ballState = SuperBallState.LIVE;
        rBody.AddForce(forward.normalized * velocity, ForceMode.Impulse);
        //IncrementPosition(); //TODO: implement
        lastCollisionLocation = this.transform.position;
    }



    void OnCollisionEnter(Collision other)
    {
        Debug.DrawLine(this.transform.position, lastCollisionLocation, Color.yellow, 480f);
        lastCollisionLocation = this.transform.position;
        if(other.gameObject.GetComponent<MirrorBehavior>() != null)
        {
            hitBreakableObject = true;
        }
        else
        {
            hitBreakableObject = false;
        }
    }

    void OnCollisionExit(Collision other)
    {
        print(this.name + "'s velocity: " + this.GetComponent<Rigidbody>().velocity);
        print(this.name + "'s speed: " + this.GetComponent<Rigidbody>().velocity.magnitude);
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

        if (hitBreakableObject)
        {
            HandleBreakableObjectCollision();
        }
        else
        {
            HandleUnbreakableObjectCollision();
        }
        CheckSuperballVelocity(this.GetComponent<Rigidbody>().velocity);
        /* I need to comment out this block to see what's causing things to go wrong.
         * Vector3 nextBestBreakableObject = Vector3.zero;
        if (!NextCollisionIsBreakable())
        {
            nextBestBreakableObject = FindClosestBreakableObject();
            if (nextBestBreakableObject != Vector3.zero)
            {
                Debug.Log("Readjusting superball's trajectory!");
                ReadjustSuperballTrajectory(nextBestBreakableObject);
            }
        }*/
        
    }

    void CheckSuperballVelocity(Vector3 velocity)
    {
        int deadAxes = 0;
        if (Mathf.Abs(velocity.x) <= 0.1f)
        {
            deadAxes++;
            xZeroVelocityCount++;
            print("incremented xZeroVelocityCount: " + xZeroVelocityCount);
        }
        else
        {
            print("vZeroCount is reset! " + velocity.x);
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

        if (xZeroVelocityCount >= 4 || yZeroVelocityCount >= 4 || zZeroVelocityCount >= 4)
        {
            Debug.LogErrorFormat("Error: Velocity in at least one direction has been nullified: {0}", rBody.velocity);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPaused = true;
#endif
        }

    }

    private void HandleBreakableObjectCollision()
    {
        Debug.Log("Handling BREAKABLE Object collision!");
        this.ChangeVelocityByIncrement(0.05f);
    }

    private void HandleUnbreakableObjectCollision()
    {
        Debug.Log("Handling an UNbreakable Object collision!");
        this.ChangeVelocityByIncrement(-0.05f);
    }

    // checks to see if the next collideable object is breakable.
    private bool NextCollisionIsBreakable()
    {
        return (nextCollisionObject.layer == collisionLayer);
    }

    // Returns a vector colliding with the closest breakable object 
    // within the radial bounds of the superball's forward vector.
    // Returns the zero Vector if no breakable object is found.
    private Vector3 FindClosestBreakableObject()
    {
        Vector3 closestObject = Vector3.zero;
        
        Collider[] colliders;
        colliders = Physics.OverlapSphere(this.transform.position, 6f, collisionLayer);
        float closestObjectTheta = maxObjectDeviation;
        for(int i = 0; i < colliders.Length; i++)
        {
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
            }
        }
        
        return closestObject;
    }

    //Checks to see if the superball has gone past the objecct whose collision
    //was predicted by our trajectory calculations in OnCollisionExit()
    private bool ObjectPassedCollision()
    {
        return true;
    }

    private void ReadjustSuperballTrajectory(Vector3 nextCollisionPoint)
    {
        rBody = GetComponent<Rigidbody>();
        float speed = rBody.velocity.magnitude;
        Vector3 newForward = (nextCollisionPoint - transform.position).normalized;
        rBody.velocity = newForward * speed;
    }

    // increases or decreases the velocity of a rigidbody by a fixed amount
    // this is done by finding the change between the velocity's magnitude
    // and the increment (a scalar float value). Then multiplying the Vector3
    // by a scalar 1.0f + increment_percentage
    private void ChangeVelocityByIncrement(float increment)
    {
        Vector3 rBodyVel = GetComponent<Rigidbody>().velocity;
        float magnitude = rBodyVel.magnitude;
        float difference = increment / magnitude;
        GetComponent<Rigidbody>().velocity *= (1.0f + difference);
    }

    // changes the velocity of a rigidbody by a scalar value
    private void ChangeVelocityByScalar(float scalar)
    {
        GetComponent<Rigidbody>().velocity *= scalar;
    }
}