using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperballAI : MonoBehaviour {

    public float fieldOfView;
    //the maximum number of degrees the superball can adjust itself
    public float maxMoveInDegrees; 
    public float radius;

    private float debugLineDuration = 15f;
    private Vector3 location, newDirection;
    private Collider[] colliders;
    private RaycastHit hitInfo;
    private SphereCollider sphereCollider;
    private SuperballAIData superballAIData;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        radius = sphereCollider.radius * sphereCollider.transform.localScale.x;
    }

    void OnCollisionExit(Collision collision)
    {
        location = transform.position;
        superballAIData.physicsDirection = GetComponent<Rigidbody>().velocity;
        FindNearestBreakableObject();
        superballAIData.newDirection = (superballAIData.newCollisionPos - location) *superballAIData.physicsDirection.magnitude;
        SuperballAdjustment();
    }

    //This finds the nearest breakable object and stores it in superballAIData
    //The nearest breakable object is not determined by its distance from the 
    //superball, but by the difference between its post-collision quaternion
    //and the angle (ABC) formed by the superball's forward-vector (BA) and the
    //line segment BC formed by the superball's position B and the next breakable
    //object's position C

    //There is no guarantee the nearest breakable object will fall within the
    //range of the superball's "cone of movability"
    private void FindNearestBreakableObject()
    {
        superballAIData.newCollisionPos = Vector3.zero;
        superballAIData.degrees = float.MaxValue;
        superballAIData.newDirection = Vector3.zero;

        float currentCollidersAngle;
        Vector3 currentColliderBestPos;
        Collider colliderWithSmallestAngle = null;
        //1. collect all objects in range and in the collision layer
        colliders = Physics.OverlapSphere(location, 4f, 1 << 8);
        if (colliders.Length == 0) return;
            
        //2. measure the angle between the old and new
        for (int i = 0; i < colliders.Length; i++)
        {
            //2a. if the object can't be reached by a sphere cast, ignore it
            currentColliderBestPos = colliders[i].ClosestPoint(location);
            Physics.SphereCast(location, radius, currentColliderBestPos - location, out hitInfo, 1 << 8);
            if (hitInfo.collider != colliders[i]) { continue; }
            //2b. measure the angle created by 2 vectors: 
            //    i. the direction Unity physics calculates the ball to move,
            //    ii. the direction the ball should move if it wants to hit
            //        the current object we're iterating through
            currentCollidersAngle = Vector3.Angle(superballAIData.physicsDirection, currentColliderBestPos - location);
            if (currentCollidersAngle < superballAIData.degrees)
            {
                colliderWithSmallestAngle = colliders[i];
                superballAIData.degrees = currentCollidersAngle;
                superballAIData.newCollisionPos = currentColliderBestPos;
            }
        }
        if (colliderWithSmallestAngle != null)
        {
            Debug.DrawLine(location, colliderWithSmallestAngle.transform.position, Color.black, 480f);
        }

        superballAIData.newCollisionPos = colliderWithSmallestAngle.transform.position;

    }

    //rotate by maxMoveInDegrees from physicsDirection to newDirection
    private void SuperballAdjustment()
    {
        Vector3 newVelocity = Vector3.zero;
        Vector3 upVector = Vector3.Cross(superballAIData.physicsDirection, superballAIData.newDirection);
        if (upVector.y < 0f)
        {
             upVector *= -1f;
        }

        Quaternion from = Quaternion.LookRotation(superballAIData.physicsDirection, upVector);
        Quaternion to = Quaternion.LookRotation(superballAIData.newDirection, upVector);

        //if the ball can move to meet the object we're interested in, do it
        if (superballAIData.degrees <= maxMoveInDegrees)
        {
            newVelocity = Quaternion.RotateTowards(from, to, superballAIData.degrees) * Vector3.forward;
        }
        //if the ball can't reach the nearest object, rotate it the max degrees so it can at least get CLOSE to it
        else
        {
            newVelocity = Quaternion.RotateTowards(from, to, maxMoveInDegrees) * Vector3.forward;
        }
        
        this.GetComponent<Rigidbody>().velocity = newVelocity.normalized * superballAIData.physicsDirection.magnitude;

        //the line indicating where Unity's physics system wanted us to move
        Debug.DrawLine(transform.position, transform.position + superballAIData.physicsDirection, Color.blue, debugLineDuration);

        //the line indicating which collision our AI wanted to hit
        Debug.DrawLine(transform.position, superballAIData.newCollisionPos, Color.red, debugLineDuration);

        //the line indicating the actual direction the ball will move
        Debug.DrawLine(transform.position, transform.position + this.GetComponent<Rigidbody>().velocity, Color.green, debugLineDuration);
    }

    struct SuperballAIData
    {
        public float degrees;
        public Vector3 newCollisionPos;
        public Vector3 physicsDirection;
        public Vector3 newDirection;

        public SuperballAIData(float _degrees, Vector3 _newCollisionPos, Vector3 _physicsDirection, Vector3 _newDirection)
        {
            degrees = _degrees;
            newCollisionPos = _newCollisionPos;
            physicsDirection = _physicsDirection;
            newDirection = _newDirection;
        }
    }
}
