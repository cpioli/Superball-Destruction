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

    public Vector3 forward = new Vector3(1f, 0f, 1f);
    public float velocity = 1.0f;

    // Use this for initialization
    void Start()
    {
        ballState = SuperBallState.ATREST;
        rBody = this.GetComponent<Rigidbody>();
        forward.Normalize();
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
                //IncrementPosition();
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
    }

    void OnCollisionExit(Collision other)
    {
        print(this.name + "'s velocity: " + this.GetComponent<Rigidbody>().velocity);
        print(this.name + "'s speed: " + this.GetComponent<Rigidbody>().velocity.magnitude);
        /* removing this so I can properly test for collision detection
         if(other.collider.gameObject.name.Contains("Plane"))
        {
            if(other.collider.gameObject.GetComponent<PanelBehavior>().worthPoints)
            {
                this.GetComponent<Rigidbody>().velocity *= 1.01f;
            }
            
        }*/
        Ray ray = new Ray(lastCollisionLocation, this.GetComponent<Rigidbody>().velocity.normalized);
        RaycastHit hitInfo;
        if(Physics.Raycast(ray, out hitInfo))
        {
            nextCollisionLocation = hitInfo.point;
        }
        else
        {
            Debug.Log("Error calculating next collision!");
        }
    }

    //Checks to see if the superball has gone past the objecct whose collision
    //was predicted by our trajectory calculations in OnCollisionExit()
    private bool ObjectPassedCollision()
    {
        return true;
    }

}