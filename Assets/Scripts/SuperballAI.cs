using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperballAI : MonoBehaviour {

    public float fieldOfView;
    public float maxMoveInDegrees;
    private float radius;
    private Vector3 location, newDirection;
    private Collider[] colliders;
    private RaycastHit hitInfo;
    private SphereCollider sphereCollider;
    private SuperballAIData superballAIData;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        radius = sphereCollider.radius * sphereCollider.transform.localScale.x;
        Time.timeScale = 0.5f;
    }

    void OnCollisionEnter(Collision collision)
    {
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPaused = true;
//#endif
    }

    void OnCollisionExit(Collision collision)
    {
        location = transform.position;
        superballAIData.physicsDirection = GetComponent<Rigidbody>().velocity;
        FindNearestBreakableObject();
        superballAIData.newDirection = (superballAIData.newCollisionPos - location) *superballAIData.physicsDirection.magnitude;
        SuperballAdjustment();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = true;
#endif
    }

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
        if (colliders.Length == 0) { return; }
            
        //2. measure the angle between the old and new
        for (int i = 0; i < colliders.Length; i++)
        {
            //2a. if the object can't be reached by a sphere cast, ignore it
            currentColliderBestPos = colliders[i].ClosestPoint(location);
            Physics.SphereCast(location, radius, currentColliderBestPos - location, out hitInfo, 1 << 8);
            if (hitInfo.collider != colliders[i]) { continue; }

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
        if (superballAIData.degrees <= maxMoveInDegrees)
        {
            newVelocity = Quaternion.RotateTowards(from, to, superballAIData.degrees) * Vector3.forward;
        }
        else
        {
            newVelocity = Quaternion.RotateTowards(from, to, maxMoveInDegrees) * Vector3.forward;
        }
        this.GetComponent<Rigidbody>().velocity = newVelocity.normalized * superballAIData.physicsDirection.magnitude;

        print("oldVelocity: " + superballAIData.physicsDirection.magnitude 
            + " * " + superballAIData.physicsDirection.normalized + " = " + superballAIData.physicsDirection);
        print("newVelocity: " + GetComponent<Rigidbody>().velocity.magnitude 
            + " * " + GetComponent<Rigidbody>().velocity.normalized + " = " + GetComponent<Rigidbody>().velocity);
        Debug.DrawLine(transform.position, transform.position + superballAIData.physicsDirection, Color.yellow, 480f);
        Debug.DrawLine(transform.position, superballAIData.newCollisionPos, Color.red, 480f);
        Debug.DrawLine(transform.position, transform.position + this.GetComponent<Rigidbody>().velocity, Color.green, 480f);
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
