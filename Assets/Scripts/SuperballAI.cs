using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperballAI : MonoBehaviour {

    public float fieldOfView;
    public float maxMoveInDegrees;
    private float radius;
    private Vector3 location, direction, newDirection;
    private Collider[] colliders;
    private RaycastHit hitInfo;
    private SphereCollider sphereCollider;
    private SuperballAIData newGoal;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        radius = sphereCollider.radius * sphereCollider.transform.localScale.x;

    }

    void OnCollisionExit(Collision collision)
    {
        location = transform.position;
        direction = GameObject.Find("Sphere").GetComponent<Rigidbody>().velocity;
        FindNearestBreakableObject(out newGoal);
        newGoal.newDirection = (newGoal.newCollisionPos - location) * direction.magnitude;
        SuperballAdjustment(newGoal);

        //}
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPaused = true;
//#endif
    }

    private void FindNearestBreakableObject(out SuperballAIData redirection)
    {
        redirection.newCollisionPos = Vector3.zero;
        redirection.degrees = float.MaxValue;
        redirection.oldDirection = GetComponent<Rigidbody>().velocity;
        redirection.newDirection = Vector3.zero;

        Vector3 currentColliderBestPos;
        float currentCollidersAngle;
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

            currentCollidersAngle = Vector3.Angle(direction, currentColliderBestPos - location);
            if (currentCollidersAngle < redirection.degrees)
            {
                colliderWithSmallestAngle = colliders[i];
                redirection.degrees = currentCollidersAngle;
                redirection.newCollisionPos = currentColliderBestPos;
            }
        }
        if (colliderWithSmallestAngle != null)
        {
            Debug.DrawLine(location, colliderWithSmallestAngle.transform.position, Color.black, 480f);
        }

        redirection.newCollisionPos = colliderWithSmallestAngle.transform.position;

    }

    //rotate by maxMoveInDegrees from oldDirection to newDirection
    private void SuperballAdjustment(SuperballAIData superballAIData)
    {
        
        if(superballAIData.degrees <= maxMoveInDegrees)
        {
            this.GetComponent<Rigidbody>().velocity = superballAIData.newDirection;
        }
        else
        {
            Vector3 upVector = Vector3.Cross(superballAIData.oldDirection, superballAIData.newDirection);
            if (upVector.y < 0f)
            {
                upVector *= -1f;
            }

            Quaternion from = Quaternion.LookRotation(superballAIData.oldDirection, upVector);
            Quaternion to = Quaternion.LookRotation(superballAIData.newDirection, upVector);
            Vector3 newVelocity = Quaternion.RotateTowards(from, to, maxMoveInDegrees) * Vector3.forward;
            this.GetComponent<Rigidbody>().velocity = newVelocity.normalized * superballAIData.oldDirection.magnitude;
        }

        Debug.DrawLine(transform.position, transform.position + superballAIData.oldDirection, Color.yellow, 480f);
        Debug.DrawLine(transform.position, superballAIData.newCollisionPos, Color.red, 480f);
        Debug.DrawLine(transform.position, transform.position + this.GetComponent<Rigidbody>().velocity, Color.green, 480f);
    }

    struct SuperballAIData
    {
        public float degrees;
        public Vector3 newCollisionPos;
        public Vector3 oldDirection;
        public Vector3 newDirection;

        public SuperballAIData(float _degrees, Vector3 _newCollisionPos, Vector3 _oldDirection, Vector3 _newDirection)
        {
            degrees = _degrees;
            newCollisionPos = _newCollisionPos;
            oldDirection = _oldDirection;
            newDirection = _newDirection;
        }
    }
}
