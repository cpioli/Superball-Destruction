using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperballAI : MonoBehaviour {

    public float fieldOfView;
    public float maximumMovement;
    private float radius;
    private Vector3 location, direction, newDirection;
    private Collider[] colliders;
    private RaycastHit hitInfo;
    private SphereCollider sphereCollider;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        radius = sphereCollider.radius * sphereCollider.transform.localScale.x;

    }

    void OnCollisionExit(Collision collision)
    {
        location = transform.position;
        direction = GetComponent<Rigidbody>().velocity;


        Physics.SphereCast(location, radius, direction, out hitInfo, 10f, 1 << 8);
        //if(hitInfo.collider.gameObject.GetComponent<MirrorBehavior>() == null)
        //{
        newDirection = FindNearestBreakableObject();
        //}
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = true;
#endif
    }

    private Vector3 FindNearestBreakableObject()
    {
        float currentCollidersAngle;
        float smallestAngle = float.MaxValue;
        Vector3 currentColliderBestPos, colliderWithShortestPos; 
        Collider colliderWithSmallestAngle = null;
        //1. collect all objects in range and in the collision layer
        colliders = Physics.OverlapSphere(location, 4f, 1 << 8);
        
        if (colliders.Length == 0)
            return Vector3.zero;
        //2. measure the angle between the old and new
        for (int i = 0; i < colliders.Length; i++)
        {
            //2a. if the object can't be reached by a sphere cast, ignore it
            currentColliderBestPos = colliders[i].ClosestPoint(location);
            Physics.SphereCast(location, radius, currentColliderBestPos - location, out hitInfo, 1 << 8);
            if (hitInfo.collider != colliders[i]) { continue; }

            //Vector3 endPosition = colliders[i].transform.position;
            //Debug.DrawLine(location, endPosition, Color.black, 480f);

            currentCollidersAngle = Vector3.Angle(direction, currentColliderBestPos - location);
            if (currentCollidersAngle < smallestAngle)
            {
                colliderWithSmallestAngle = colliders[i];
                smallestAngle = currentCollidersAngle;
                colliderWithShortestPos = currentColliderBestPos;
            }
        }
        if (colliderWithSmallestAngle != null)
        {
            Debug.DrawLine(location, colliderWithSmallestAngle.transform.position, Color.cyan, 480f);
        }
        return colliderWithSmallestAngle.transform.position;

    }
}
