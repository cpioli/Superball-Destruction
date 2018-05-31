using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowsBehavior : MonoBehaviour {

    public GameObject ricochetArrow;

    private Vector3 aimArrowDirection, ricochetArrowDirection;
    private RaycastHit hitInfo;
    private SphereCollider sphereCollider;

	// Use this for initialization
	void Start () {
        aimArrowDirection = Vector3.zero;
        ricochetArrowDirection = Vector3.zero;
        sphereCollider = GameObject.Find("Sphere").GetComponent<SphereCollider>();

    }

    public void AlignArrowsForAiming(Vector3 position, Quaternion orientation)
    {
        //step1: figure out how to orient the first arrow (this)
        transform.position = position;
        transform.LookAt(position + orientation * Vector3.up);

        //step2: figure out how to orient the second arrow (ricochet arrow)
        float radius = sphereCollider.radius * sphereCollider.transform.localScale.x;
        Vector3 direction = transform.rotation * Vector3.forward;
        Vector3 origin = sphereCollider.transform.position;
        Physics.SphereCast(origin, radius, direction, out hitInfo, 10f, 1 << 8);
        Vector3 collisionPoint = hitInfo.point;
        Vector3 newDirection = Vector3.Reflect(direction, hitInfo.normal);
        ricochetArrow.transform.position = collisionPoint;
        ricochetArrow.transform.rotation = Quaternion.LookRotation(newDirection);
        if (Input.GetKeyUp(KeyCode.F))
        {
            //Draw the orientation
            Debug.DrawLine(origin, origin + direction, Color.red, 480f);
            Debug.DrawLine(origin, hitInfo.point, Color.green, 480f);
            Debug.DrawLine(collisionPoint, collisionPoint + newDirection, Color.blue, 480f);
        }
        if(Input.GetKeyUp(KeyCode.Z))
        {
            DrawDirectionalLines(origin);
        }
    }

    public void DeactivateArrows()
    {
        this.GetComponent<MeshRenderer>().enabled = false;
        ricochetArrow.GetComponent<MeshRenderer>().enabled = false;
    }

    public void ActivateArrows()
    {
        this.GetComponent<MeshRenderer>().enabled = true;
        ricochetArrow.GetComponent<MeshRenderer>().enabled = true;
    }

    public void DrawDirectionalLines(Vector3 origin)
    {
        Debug.DrawRay(origin, transform.rotation * Vector3.up, Color.gray, 480f);
        Debug.DrawRay(origin, transform.rotation * Vector3.down, Color.white, 480f);
        Debug.DrawRay(origin, transform.rotation * Vector3.forward, Color.red, 480f);
        Debug.DrawRay(origin, transform.rotation * Vector3.back, Color.blue, 480f);
        Debug.DrawRay(origin, transform.rotation * Vector3.left, Color.green, 480f);
        Debug.DrawRay(origin, transform.rotation * Vector3.right, Color.yellow, 480f);
    }
}
