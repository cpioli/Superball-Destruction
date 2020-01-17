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

    public float velocity = 1.0f;
    public Vector3 forward = new Vector3(1f, 0f, 1f);
    public AudioClip bounceSound;

    private bool isLastObjectBreakable, isCurrentObjectBreakable; //tracks if we increment or reduce fibonacci
    private bool liveStateOverridesFallingCheck;

    private int collisionLayer = 1 << 8;
    private int xZeroVelocityCount, yZeroVelocityCount, zZeroVelocityCount;
    private int collisionID;
    private int points = 100;
    private int currentVelocityIncrement;

    private float maxSpeed = 11.176f; //25mph
    private float gravityActivationThreshold = 7.5f;
    private float previousMagnitude;
    private float[] oldIncrement1 = { 0.1f, 0.1f, 0.2f, 0.3f, 0.5f, 0.8f, 1.3f };
    private float[] newIncrement1 = { 0.1f, 0.1f, 0.15f, 0.2f, 0.275f, 0.375f, 0.65f };
    private float accumFloorDur;

    private Vector3 lastCollisionLocation, nextCollisionLocation, currentDirection;
    private GameObject nextCollisionObject, CannonBarrel;
    private GameObject emptyGameObject;
    private GameObject collisionFolder;
    private Rigidbody rBody;

    // Use this for initialization
    void Awake()
    {
        print("ball state is DEAD");
        ballState = SuperBallState.DEAD;
        xZeroVelocityCount = 0;
        yZeroVelocityCount = 0;
        zZeroVelocityCount = 0;
        forward.Normalize();

        CannonBarrel = GameObject.Find("Cannon");
        rBody = this.GetComponent<Rigidbody>();

        GameObject.Find("RoomCamera").GetComponent<Camera>().enabled = false;
        currentVelocityIncrement = 0;
        isLastObjectBreakable = true;
        isCurrentObjectBreakable = true;
        liveStateOverridesFallingCheck = false;

        accumFloorDur = 0f;

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
        print("Startup ball");
        ballState = SuperBallState.ATREST;
        rBody.useGravity = false;
        isLastObjectBreakable = true;
        isCurrentObjectBreakable = true;
        liveStateOverridesFallingCheck = false;
        accumFloorDur = 0f;
        currentVelocityIncrement = 0;
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
        rBody.AddForce(CannonBarrel.transform.up.normalized * velocity * 0.6f, ForceMode.Impulse);
        lastCollisionLocation = this.transform.position;
        print("Current ball velocity: " + rBody.velocity.magnitude);
    }

    void XZVelocityDecay()
    {
        if (Mathf.Abs(rBody.velocity.y) >= 0.1f) return;

        accumFloorDur += Time.deltaTime;
        print(accumFloorDur);
        Mathf.Lerp(rBody.velocity.x, 0f, accumFloorDur);
        Mathf.Lerp(rBody.velocity.z, 0f, accumFloorDur);

        if(accumFloorDur >= 1.0f) //alternatively use if(rBody.velocity.x == 0f) or if(rBody.velocity.z == 0f)
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

        CreateNewCollisionPoint(other);

        lastCollisionLocation = this.transform.position;
    }

    private void CreateNewCollisionPoint(Collision other)
    {
        for (int i = 0; i < other.contacts.Length; i++)
        {
            ContactPoint contactPoint = other.contacts[i];
            GameObject nextPoint = Instantiate(emptyGameObject, contactPoint.point, Quaternion.identity) as GameObject;
            nextPoint.name = collisionID.ToString() + "-" + i.ToString();
            nextPoint.transform.parent = collisionFolder.transform;
        }
        collisionID++;

    }

    void OnCollisionExit(Collision other)
    {
        if (ballState != SuperBallState.LIVE && ballState != SuperBallState.FALLING)
            return;

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

        AdjustBallVelocity(other);

        if(rBody.velocity.magnitude < gravityActivationThreshold)
        {
            print("Below threshold!");
        }
        if(FallingStateIsTriggered())
        {
            Debug.Log("Now entering the FALLEN state: " + rBody.velocity);
            SwitchToFallingState();
        }
    }

    private void AdjustBallVelocity(Collision col)
    {
        isLastObjectBreakable = isCurrentObjectBreakable;
        isCurrentObjectBreakable = col.gameObject.GetComponent<MirrorBehavior>() != null ? true : false;
        if (isLastObjectBreakable != isCurrentObjectBreakable)
        {
            currentVelocityIncrement = 0;
        }
        else
        {
            if (currentVelocityIncrement < newIncrement1.Length - 1)
                currentVelocityIncrement++;
        }
        if (isCurrentObjectBreakable)
            HandleBreakableObjectCollision(newIncrement1[currentVelocityIncrement], col);
        else
            HandleUnbreakableObjectCollision(-newIncrement1[currentVelocityIncrement], col);

        ExecuteEvents.Execute<IGameHUDEvent>(
            GameObject.Find("InGame"),
            null, (x, y) => x.UpdateSpeed(rBody.velocity.magnitude));
    }

    private void HandleBreakableObjectCollision(float increment, Collision collision)
    {
        ExecuteEvents.Execute<IGameEventHandler>(GameObject.Find("GameManager"), null, (x, y) => x.UpdateScore(points, isCurrentObjectBreakable));

        Debug.Log((collisionID-1).ToString() + " BREAKABLE Object! At Position " + lastCollisionLocation +
            " Against object " + collision.transform.gameObject.name + "\n" +
            this.name + "'s speed: " + this.GetComponent<Rigidbody>().velocity.magnitude +
            " vector: " + GetComponent<Rigidbody>().velocity.ToString());

        if (ballState == SuperBallState.FALLING)
        {
            ballState = SuperBallState.LIVE;
            rBody.useGravity = false;
            Debug.Log("Exiting FALLEN state, reentering LIVE state!");
            liveStateOverridesFallingCheck = true;
            //the velocity might be < gravityActivationThreshold and the state could reset to FALLING
            //this boolean allows us to override that.
        }

        if (rBody.velocity.magnitude < maxSpeed)
        {
            print("increasing speed! Magnitude: " + rBody.velocity.magnitude + " maxSpeed: " + maxSpeed);
            this.IncrementVelocityFixed(increment);
        }
        else if (rBody.velocity.magnitude >= maxSpeed)
        {
            Debug.Log("Speed is unchanged");
            return;
        }
        else if(rBody.velocity.magnitude + increment > maxSpeed)
        { 
            Debug.Log("Capping velocity");
            Debug.Log("magnitude: " + rBody.velocity.magnitude);
            this.IncrementVelocityFixed(maxSpeed - rBody.velocity.magnitude);
        }

        Debug.Log("New adjusted stats: " + this.name + "'s speed: " + 
            this.GetComponent<Rigidbody>().velocity.magnitude + " vector: " +
            GetComponent<Rigidbody>().velocity.ToString());
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

    private void IncrementVelocityFixed(float fixedAmount)
    {
        rBody.velocity = rBody.velocity.normalized * (rBody.velocity.magnitude + fixedAmount);
    }

    private bool FallingStateIsTriggered()
    {
        return (rBody.velocity.magnitude < gravityActivationThreshold &&
                !liveStateOverridesFallingCheck &&
                ballState != SuperBallState.FALLING);
    }

    private void SwitchToFallingState()
    {
        ballState = SuperBallState.FALLING;
        rBody.useGravity = true;
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
        if (Mathf.Abs(previousMagnitude - rBody.velocity.magnitude) > newIncrement1[currentVelocityIncrement] + 0.2f)
        {
#if UNITY_EDITOR
            Debug.Log("Change in magnitude: " + Mathf.Abs(previousMagnitude -
                rBody.velocity.magnitude) + "\n" + "current fibonacci " +
                "increment: " + newIncrement1[currentVelocityIncrement]);
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