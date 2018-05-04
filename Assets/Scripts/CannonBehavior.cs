﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBehavior : MonoBehaviour {

    public float translationSpeed = .025f;
    public float shift = 80f;
    public float rotationSpeed = 0.5f;
    GameObject camObject;

    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;

    float rotationX = 0F;
    float rotationY = 0F;

    private List<float> rotArrayX = new List<float>();
    float rotAverageX = 0F;

    private List<float> rotArrayY = new List<float>();
    float rotAverageY = 0F;

    public float frameCounter = 20;

    Quaternion originalRotation;
    //reusable components to save memory.
    public GameObject RicochetArrows;
    public GameObject CannonBarrel;
    private SphereCollider sphereCollider;
    private RaycastHit hitInfo;
    private SuperballBehavior sbBehavior;

    // Use this for initialization
    void Start () {
        camObject = GameObject.Find("Main Camera");

        //https://forum.unity.com/threads/simple-first-person-camera-script.417611/
        //        Rigidbody rb = GetComponent<Rigidbody>();
        //        if (rb)
        //            rb.freezeRotation = true;
        sphereCollider = GameObject.Find("Sphere").GetComponent<SphereCollider>();
        originalRotation = transform.localRotation;
        sbBehavior = GameObject.Find("Sphere").GetComponent<SuperballBehavior>();
    }
	
	// Update is called once per frame
	void Update () {
        UpdateControls();
        if(sbBehavior.ballState == SuperballBehavior.SuperBallState.ATREST)
            UpdateAim();
	}

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
        AlignRicochetArrows();

    }

    private void AlignRicochetArrows()
    {
        float radius = sphereCollider.radius * sphereCollider.transform.localScale.x;
        Vector3 origin = sphereCollider.transform.position;
        Vector3 direction = CannonBarrel.transform.up; //due to some bad orientation of the model, this is the correct orientation
        Physics.SphereCast(origin, radius, direction, out hitInfo, 10f, 1 << 8);

        Vector3 collisionPoint = hitInfo.point;
        Vector3 newDirection = Vector3.Reflect(direction, hitInfo.normal);
        if(Input.GetKeyUp(KeyCode.F))
        {
            //Draw the orientation
            Debug.DrawLine(origin, origin + direction, Color.red, 480f);
            Debug.DrawLine(origin, hitInfo.point, Color.green, 480f);
            Debug.DrawLine(collisionPoint, collisionPoint + newDirection, Color.blue, 480f);
        }
        RicochetArrows.transform.position = collisionPoint;
        RicochetArrows.transform.rotation = Quaternion.LookRotation(newDirection);
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