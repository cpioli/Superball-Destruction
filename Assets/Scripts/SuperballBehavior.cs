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

    private Vector3 onEnterCollisionVector; //last vector when colliding with an object
    private Vector3 onExitCollisionVector; //last vector when bouncing off an object

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
                IncrementPosition();
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

    }

    void OnCollisionEnter(Collision other)
    {
        //print(this.name + "'s velocity: " + this.GetComponent<Rigidbody>().velocity);
    }

    void OnCollisionExit(Collision other)
    {
        //print(this.name + "'s velocity: " + this.GetComponent<Rigidbody>().velocity);
        print(this.name + "'s speed: " + this.GetComponent<Rigidbody>().velocity.magnitude);
        if(other.collider.gameObject.GetComponent<PanelBehavior>().worthPoints)
        {
            this.GetComponent<Rigidbody>().velocity *= 1.05f;
        }
        
    }

    void IncrementPosition()
    //Acts as physics: moves the ball in the currentDirection vector by velocity * deltaTime
    //also detects collision with panels
    {
        //this.transform.position += currentDirection * velocity * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        print(other.gameObject.name);
        Mesh mesh = other.gameObject.GetComponent<MeshFilter>().mesh;
        Vector3 normalVector = other.GetComponent<PanelBehavior>().GetNormal();
        print(other.gameObject.name + " Normal: " + normalVector);

        /*if (Mathf.Round(normalVector.x) != 0f)
        {
            print("Changing x direction! " + Mathf.Round(normalVector.x));
            currentDirection.x = -currentDirection.x;
        }
        else if (Mathf.Round(normalVector.z) != 0f)
        {
            print("Changing z direction! " + Mathf.Round(normalVector.z));
            currentDirection.z = -currentDirection.z;
        }
        else if (Mathf.Round(normalVector.y) != 0f)
        {
            print("Changing y direction! " + Mathf.Round(normalVector.y));
            currentDirection.y = -currentDirection.y;
        }
        print("Current direction: " + currentDirection);*/
    }
}